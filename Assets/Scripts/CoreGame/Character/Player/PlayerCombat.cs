using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using MMF_Player = MoreMountains.Feedbacks.MMF_Player;

public class PlayerCombat : CharacterCombatBase
{
    private const float NoBufferedAttack = float.NegativeInfinity;
    private const float BroadphasePadding = 1.5f;
    private const float AngleScorePerDegree = 0.022f;
    private const int HitBufferSize = 24;

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
    [SerializeField] private float lungeStopDistance = 0.9f;
    [SerializeField] private float lungeDuration = 0.14f;

    [Header("Input Buffer")]
    [SerializeField] private float attackBufferDuration = 0.4f;

    [Header("Swing Trail")]
    [SerializeField] private TrailRenderer swingTrail;

    [Header("Feedbacks (Feel)")]
    [SerializeField] private MMF_Player hitFeedback;
    [SerializeField] private MMF_Player killFeedback;

    private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
    private readonly HashSet<CharacterStateBase> hitTargets = new HashSet<CharacterStateBase>();

    private CharacterControllerBase character;
    private PlayerCamera playerCamera;
    private AttackData currentAttack;
    private PlayerState playerState;

    private float bufferedAttackTime = NoBufferedAttack;
    private bool hitWindowOpen;
    private float hitWindowExpireTime;
    private Tween lungeTween;

    public void Initialize(CharacterStateBase state, CharacterControllerBase character, PlayerCamera playerCamera)
    {
        base.Initialize(state);

        this.playerState = state as PlayerState;
        this.character = character;
        this.playerCamera = playerCamera;

        if (swingTrail != null)
            swingTrail.emitting = false;
    }

    protected override void Update()
    {
        base.Update();

        if (state == null) return;

        ConsumeBufferedAttack();
        SweepHitWindow();
    }

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

        if (Time.time - bufferedAttackTime > attackBufferDuration)
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

        EndHitWindow();
        currentAttack = attack;

        AimAttack(attack);
        PerformAttack(attack, character);
        character.GetAnimation.SetPlaybackSpeed(attack.animationSpeed > 0f ? attack.animationSpeed : 1f);
        return true;
    }

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

    #region Aim Assist
    private void AimAttack(AttackData attack)
    {
        Vector3 inputDirection = character.GetMovement.GetMoveDirection();
        bool hasInput = inputDirection.sqrMagnitude > 0.01f;
        Vector3 desiredDirection = hasInput ? inputDirection : GetFlatForward();

        CharacterControllerBase target = ResolveLockedTarget();
        if (target == null)
        {
            float halfAngle = hasInput ? aimAssistAngleWithInput : aimAssistAngleWithoutInput;
            target = FindAimAssistTarget(desiredDirection, halfAngle);
        }

        Vector3 attackDirection = desiredDirection;
        float lungeDistance = 0f;

        if (target != null)
        {
            Vector3 toTarget = target.transform.position - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;

            if (distance > 0.05f)
            {
                attackDirection = toTarget / distance;

                float edgeDistance = distance - GetTargetRadius(target);
                if (edgeDistance <= aimAssistRange)
                    lungeDistance = Mathf.Clamp(edgeDistance - lungeStopDistance, 0f, attack.lungeDistance);
            }
        }

        if (attackDirection.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(attackDirection);

        StartLunge(attackDirection, lungeDistance);
    }

    private CharacterControllerBase ResolveLockedTarget()
    {
        if (lockOnTransform == null) return null;

        CharacterStateBase lockedState = LockedTargetState;
        if (lockedState == null || lockedState.IsDead) return null;

        return lockedState.Owner;
    }

    private CharacterControllerBase FindAimAssistTarget(Vector3 desiredDirection, float halfAngle)
    {
        Vector3 origin = transform.position;
        int count = Physics.OverlapSphereNonAlloc(
            origin, aimAssistRange + BroadphasePadding, hitBuffer, hitLayer, QueryTriggerInteraction.Ignore);

        CharacterControllerBase best = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (!TryResolveTarget(hitBuffer[i], out CharacterControllerBase candidate)) continue;

            Vector3 toCandidate = candidate.transform.position - origin;
            if (Mathf.Abs(toCandidate.y) > hitHeightTolerance) continue;
            toCandidate.y = 0f;

            float edgeDistance = toCandidate.magnitude - GetTargetRadius(candidate);
            if (edgeDistance > aimAssistRange) continue;

            float angle = Vector3.Angle(desiredDirection, toCandidate);
            if (angle > halfAngle) continue;

            float score = Mathf.Max(0f, edgeDistance) + angle * AngleScorePerDegree;
            if (score < bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        return best;
    }

    private void StartLunge(Vector3 direction, float distance)
    {
        KillLunge();
        if (distance <= 0.01f) return;

        CharacterController controller = character.CharacterController;
        float travelled = 0f;

        lungeTween = DOVirtual.Float(0f, distance, lungeDuration, value =>
            {
                float step = value - travelled;
                travelled = value;

                if (state.IsDead || !controller.enabled) return;
                controller.Move(state.ResolveEdgeGuard(direction * step));
            })
            .SetEase(Ease.OutCubic)
            .SetLink(gameObject);
    }

    private void KillLunge()
    {
        if (lungeTween != null && lungeTween.IsActive())
            lungeTween.Kill();

        lungeTween = null;
    }

    private Vector3 GetFlatForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
    }
    #endregion

    #region Hit Window
    public override void OpenDamageCollider() => BeginHitWindow();
    public override void CloseDamageCollider() => EndHitWindow();

    public override void DisableAllDamageColliders()
    {
        base.DisableAllDamageColliders();
        EndHitWindow();
        KillLunge();
    }

    private void BeginHitWindow()
    {
        if (currentAttack == null || state == null || state.IsDead) return;

        hitWindowOpen = true;
        hitWindowExpireTime = Time.time + hitWindowMaxDuration;
        hitTargets.Clear();

        if (swingTrail != null)
        {
            swingTrail.Clear();
            swingTrail.emitting = true;
        }

        SweepHitWindow();
    }

    private void EndHitWindow()
    {
        hitWindowOpen = false;

        if (swingTrail != null)
            swingTrail.emitting = false;
    }

    private void SweepHitWindow()
    {
        if (!hitWindowOpen) return;

        if (state.IsDead || !state.IsAttacking || Time.time > hitWindowExpireTime)
        {
            EndHitWindow();
            return;
        }

        Vector3 origin = transform.position;
        Vector3 forward = GetFlatForward();

        int count = Physics.OverlapSphereNonAlloc(
            origin + Vector3.up * hitOriginHeight,
            currentAttack.range + BroadphasePadding,
            hitBuffer, hitLayer, QueryTriggerInteraction.Ignore);

        int hitCount = 0;
        bool killedAny = false;

        for (int i = 0; i < count; i++)
        {
            if (!TryResolveTarget(hitBuffer[i], out CharacterControllerBase target)) continue;

            CharacterStateBase targetState = target.GetState;
            if (targetState.IsInvulnerable) continue;
            if (hitTargets.Contains(targetState)) continue;
            if (!IsInsideHitArc(origin, forward, target)) continue;

            hitTargets.Add(targetState);
            targetState.TakeDamage(currentAttack.damage, transform.position);

            hitCount++;
            killedAny |= targetState.IsDead;
        }

        if (hitCount > 0)
            PlayHitFeedback(killedAny);
    }

    private void PlayHitFeedback(bool killedAny)
    {
        MMF_Player feedback = killedAny && killFeedback != null ? killFeedback : hitFeedback;
        if (feedback == null) return;

        feedback.PlayFeedbacks(transform.position, currentAttack.impact);
    }

    private bool TryResolveTarget(Collider targetCollider, out CharacterControllerBase target)
    {
        if (!CombatTargetRegistry.TryGet(targetCollider, out target)) return false;
        if (target == null || target == character) return false;

        CharacterStateBase targetState = target.GetState;
        return targetState != null && !targetState.IsDead;
    }

    private bool IsInsideHitArc(Vector3 origin, Vector3 forward, CharacterControllerBase target)
    {
        Vector3 toTarget = target.transform.position - origin;
        if (Mathf.Abs(toTarget.y) > hitHeightTolerance) return false;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        if (distance < 0.05f) return true;

        float targetRadius = GetTargetRadius(target);
        float edgeDistance = distance - targetRadius;
        if (edgeDistance > currentAttack.range) return false;

        float arc = edgeDistance <= pointBlankDistance
            ? Mathf.Max(currentAttack.arc, pointBlankArc)
            : currentAttack.arc;

        float angularPadding = Mathf.Asin(Mathf.Clamp01(targetRadius / distance)) * Mathf.Rad2Deg;
        float angle = Vector3.Angle(forward, toTarget);

        return angle <= arc * 0.5f + angularPadding;
    }

    private float GetTargetRadius(CharacterControllerBase target)
    {
        CharacterController controller = target.CharacterController;
        if (controller == null) return 0f;

        Vector3 scale = target.transform.lossyScale;
        return controller.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
    }
    #endregion

    #region Lock On
    public void LockTarget()
    {
        if (lockOnTransform != null)
        {
            ClearLockTarget();
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, BroadphaseRange, lockOnLayer);

        float nearestDistance = float.MaxValue;
        Transform nearestTarget = null;

        foreach (Collider hit in hits)
        {
            if (!IsInView(hit.transform.position)) continue;

            float sqrDistance = (hit.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance < nearestDistance)
            {
                nearestDistance = sqrDistance;
                nearestTarget = hit.transform;
            }
        }

        SetLockTarget(nearestTarget);
    }

    public void SwitchTarget(Vector2 inputDir)
    {
        if (lockOnTransform == null || inputDir.sqrMagnitude < 0.01f) return;

        Vector3 camRight = playerCamera.Right;
        Vector3 camForward = playerCamera.Forward;
        Vector3 playerPos = transform.position;

        Vector2 currentScreen = ToScreenPlane(lockOnTransform.position - playerPos, camRight, camForward);

        Collider[] hits = Physics.OverlapSphere(playerPos, BroadphaseRange, lockOnLayer);

        Transform best = null;
        float bestScore = float.MinValue;

        Vector2 desired2D = inputDir.normalized;

        foreach (Collider hit in hits)
        {
            Transform t = hit.transform;
            if (t == lockOnTransform) continue;

            if (!IsInView(t.position)) continue;

            Vector2 candidateScreen = ToScreenPlane(t.position - playerPos, camRight, camForward);

            Vector2 delta = candidateScreen - currentScreen;
            if (delta.sqrMagnitude < 0.0001f) continue;

            Vector2 deltaDir = delta.normalized;

            float alignment = Vector2.Dot(deltaDir, desired2D);
            if (alignment <= 0f) continue;

            float score = alignment / delta.magnitude;

            if (score > bestScore)
            {
                bestScore = score;
                best = t;
            }
        }

        if (best != null)
            SetLockTarget(best);
    }

    private Vector2 ToScreenPlane(Vector3 worldDir, Vector3 camRight, Vector3 camForward)
    {
        float x = Vector3.Dot(worldDir, camRight);
        float y = Vector3.Dot(worldDir, camForward);
        return new Vector2(x, y);
    }
    #endregion
}
