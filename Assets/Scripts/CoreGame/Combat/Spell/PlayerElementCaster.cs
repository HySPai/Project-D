using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class PlayerElementCaster : MonoBehaviour
{
    private const ElementType FirstCastable = ElementType.Wind;
    private const int HitBufferSize = 16;
    private static readonly int ElementCount = Enum.GetValues(typeof(ElementType)).Length;
    private static readonly int CastableCount = ElementCount - (int)FirstCastable;

    [Header("Elements")]
    [SerializeField] private ElementType currentElement = ElementType.Fire;

    [Header("Data")]
    [SerializeField] private SO_ElementCasterData casterData;

    [Header("Fireball")]
    [SerializeField] private SO_FireballData fireballData;
    [SerializeField] private Fireball fireballPrefab;

    [Header("Water Ring")]
    [SerializeField] private SO_WaterRingData waterRingData;

    [Header("Stone Burst")]
    [SerializeField] private SO_StoneBurstData stoneBurstData;

    [Header("Wind Vortex")]
    [SerializeField] private SO_WindVortexData vortexData;
    [SerializeField] private WindVortex vortexPrefab;

    [Header("Targets")]
    [SerializeField] private LayerMask spellHitLayer = 1 << 8;

    private readonly List<Fireball> fireballPool = new List<Fireball>();
    private readonly List<WindVortex> vortexPool = new List<WindVortex>();
    private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
    private readonly List<CharacterControllerBase> spellTargets = new List<CharacterControllerBase>();
    private bool isWindingUp;
    private readonly float[] cooldownRemaining = new float[ElementCount];

    private CharacterStateBase state;
    private CharacterControllerBase character;
    private PlayerCombat combat;
    private float currentMana;

    public ElementType CurrentElement => currentElement;
    public float CurrentMana => currentMana;
    public float MaxMana => casterData != null ? casterData.maxMana : 0f;

    public event Action<ElementType> OnElementChanged;
    public event Action<float, float> OnManaChanged;
    public event Action<float> OnCooldownChanged;

    public void Initialize(CharacterStateBase state, CharacterControllerBase character, PlayerCombat combat)
    {
        this.state = state;
        this.character = character;
        this.combat = combat;

        currentMana = MaxMana;

        if (combat != null)
            combat.OnWeaponHit += ChargeFromHit;
    }

    private void OnDestroy()
    {
        if (combat != null)
            combat.OnWeaponHit -= ChargeFromHit;
    }

    private void Update()
    {
        bool currentWasCooling = cooldownRemaining[(int)currentElement] > 0f;

        for (int i = 0; i < ElementCount; i++)
        {
            if (cooldownRemaining[i] > 0f)
                cooldownRemaining[i] = Mathf.Max(0f, cooldownRemaining[i] - Time.deltaTime);
        }

        if (currentWasCooling)
            RaiseCooldownChanged();
    }

    /// <summary>Moves to the next (step 1) or previous (step -1) castable element in ElementType order, skipping None and wrapping at both ends.</summary>
    public void CycleElement(int step)
    {
        if (step == 0) return;

        currentElement = NextCastable(currentElement, step);
        Debug.Log($"[Phép] Đổi nguyên tố: {currentElement}");
        OnElementChanged?.Invoke(currentElement);
        RaiseCooldownChanged();
    }

    /// <summary>The castable element step places away from current, never None; a None input counts as the slot before the first castable.</summary>
    public static ElementType NextCastable(ElementType current, int step)
    {
        int index = Mathf.Max(-1, (int)current - (int)FirstCastable);
        int next = ((index + step) % CastableCount + CastableCount) % CastableCount;
        return (ElementType)(next + (int)FirstCastable);
    }

    /// <summary>Tap of the cast button: fires the combat skill of the equipped element if mana and cooldown allow.</summary>
    public void CastSkill()
    {
        if (!CanCast() || casterData == null) return;

        if (cooldownRemaining[(int)currentElement] > 0f)
        {
            Debug.Log($"[Phép] {currentElement} đang hồi: còn {cooldownRemaining[(int)currentElement]:0.0}s");
            return;
        }

        if (currentMana < casterData.skillCost)
        {
            Debug.Log($"[Phép] Thiếu mana: {currentMana:0}/{casterData.skillCost:0}");
            return;
        }

        if (!LaunchSkill(currentElement)) return;

        SetMana(currentMana - casterData.skillCost);
        cooldownRemaining[(int)currentElement] = casterData.GetCooldown(currentElement);
        RaiseCooldownChanged();
    }

    /// <summary>Hold of the cast button: imbues the main weapon, which needs and spends a full mana bar.</summary>
    public void Imbue()
    {
        if (!CanCast() || casterData == null) return;

        if (currentMana < casterData.maxMana)
        {
            Debug.Log($"[Phép] Yểm cần đầy mana: {currentMana:0}/{casterData.maxMana:0}");
            return;
        }

        SetMana(0f);
        Debug.Log($"[Phép] Yểm {currentElement} lên vũ khí (chưa làm)");
    }

    /// <summary>Fills mana and clears every cooldown, as resting at a rest point or respawning does.</summary>
    public void RestoreMana()
    {
        for (int i = 0; i < ElementCount; i++)
            cooldownRemaining[i] = 0f;

        SetMana(MaxMana);
        RaiseCooldownChanged();
    }

    /// <summary>Remaining cooldown of the equipped element, 1 just cast and 0 ready.</summary>
    public float GetCurrentCooldown01()
    {
        if (casterData == null) return 0f;

        float total = casterData.GetCooldown(currentElement);
        return total > 0f ? cooldownRemaining[(int)currentElement] / total : 0f;
    }

    private void ChargeFromHit(float damage)
    {
        if (casterData == null) return;

        SetMana(currentMana + casterData.GetCharge(damage));
    }

    private void SetMana(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, MaxMana);
        if (Mathf.Approximately(clamped, currentMana)) return;

        currentMana = clamped;
        OnManaChanged?.Invoke(currentMana, MaxMana);
    }

    private void RaiseCooldownChanged() => OnCooldownChanged?.Invoke(GetCurrentCooldown01());

    private bool CanCast()
    {
        if (state == null) return false;
        return !state.IsDead && !state.IsRolling && !state.IsAttacking && !isWindingUp;
    }

    private bool LaunchSkill(ElementType element)
    {
        switch (element)
        {
            case ElementType.Wind:
                return LaunchVortex();
            case ElementType.Fire:
                return LaunchFireball();
            case ElementType.Water:
                return LaunchWaterRing();
            case ElementType.Stone:
                return LaunchStoneBurst();
            default:
                return false;
        }
    }

    private bool LaunchWaterRing()
    {
        if (waterRingData == null) return false;

        Vector3 center = transform.position;
        SpellTargeting.CollectTargets(center, waterRingData.radius, spellHitLayer, character, hitBuffer, spellTargets);

        foreach (CharacterControllerBase target in spellTargets)
        {
            CharacterStateBase targetState = target.GetState;
            DamageInfo info = new DamageInfo(
                waterRingData.damage, character, center, ElementType.Water, DamageSource.Spell, false, 0f);
            targetState.ReceiveDamage(in info);

            if (targetState.IsDead) continue;

            if (target is EnemyController enemy)
                enemy.PushAway(center, waterRingData.pushDistance);

            if (target.GetSlow != null)
                target.GetSlow.Apply();
        }

        Debug.Log($"[Phép] Vòng Nước trúng {spellTargets.Count} mục tiêu");
        return true;
    }

    private bool LaunchStoneBurst()
    {
        if (stoneBurstData == null) return false;

        Vector3 direction = GetCastDirection();
        transform.rotation = Quaternion.LookRotation(direction);

        StoneBurstAsync(direction).Forget();
        return true;
    }

    private async UniTaskVoid StoneBurstAsync(Vector3 direction)
    {
        isWindingUp = true;

        bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(stoneBurstData.windup),
                cancellationToken: this.GetCancellationTokenOnDestroy())
            .SuppressCancellationThrow();

        isWindingUp = false;
        if (canceled) return;

        if (state == null || state.IsDead || state.IsRolling)
        {
            Debug.Log("[Phép] Đá Văng bị hủy giữa lúc khởi động");
            return;
        }

        ReleaseStoneBurst(direction);
    }

    private void ReleaseStoneBurst(Vector3 direction)
    {
        Vector3 origin = transform.position;
        SpellTargeting.CollectTargets(origin, stoneBurstData.range, spellHitLayer, character, hitBuffer, spellTargets);

        int hits = 0;
        foreach (CharacterControllerBase target in spellTargets)
        {
            if (!SpellTargeting.IsWithinArc(origin, direction, target.transform.position, stoneBurstData.arc)) continue;

            CharacterStateBase targetState = target.GetState;
            DamageInfo info = new DamageInfo(
                stoneBurstData.damage, character, origin, ElementType.Stone, DamageSource.Spell, false, 0f);
            targetState.ReceiveDamage(in info);
            hits++;

            if (!targetState.IsDead && target is EnemyController enemy)
                enemy.ApplyStun(stoneBurstData.stunDuration);
        }

        Debug.Log($"[Phép] Đá Văng trúng {hits} mục tiêu");
    }

    private bool LaunchVortex()
    {
        if (vortexData == null || vortexPrefab == null) return false;

        Vector3 direction = GetCastDirection();
        transform.rotation = Quaternion.LookRotation(direction);

        Vector3 position = ResolveVortexPosition(transform.position, direction * vortexData.castDistance);
        GetFreeVortex().Launch(vortexData, character, position);
        Debug.Log($"[Phép] Lốc Xoáy tại {position}");
        return true;
    }

    private static Vector3 ResolveVortexPosition(Vector3 origin, Vector3 offset)
    {
        Vector3 desired = origin + offset;

        if (!NavMesh.SamplePosition(origin, out NavMeshHit start, 2f, NavMesh.AllAreas))
            return desired;

        return NavMesh.Raycast(start.position, desired, out NavMeshHit hit, NavMesh.AllAreas)
            ? hit.position
            : desired;
    }

    private WindVortex GetFreeVortex()
    {
        foreach (WindVortex vortex in vortexPool)
        {
            if (!vortex.gameObject.activeSelf)
                return vortex;
        }

        WindVortex created = Instantiate(vortexPrefab);
        created.gameObject.SetActive(false);
        vortexPool.Add(created);
        return created;
    }

    private bool LaunchFireball()
    {
        if (fireballData == null || fireballPrefab == null) return false;

        Vector3 direction = GetCastDirection();
        transform.rotation = Quaternion.LookRotation(direction);

        Vector3 position = transform.position
                           + Vector3.up * fireballData.spawnHeight
                           + direction * fireballData.spawnForward;

        GetFreeFireball().Launch(fireballData, character, position, direction);
        Debug.Log("[Phép] Cầu Lửa");
        return true;
    }

    private Vector3 GetCastDirection()
    {
        Transform target = combat != null ? combat.LockOnTransform : null;
        Vector3 direction = target != null ? target.position - transform.position : transform.forward;
        direction.y = 0f;

        return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
    }

    private Fireball GetFreeFireball()
    {
        foreach (Fireball fireball in fireballPool)
        {
            if (!fireball.gameObject.activeSelf)
                return fireball;
        }

        Fireball created = Instantiate(fireballPrefab);
        created.gameObject.SetActive(false);
        fireballPool.Add(created);
        return created;
    }
}
