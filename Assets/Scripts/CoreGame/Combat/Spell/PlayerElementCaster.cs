using System;
using UnityEngine;

/// <summary>Owns mana, per-element cooldowns and the equipped element; each element's skill is cast by its ISpellLauncher.</summary>
public class PlayerElementCaster : MonoBehaviour
{
    private const ElementType FirstCastable = ElementType.Wind;
    private static readonly int ElementCount = Enum.GetValues(typeof(ElementType)).Length;
    private static readonly int CastableCount = ElementCount - (int)FirstCastable;

    [Header("Elements")]
    [SerializeField] private ElementType currentElement = ElementType.Fire;

    [Header("Data")]
    [SerializeField] private SO_ElementCasterData casterData;

    [Header("Debug")]
    [SerializeField] private bool debugFreeCast;

    [Header("Fireball")]
    [SerializeField] private SO_FireballData fireballData;
    [SerializeField] private Fireball fireballPrefab;

    [Header("Water Ring")]
    [SerializeField] private SO_WaterRingData waterRingData;
    [SerializeField] private WaterRingView waterRingViewPrefab;

    [Header("Stone Burst")]
    [SerializeField] private SO_StoneBurstData stoneBurstData;
    [SerializeField] private StoneBurstView stoneBurstViewPrefab;

    [Header("Wind Vortex")]
    [SerializeField] private SO_WindVortexData vortexData;
    [SerializeField] private WindVortex vortexPrefab;

    [Header("Targets")]
    [SerializeField] private LayerMask spellHitLayer = 1 << 8;

    private readonly float[] cooldownRemaining = new float[ElementCount];
    private readonly ISpellLauncher[] launchers = new ISpellLauncher[ElementCount];

    private CharacterStateBase state;
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
        this.combat = combat;

        currentMana = MaxMana;
        BuildLaunchers(character);

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
        float deltaTime = Time.deltaTime;

        for (int i = 0; i < ElementCount; i++)
        {
            if (cooldownRemaining[i] > 0f)
                cooldownRemaining[i] = Mathf.Max(0f, cooldownRemaining[i] - deltaTime);
        }

        if (currentWasCooling)
            RaiseCooldownChanged();
    }

    /// <summary>Moves to the next (step 1) or previous (step -1) castable element in ElementType order, skipping None and wrapping at both ends.</summary>
    public void CycleElement(int step)
    {
        if (step == 0) return;

        currentElement = NextCastable(currentElement, step);
        GameLog.Info($"[Phép] Đổi nguyên tố: {currentElement}");
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

        int slot = (int)currentElement;
        if (cooldownRemaining[slot] > 0f)
        {
            GameLog.Info($"[Phép] {currentElement} đang hồi: còn {cooldownRemaining[slot]:0.0}s");
            return;
        }

        if (!debugFreeCast && currentMana < casterData.skillCost)
        {
            GameLog.Info($"[Phép] Thiếu mana: {currentMana:0}/{casterData.skillCost:0}");
            return;
        }

        ISpellLauncher launcher = launchers[slot];
        if (launcher == null || !launcher.TryLaunch()) return;

        if (!debugFreeCast)
            SetMana(currentMana - casterData.skillCost);
        cooldownRemaining[slot] = casterData.GetCooldown(currentElement);
        RaiseCooldownChanged();
    }

    /// <summary>Hold of the cast button: imbues the main weapon, which needs and spends a full mana bar.</summary>
    public void Imbue()
    {
        if (!CanCast() || casterData == null) return;

        if (!debugFreeCast && currentMana < casterData.maxMana)
        {
            GameLog.Info($"[Phép] Yểm cần đầy mana: {currentMana:0}/{casterData.maxMana:0}");
            return;
        }

        if (!debugFreeCast)
            SetMana(0f);
        GameLog.Info($"[Phép] Yểm {currentElement} lên vũ khí (chưa làm)");
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

    private void BuildLaunchers(CharacterControllerBase character)
    {
        SpellCastContext context = new SpellCastContext(
            transform, character, state, combat, spellHitLayer, destroyCancellationToken);

        Array.Clear(launchers, 0, launchers.Length);
        Register(new VortexLauncher(vortexData, vortexPrefab, context));
        Register(new FireballLauncher(fireballData, fireballPrefab, context));
        Register(new WaterRingLauncher(waterRingData, waterRingViewPrefab, context));
        Register(new StoneBurstLauncher(stoneBurstData, stoneBurstViewPrefab, context));
    }

    private void Register(ISpellLauncher launcher) => launchers[(int)launcher.Element] = launcher;

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
        return !state.IsDead && !state.IsRolling && !state.IsAttacking && !IsAnyLauncherBusy();
    }

    private bool IsAnyLauncherBusy()
    {
        for (int i = 0; i < launchers.Length; i++)
        {
            if (launchers[i] != null && launchers[i].IsBusy) return true;
        }

        return false;
    }
}
