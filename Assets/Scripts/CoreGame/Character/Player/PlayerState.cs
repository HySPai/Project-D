using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class PlayerState : CharacterStateBase
{
    private SO_PlayerStats playerStats;

    #region Hearts
    public event Action<int, int> OnHeartsChanged;

    private int currentHearts;
    public int CurrentHearts => currentHearts;
    public int MaxHearts => playerStats.maxHearts;

    private void RaiseHeartsChanged() => OnHeartsChanged?.Invoke(currentHearts, playerStats.maxHearts);

    private const float MinPointsPerHeart = 0.01f;

    private float damageRemainder;

    [Button]
    public void DebugReceiveDamage(float points = 25f)
    {
        DamageInfo info = new DamageInfo(
            points, null, transform.position + transform.forward,
            ElementType.None, DamageSource.Weapon, false, 0f);
        ReceiveDamage(in info);
    }

    private PlayerGuard guard;
    private GamepadRumble rumble;

    public override void Initialize(CharacterControllerBase owner)
    {
        base.Initialize(owner);
        guard = (owner as PlayerController)?.GetGuard;
        rumble = (owner as PlayerController)?.GetRumble;
    }

    private void PlayDamageRumble()
    {
        if (rumble != null)
            rumble.PlayDamageTaken(currentHearts <= 0);
    }

    protected override bool AbsorbIncoming(ref DamageInfo info, out DamageResult absorbed)
    {
        absorbed = default;
        return guard != null && guard.TryAbsorb(ref info, out absorbed);
    }

    protected override void PlayHitReaction(in DamageInfo info)
    {
        if (guard != null && guard.IsBroken) return;
        base.PlayHitReaction(in info);
    }

    public bool ApplyChipDamage(float points)
    {
        if (points <= 0f || isDead) return false;

        ReduceHearts(points);
        PlayDamageFeedback();
        PlayDamageRumble();

        if (currentHearts > 0) return false;

        Die();
        return true;
    }

    private void ReduceHearts(float points)
    {
        float pointsPerHeart = Mathf.Max(MinPointsPerHeart, playerStats.pointsPerHeart);
        damageRemainder += points;
        int hearts = Mathf.FloorToInt(damageRemainder / pointsPerHeart);
        damageRemainder -= hearts * pointsPerHeart;

        if (hearts <= 0) return;

        currentHearts = Mathf.Max(0, currentHearts - hearts);
        currentHp = currentHearts;
        RaiseHeartsChanged();
        RaiseHpChanged();
    }

    protected override DamageResult ApplyDamage(in DamageInfo info)
    {
        if (info.points <= 0f) return default;

        ReduceHearts(info.points);

        InterruptAttack();
        PlayDamageFeedback();
        PlayDamageRumble();

        if (currentHearts <= 0)
        {
            Die();
            return new DamageResult(info.points, false, false, true);
        }

        PlayHitReaction(in info);
        GrantInvulnerability(playerStats.invulnerabilityDuration);
        return new DamageResult(info.points, false, false, false);
    }

    private void InterruptAttack()
    {
        var combat = owner != null ? owner.GetCombat : null;
        if (combat != null)
            combat.DisableAllDamageColliders();
    }

    [Button]
    public int Heal(int amount)
    {
        if (amount <= 0 || isDead) return 0;

        damageRemainder = 0f;
        int before = currentHearts;
        currentHearts = Mathf.Min(playerStats.maxHearts, currentHearts + amount);
        int healed = currentHearts - before;

        if (healed > 0)
        {
            currentHp = currentHearts;
            RaiseHeartsChanged();
            RaiseHpChanged();
        }

        return healed;
    }
    #endregion

    #region Stamina
    [Header("Stamina (runtime)")]
    [SerializeField] protected float currentStamina;
    protected float lastStaminaUseTime;
    private bool runExhausted;

    public event Action<float, float> OnStaminaChanged;

    public float Stamina => playerStats.maxStamina;
    public float CurrentStamina => currentStamina;
    public bool HasStamina => currentStamina > 0f;
    public bool HasEnoughStamina(float amount) => currentStamina >= amount;

    public float RollStaminaCost => playerStats.rollStaminaCost;
    public float BackstepStaminaCost => playerStats.backstepStaminaCost;
    public float RunStaminaDrainRate => playerStats.runStaminaDrainRate;

    public float RollCancelNormalizedTime => playerStats.rollCancelNormalizedTime;
    public float RollCancelBlendTime => playerStats.rollCancelBlendTime;
    public float RollExitSpeedRatio => playerStats.rollExitSpeedRatio;

    public bool CanRun
    {
        get
        {
            if (!isCombatStance) return true;
            if (currentStamina <= 0f) runExhausted = true;
            else if (currentStamina >= playerStats.runResumeStamina) runExhausted = false;
            return !runExhausted;
        }
    }

    private void RaiseStaminaChanged() => OnStaminaChanged?.Invoke(currentStamina, playerStats.maxStamina);

    public void DrainStamina(float amount)
    {
        if (amount <= 0f) return;
        currentStamina = Mathf.Max(0f, currentStamina - amount);
        lastStaminaUseTime = Time.time;
        RaiseStaminaChanged();
    }

    protected virtual void RegenerateStamina()
    {
        if (playerStats == null) return;
        if (guard != null && guard.IsGuardActive) return;
        if (currentStamina >= playerStats.maxStamina) return;
        if (Time.time - lastStaminaUseTime < playerStats.staminaRegenDelay) return;

        currentStamina = Mathf.Min(playerStats.maxStamina,
            currentStamina + playerStats.staminaRegenRate * Time.deltaTime);
        RaiseStaminaChanged();
    }
    #endregion

    #region Stance
    private bool isCombatStance;

    /// <summary>True while the sword and shield are drawn; false while exploring.</summary>
    public bool IsCombatStance => isCombatStance;

    public void SetCombatStance(bool value) => isCombatStance = value;
    #endregion

    #region Ground Check Extra
    [Header("Ground Check")]
    [SerializeField] protected float groundCheckDistance = 5f;
    [SerializeField] protected float edgeCheckForwardDistance = 0.5f;

    public float GroundCheckDistance => groundCheckDistance;
    public float EdgeCheckForwardDistance => edgeCheckForwardDistance;
    #endregion

    #region Input
    [Header("Input")]
    [SerializeField] private float fullSpeedInputThreshold = 0.7f;
    [SerializeField] private Vector2 currentInput;
    [SerializeField] private Vector2 currentCameraInput;

    public float FullSpeedInputThreshold => fullSpeedInputThreshold;
    public Vector2 CurrentInput => currentInput;
    public Vector2 CurrentCameraInput => currentCameraInput;

    public void SetCurrentInput(Vector2 value) => currentInput = value;
    public void SetCurrentCameraInput(Vector2 value) => currentCameraInput = value;
    #endregion

    #region Water
    [Header("Water")]
    [SerializeField] private LayerMask waterLayer;

    private readonly Collider[] waterHits = new Collider[4];
    private int waterSampleFrame = -1;
    private float waterDepth;

    /// <summary>True while the feet are inside a water volume (wading or deeper).</summary>
    public bool IsInWater => WaterDepth > 0f;

    /// <summary>How far the water surface sits above the feet, in metres; 0 on dry ground. Sampled once per frame.</summary>
    public float WaterDepth
    {
        get
        {
            if (waterSampleFrame == Time.frameCount) return waterDepth;
            waterSampleFrame = Time.frameCount;

            Vector3 feet = transform.position;
            int count = Physics.OverlapSphereNonAlloc(
                feet + Vector3.up * playerStats.waterCheckHeight, playerStats.waterCheckRadius,
                waterHits, waterLayer, QueryTriggerInteraction.Collide);

            waterDepth = 0f;
            for (int i = 0; i < count; i++)
                waterDepth = Mathf.Max(waterDepth, waterHits[i].bounds.max.y - feet.y);
            return waterDepth;
        }
    }

    /// <summary>Speed multiplier while wading: 1 on dry ground, easing down to the minimum at full slow depth.</summary>
    public float WadeSpeedMultiplier =>
        GetWadeSpeedMultiplier(WaterDepth, playerStats.waterFullSlowDepth, playerStats.waterMinSpeedMultiplier);

    /// <summary>Linear slow-down from 1 at depth 0 to minMultiplier at fullSlowDepth and beyond.</summary>
    public static float GetWadeSpeedMultiplier(float depth, float fullSlowDepth, float minMultiplier)
    {
        if (depth <= 0f) return 1f;
        float t = fullSlowDepth > 0f ? Mathf.Clamp01(depth / fullSlowDepth) : 1f;
        return Mathf.Lerp(1f, minMultiplier, t);
    }
    #endregion

    #region Locomotion
    public float MoveInputDeadzone => playerStats.moveInputDeadzone;
    public float MoveAnimSpeedThreshold => playerStats.moveAnimSpeedThreshold;

    /// <summary>Stick magnitude with the deadzone removed and rescaled, then mapped so FullSpeedInputThreshold reaches 1.</summary>
    public float GetMoveAmount(Vector2 input)
    {
        float deadzone = playerStats.moveInputDeadzone;
        float magnitude = input.magnitude;
        if (magnitude <= deadzone) return 0f;

        float live = (magnitude - deadzone) / Mathf.Max(0.0001f, 1f - deadzone);
        float fullSpeed = (fullSpeedInputThreshold - deadzone) / Mathf.Max(0.0001f, 1f - deadzone);
        return Mathf.Clamp01(live / Mathf.Max(0.0001f, fullSpeed));
    }

    /// <summary>Locomotion blend value from the real speed: 0 idle, 1 at walk speed, 2 at run speed.</summary>
    public float AnimationMoveAmount
    {
        get
        {
            float speed = currentMoveSpeed;
            if (speed <= playerStats.moveAnimSpeedThreshold) return 0f;
            if (speed <= moveSpeed) return speed / Mathf.Max(0.0001f, moveSpeed);
            return 1f + Mathf.Clamp01((speed - moveSpeed) / Mathf.Max(0.0001f, runSpeed - moveSpeed));
        }
    }
    #endregion

    #region Lifecycle
    /// <summary>Fills hearts and stamina without moving or reviving the character, as resting at a rest point does.</summary>
    public void RestoreVitals()
    {
        if (isDead) return;

        damageRemainder = 0f;
        currentHearts = playerStats.maxHearts;
        currentHp = currentHearts;
        currentStamina = playerStats.maxStamina;
        runExhausted = false;

        RaiseHeartsChanged();
        RaiseHpChanged();
        RaiseStaminaChanged();
    }

    protected override void InitializeStats()
    {
        base.InitializeStats();

        playerStats = stats as SO_PlayerStats;
        if (playerStats == null)
        {
            Debug.LogError($"{name}: stats phải là SO_PlayerStats", this);
            return;
        }

        currentStamina = playerStats.maxStamina;
        currentHearts = playerStats.maxHearts;
        maxHp = playerStats.maxHearts;
    }

    protected virtual void Update()
    {
        RegenerateStamina();
    }

    protected override void Die()
    {
        base.Die();
        GameEvent.OnPlayerDied?.Invoke();
    }

    public override void ResetState()
    {
        base.ResetState();

        damageRemainder = 0f;
        currentHearts = playerStats.maxHearts;
        currentHp = currentHearts;

        currentStamina = playerStats.maxStamina;
        lastStaminaUseTime = 0f;
        runExhausted = false;

        currentInput = Vector2.zero;
        currentCameraInput = Vector2.zero;

        RaiseHeartsChanged();
        RaiseHpChanged();
        RaiseStaminaChanged();
    }
    #endregion

    #region Animation Test Buttons
    private void PlayAction(in AnimationAction action)
    {
        var animation = owner != null ? owner.GetAnimation : null;
        if (animation == null)
        {
            Debug.LogWarning($"{name}: thiếu Animation để play action", this);
            return;
        }
        animation.Play(action);
    }
    #endregion
}