using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Player weapon combat: attack input buffering, combo selection, weapon equip and lock-on. Aiming, the hit
/// window and hit feedback live in PlayerAimAssist, PlayerHitWindow and PlayerHitFeedback, built in Initialize.
/// </summary>
public class PlayerCombat : CharacterCombatBase
{
    private const float NoBufferedAttack = float.NegativeInfinity;
    private const float InputEpsilon = 0.01f;
    private const float DirectionEpsilon = 0.0001f;

    [Header("Hit Detection")]
    [SerializeField] private LayerMask hitLayer = 1 << 8;
    [SerializeField] private float hitOriginHeight = 0.9f;
    [SerializeField] private float hitHeightTolerance = 1.5f;
    [SerializeField] private float pointBlankDistance = 0.8f;
    [SerializeField] private float pointBlankArc = 220f;
    [SerializeField] private float hitWindowMaxDuration = 0.8f;

    [Header("Aim Assist")]
    [SerializeField] private float aimAssistRange = 4f;
    [SerializeField] private float aimAssistAngleWithInput = 70f;
    [SerializeField] private float aimAssistAngleWithoutInput = 110f;

    [Header("Input Buffer")]
    [SerializeField] private float attackBufferDuration = 0.4f;

    [Header("Swing Trail")]
    [SerializeField] private TrailRenderer swingTrail;

    [Header("Feedbacks (Feel)")]
    [SerializeField] private CombatFeelPlayer combatFeel;
    [SerializeField] private CombatVfxController combatVfx;
    [SerializeField] private EffectController effectController;

    [Header("Debug Weapons")]
    [SerializeField] private WeaponItem[] debugWeapons;
    private int debugWeaponIndex;

    private CharacterControllerBase character;
    private PlayerCamera playerCamera;
    private PlayerState playerState;
    private AttackData currentAttack;
    private float bufferedAttackTime = NoBufferedAttack;

    private PlayerAimAssist aimAssist;
    private PlayerHitWindow hitWindow;
    private PlayerHitFeedback hitFeedback;

    public WeaponItem CurrentWeapon => currentWeapon;

    public event System.Action<WeaponItem> OnWeaponEquipped;
    public event System.Action<float> OnWeaponHit;

    public void Initialize(CharacterStateBase state, CharacterControllerBase character, PlayerCamera playerCamera)
    {
        base.Initialize(state);

        playerState = state as PlayerState;
        this.character = character;
        this.playerCamera = playerCamera;

        Camera viewCamera = playerCamera != null ? playerCamera.ViewCamera : null;
        if (viewCamera == null)
            GameLog.Warn($"{name}: PlayerCamera.viewCamera chưa gán, lock-on sẽ không tìm được mục tiêu", this);

        hitFeedback = new PlayerHitFeedback(combatFeel, combatVfx, effectController, swingTrail);
        aimAssist = new PlayerAimAssist(transform, character, viewCamera, BuildAimSettings());
        hitWindow = new PlayerHitWindow(transform, character, state, hitFeedback, RaiseWeaponHit, BuildHitSettings());
    }

    private void Update()
    {
        if (state == null) return;

        CheckAutoUnlock();
        ConsumeBufferedAttack();
        hitWindow.Sweep();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        aimAssist?.Configure(BuildAimSettings());
        hitWindow?.Configure(BuildHitSettings());
    }
#endif

    private AimAssistSettings BuildAimSettings() => new AimAssistSettings(
        hitLayer, lockOnLayer, aimAssistRange, aimAssistAngleWithInput, aimAssistAngleWithoutInput,
        hitHeightTolerance, BroadphaseRange);

    private HitWindowSettings BuildHitSettings() => new HitWindowSettings(
        hitLayer, hitOriginHeight, hitHeightTolerance, pointBlankDistance, pointBlankArc, hitWindowMaxDuration);

    private void RaiseWeaponHit(float appliedPoints) => OnWeaponHit?.Invoke(appliedPoints);

    private WeaponFeelProfile CurrentFeel => currentWeapon != null ? currentWeapon.feelProfile : null;

    #region Attack Input
    public override void Attack()
    {
        if (TryStartAttack())
        {
            bufferedAttackTime = NoBufferedAttack;
            return;
        }

        bufferedAttackTime = Time.time;
    }

    private void ConsumeBufferedAttack()
    {
        if (float.IsNegativeInfinity(bufferedAttackTime)) return;

        WeaponFeelProfile bufferProfile = CurrentFeel;
        float bufferDuration = bufferProfile != null ? bufferProfile.InputBufferDuration : attackBufferDuration;

        if (Time.time - bufferedAttackTime > bufferDuration)
        {
            bufferedAttackTime = NoBufferedAttack;
            return;
        }

        if (TryStartAttack())
            bufferedAttackTime = NoBufferedAttack;
    }

    private bool TryStartAttack()
    {
        if (state == null || state.IsDead) return false;
        if (currentWeapon == null) return false;
        if (playerState != null && !playerState.HasStamina) return false;

        if (canPerformBackstepAttack)
        {
            DisableCanDoBackstepAttack();
            currentAttackType = AttackType.BackstepAttack;
            return DoAttack(currentWeapon.backstepAttack);
        }

        if (canPerformRollingAttack)
        {
            DisableCanDoRollingAttack();
            currentAttackType = AttackType.RollingAttack01;
            return DoAttack(currentWeapon.rollingAttack);
        }

        if (state.IsAttacking)
        {
            if (!canDoCombo) return false;

            DisableCanDoCombo();
            AdvanceCombo();
            return DoAttack(GetCurrentComboAttack());
        }

        if (state.IsRunning)
        {
            currentAttackType = AttackType.RunningAttack01;
            return DoAttack(currentWeapon.runningAttack);
        }

        comboIndex = 0;
        return DoAttack(GetCurrentComboAttack());
    }

    private bool DoAttack(AttackData attack)
    {
        if (attack == null || string.IsNullOrEmpty(attack.animationName)) return false;

        hitWindow.End();
        currentAttack = attack;

        AimAttack();
        PerformAttack(attack, character);
        character.GetAnimation.SetPlaybackSpeed(attack.animationSpeed > 0f ? attack.animationSpeed : 1f);
        return true;
    }

    private void AimAttack()
    {
        Vector3 inputDirection = character.GetMovement.GetMoveDirection();
        Vector3 attackDirection = aimAssist.ResolveAttackDirection(inputDirection, ResolveLockedTarget());

        if (attackDirection.sqrMagnitude > DirectionEpsilon)
            transform.rotation = Quaternion.LookRotation(attackDirection);
    }

    private CharacterControllerBase ResolveLockedTarget()
    {
        if (lockOnTransform == null) return null;

        CharacterStateBase lockedState = LockedTargetState;
        if (lockedState == null || lockedState.IsDead) return null;

        return lockedState.Owner;
    }

    public override void ResetCombat()
    {
        base.ResetCombat();
        hitWindow?.ClearHitTargets();
        currentAttack = null;
        bufferedAttackTime = NoBufferedAttack;
    }

    /// <summary>Animation event: spends the stamina cost of the attack being performed.</summary>
    public void DrainStaminaBasedOnAttack()
    {
        if (playerState == null || currentAttack == null) return;

        playerState.DrainStamina(currentAttack.stamina);
    }

    public override void EnableCanDoCombo()
    {
        canDoCombo = true;
    }

    public override void DisableCanDoCombo()
    {
        canDoCombo = false;
    }
    #endregion

    #region Weapon
    public void EquipWeapon(WeaponItem weapon)
    {
        if (weapon == null) return;

        currentWeapon = weapon;
        comboIndex = 0;
        OnWeaponEquipped?.Invoke(weapon);
    }

    [Button]
    public void CycleDebugWeapon()
    {
        if (debugWeapons == null || debugWeapons.Length == 0) return;

        debugWeaponIndex = (debugWeaponIndex + 1) % debugWeapons.Length;
        EquipWeapon(debugWeapons[debugWeaponIndex]);
    }

    /// <summary>Block or parry feedback using the current weapon's feel profile.</summary>
    public void PlayDefenseFeedback(HitFeelTier tier, Vector3 position)
    {
        hitFeedback?.PlayDefense(CurrentFeel, tier, position);
    }
    #endregion

    #region Hit Window
    /// <summary>Animation event: opens the sweep-based hit window for the current attack.</summary>
    public override void OpenDamageCollider()
    {
        if (currentAttack == null) return;
        hitWindow.Begin(currentAttack, currentWeapon);
    }

    /// <summary>Animation event: closes the hit window.</summary>
    public override void CloseDamageCollider() => hitWindow?.End();

    public override void DisableAllDamageColliders()
    {
        base.DisableAllDamageColliders();
        hitWindow?.End();
    }
    #endregion

    #region Lock On
    /// <summary>Toggles lock-on: clears an existing lock, otherwise locks the nearest on-screen target.</summary>
    public void LockTarget()
    {
        if (lockOnTransform != null)
        {
            ClearLockTarget();
            return;
        }

        aimAssist.TryFindNearestLockTarget(out Transform target, out CharacterStateBase targetState);
        SetLockTarget(target, targetState);
    }

    /// <summary>Moves the lock to the best on-screen target in the stick direction, if any.</summary>
    public void SwitchTarget(Vector2 inputDir)
    {
        if (lockOnTransform == null || inputDir.sqrMagnitude < InputEpsilon) return;

        if (aimAssist.TryFindSwitchTarget(lockOnTransform, inputDir, playerCamera.Right, playerCamera.Forward,
                out Transform target, out CharacterStateBase targetState))
            SetLockTarget(target, targetState);
    }

    private void CheckAutoUnlock()
    {
        if (lockOnTransform == null) return;

        CharacterStateBase lockedState = LockedTargetState;
        if (lockedState != null && lockedState.IsDead)
        {
            ClearLockTarget();
            return;
        }

        if (!aimAssist.IsInView(lockOnTransform.position))
            ClearLockTarget();
    }
    #endregion
}
