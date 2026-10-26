using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    private const string LitFlagPrefix = "rest.";

    [Serializable]
    private struct BossEntry
    {
        public string flagId;
        public EnemyController boss;
    }

    [SerializeField] private PlayerController player;
    [SerializeField] private WorldResetController worldReset;
    [SerializeField] private AreaTransitionController areaTransition;
    [SerializeField] private DayNightCycle dayNight;
    [SerializeField] private BlackoutFade blackout;
    [SerializeField] private RestPoint[] restPoints;
    [SerializeField] private EnemyController[] enemies;
    [SerializeField] private BossEntry[] bosses;
    [SerializeField] private WeaponItem[] weapons;

    [Header("Slot")]
    [Range(0, SaveService.SlotCount - 1)]
    [SerializeField] private int slot;
    [SerializeField] private bool loadOnStart = true;

    [Header("Rest")]
    [SerializeField] private float restBlockRadius = 10f;
    [SerializeField] private float wakeHour = 9f;
    [SerializeField] private float restDurationSeconds = 2f;
    [SerializeField] private float blackHoldSeconds = 0.2f;

    private SaveService service;
    private SaveData data = new SaveData();
    private Action[] bossHandlers;
    private RestPoint focus;
    private bool resting;
    private bool applyingLoad;
    private bool savingBlocked;
    private bool writeFailureReported;

    public PlayerController Player => player;
    public SaveData Data => data;
    public bool IsResting => resting;

    private void Awake() => service = new SaveService(new FileSaveStorage());

    private void Start()
    {
        player.GetPlayerCombat.OnWeaponEquipped += HandleWeaponEquipped;

        bossHandlers = new Action[bosses.Length];
        for (int i = 0; i < bosses.Length; i++)
        {
            string flagId = bosses[i].flagId;
            bossHandlers[i] = () => HandleBossDied(flagId);
            bosses[i].boss.GetState.OnDied += bossHandlers[i];
        }

        if (loadOnStart) Load();
    }

    private void OnDestroy()
    {
        if (player != null) player.GetPlayerCombat.OnWeaponEquipped -= HandleWeaponEquipped;
        if (bossHandlers == null) return;

        for (int i = 0; i < bosses.Length; i++)
        {
            if (bosses[i].boss != null) bosses[i].boss.GetState.OnDied -= bossHandlers[i];
        }
    }

    private void Update()
    {
        data.playTimeSeconds += Time.unscaledDeltaTime;

        bool pressed = player.GetInput.ConsumeInteractPressed();
        if (pressed && focus != null && !resting) TryInteract(focus);
    }

    public void SetFocus(RestPoint point) => focus = point;

    public void ClearFocus(RestPoint point)
    {
        if (focus == point) focus = null;
    }

    /// <summary>Brings the player back at the last rest point, or the starting spot, with the world reset around them.</summary>
    public void Respawn()
    {
        Vector3 previousPosition = player.transform.position;
        RestPoint rest = FindRestPoint(data.lastRestPointId);

        if (rest != null) player.SetSpawnPoint(rest.SpawnPoint.position, rest.SpawnPoint.rotation);

        worldReset.ResetWorld();
        areaTransition.SnapToRoom(rest != null ? rest.Room : areaTransition.StartRoom, previousPosition);
        ApplyDefeatedBosses();
    }

    [Button]
    public void SaveNow() => Save();

    [Button]
    public void DeleteSlot()
    {
        service.Delete(slot);
        data = new SaveData();
        savingBlocked = false;
    }

    private void Load()
    {
        SaveLoadResult result = service.Load(slot);

        switch (result.status)
        {
            case SaveLoadStatus.Empty:
                return;
            case SaveLoadStatus.TooNew:
                savingBlocked = true;
                Debug.LogError($"Save slot {slot} was written by a newer version of the game and was not loaded.", this);
                return;
            case SaveLoadStatus.Corrupt:
                savingBlocked = true;
                Debug.LogError($"Save slot {slot} is damaged and was left untouched.", this);
                return;
            case SaveLoadStatus.RecoveredFromBackup:
                Debug.LogWarning($"Save slot {slot} was restored from its backup; recent progress may be lost.", this);
                break;
        }

        data = result.data;
        ApplyLoadedData();
    }

    private void ApplyLoadedData()
    {
        applyingLoad = true;

        WeaponItem weapon = FindWeapon(data.equippedWeaponId);
        if (weapon != null) player.GetPlayerCombat.EquipWeapon(weapon);

        for (int i = 0; i < restPoints.Length; i++)
            restPoints[i].SetLit(data.HasFlag(LitFlag(restPoints[i].Id)), false);

        dayNight.SetHour(data.hourOfDay);
        Respawn();
        applyingLoad = false;
    }

    private void TryInteract(RestPoint point)
    {
        PlayerState state = player.GetPlayerState;
        if (state.IsDead || state.IsAttacking || state.IsRolling) return;

        if (IsBeingHunted())
        {
            point.PlayRefused();
            return;
        }

        if (data.HasFlag(LitFlag(point.Id)))
            RestAsync(point).Forget();
        else
            Light(point);
    }

    private void Light(RestPoint point)
    {
        data.AddFlag(LitFlag(point.Id));
        point.SetLit(true, true);

        data.lastRestPointId = point.Id;
        player.SetSpawnPoint(point.SpawnPoint.position, point.SpawnPoint.rotation);
        Save();
    }

    private async UniTaskVoid RestAsync(RestPoint point)
    {
        CancellationToken token = this.GetCancellationTokenOnDestroy();

        resting = true;
        player.SetControlLocked(true);
        dayNight.SetPaused(true);

        float startHour = dayNight.Hour;
        DayNightMath.PlanRest(startHour, wakeHour, out float darkenHours, out float brightenHours);
        float phaseSeconds = Mathf.Max(0f, restDurationSeconds - blackHoldSeconds) * 0.5f;

        await Lapse(startHour, darkenHours, true, phaseSeconds, token);

        CompleteRest(point);

        bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(blackHoldSeconds), DelayType.UnscaledDeltaTime, cancellationToken: token)
            .SuppressCancellationThrow();
        if (canceled) return;

        float brightenFrom = darkenHours > 0f ? 0f : startHour;
        await Lapse(brightenFrom, brightenHours, false, phaseSeconds, token);

        dayNight.SetHour(wakeHour);
        dayNight.SetPaused(false);
        player.SetControlLocked(false);
        resting = false;
        Save();
    }

    private async UniTask Lapse(float fromHour, float hours, bool darkening, float duration, CancellationToken token)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            dayNight.SetHour(fromHour + hours * t);
            float darkness = DayNightMath.Smooth(t);
            blackout.SetBlackness(darkening ? darkness : 1f - darkness);

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        blackout.SetBlackness(darkening ? 1f : 0f);
    }

    private void CompleteRest(RestPoint point)
    {
        Vector3 previousPosition = player.transform.position;

        player.GetPlayerState.RestoreVitals();
        player.Teleport(point.SpawnPoint.position, point.SpawnPoint.rotation);
        player.SetSpawnPoint(point.SpawnPoint.position, point.SpawnPoint.rotation);

        worldReset.ResetWorld(false);
        areaTransition.SnapToRoom(point.Room, previousPosition);
        ApplyDefeatedBosses();

        data.lastRestPointId = point.Id;
    }

    private bool IsBeingHunted()
    {
        float sqrRadius = restBlockRadius * restBlockRadius;
        Vector3 origin = player.transform.position;

        for (int i = 0; i < enemies.Length; i++)
        {
            if (!enemies[i].isActiveAndEnabled || !enemies[i].IsPursuing) continue;
            if ((enemies[i].transform.position - origin).sqrMagnitude <= sqrRadius) return true;
        }

        return false;
    }

    private void ApplyDefeatedBosses()
    {
        for (int i = 0; i < bosses.Length; i++)
        {
            if (data.HasFlag(bosses[i].flagId)) bosses[i].boss.gameObject.SetActive(false);
        }
    }

    private void HandleBossDied(string flagId)
    {
        if (!data.AddFlag(flagId)) return;

        Save();
    }

    private void HandleWeaponEquipped(WeaponItem weapon)
    {
        if (applyingLoad) return;

        data.equippedWeaponId = weapon.name;
        Save();
    }

    private void Save()
    {
        if (savingBlocked) return;

        data.hourOfDay = dayNight.Hour;
        data.savedAtUtc = DateTime.UtcNow.ToString("o");
        if (service.Save(slot, data))
        {
            writeFailureReported = false;
            return;
        }

        if (writeFailureReported) return;

        writeFailureReported = true;
        Debug.LogWarning($"Could not write save slot {slot}; the game will retry on the next save.", this);
    }

    private RestPoint FindRestPoint(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        for (int i = 0; i < restPoints.Length; i++)
        {
            if (restPoints[i].Id == id) return restPoints[i];
        }

        return null;
    }

    private WeaponItem FindWeapon(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].name == id) return weapons[i];
        }

        return null;
    }

    private static string LitFlag(string restPointId) => LitFlagPrefix + restPointId;
}
