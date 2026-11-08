using UnityEngine;

/// <summary>Camera-relative player locomotion, rotation, roll with early cancel, and backstep.</summary>
public class PlayerMovement : CharacterMovementBase
{
    private CharacterController cc;
    private Vector2 input;
    private PlayerState state;
    private PlayerAnimation anim;
    private PlayerCombat combat;
    private PlayerCamera playerCamera;
    private PlayerGuard guard;

    private const string DefaultBackstepAnimation = "Backstep_Main";
    private static readonly int RollStateHash = Animator.StringToHash(CharacterAnimations.RollForward.name);

    public void Initialize(PlayerState state, PlayerAnimation animation, PlayerCombat combat, PlayerCamera playerCamera, PlayerGuard guard)
    {
        this.guard = guard;
        this.state = state;
        this.anim = animation;
        this.combat = combat;
        this.playerCamera = playerCamera;
        cc = state.Owner.CharacterController;
    }

    public override void SetInput(Vector2 input) => this.input = input;

    public override void Move()
    {
        if (state == null || state.IsDead) return;

        if (state.IsRolling && !TryCancelRoll()) return;

        if (state.ApplyRootMotion) return;

        if (!state.CanMove)
        {
            cc.Move(state.GetGravityDelta());
            return;
        }

        float moveAmount = state.GetMoveAmount(input);
        Vector3 move = moveAmount > 0f ? playerCamera.GetMoveDirection(input) : Vector3.zero;

        if (move.sqrMagnitude > 0.001f)
            move.Normalize();

        HandleRunStamina(moveAmount);
        UpdateMoveSpeed(moveAmount);

        anim.SetMoving(state.CurrentMoveSpeed > state.MoveAnimSpeedThreshold);

        Vector3 horizontalDelta = move * (state.CurrentMoveSpeed * Time.deltaTime);
        horizontalDelta = state.ResolveEdgeGuard(horizontalDelta);
        cc.Move(horizontalDelta + state.GetGravityDelta());

        HandleRotation(move);
    }

    private void HandleRotation(Vector3 move)
    {
        if (!state.CanRotate) return;

        Vector3 lookDir;

        if (combat.LockOnTransform != null)
        {
            lookDir = combat.LockOnTransform.position - transform.position;
            lookDir.y = 0f;
        }
        else
        {
            lookDir = move;
        }

        if (lookDir.sqrMagnitude <= 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(lookDir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, targetRot, state.RotateSpeed * Time.deltaTime);
    }

    private void UpdateMoveSpeed(float moveAmount)
    {
        float targetSpeed = 0f;
        if (moveAmount > 0f)
            targetSpeed = state.IsRunning ? state.RunSpeed : state.MoveSpeed * moveAmount;

        targetSpeed *= guard != null ? guard.MoveSpeedMultiplier : 1f;
        targetSpeed *= state.WadeSpeedMultiplier;

        state.SetTargetMoveSpeed(targetSpeed);
        state.SetCurrentMoveSpeed(targetSpeed);
    }

    public override void Roll()
    {
        if (state == null || state.IsDead) return;
        if (state.IsRolling) return;
        if (state.IsAttacking) return;
        if (!state.HasStamina) return;
        if (state.IsInWater) return;

        Vector3 moveDirection = playerCamera.GetMoveDirection(input);
        if (state.GetMoveAmount(input) <= 0f || moveDirection.sqrMagnitude <= 0.0001f)
        {
            Backstep();
            return;
        }

        moveDirection.Normalize();

        transform.rotation = Quaternion.LookRotation(moveDirection);

        state.DrainStamina(state.RollStaminaCost);
        state.SetRolling(true);
        anim.Play(CharacterAnimations.RollForward);
    }

    private bool TryCancelRoll()
    {
        if (playerCamera.GetMoveDirection(input).sqrMagnitude <= 0.01f) return false;
        if (anim.GetActionNormalizedTime(RollStateHash) < state.RollCancelNormalizedTime) return false;

        anim.CancelAction(state.RollCancelBlendTime);
        state.SetRolling(false);
        state.SetAttacking(false);
        state.SetApplyRootMotion(false);
        state.SetCanRotate(true);
        state.SetCanMove(true);
        state.SetCurrentMoveSpeed(Mathf.Max(state.CurrentMoveSpeed, state.MoveSpeed * state.RollExitSpeedRatio));
        return true;
    }

    private void Backstep()
    {
        WeaponItem weapon = combat.CurrentWeapon;
        string animationName = weapon != null && !string.IsNullOrEmpty(weapon.backstepAnimation)
            ? weapon.backstepAnimation
            : DefaultBackstepAnimation;

        combat.DisableCanDoBackstepAttack();
        state.DrainStamina(state.BackstepStaminaCost);
        anim.Play(new AnimationAction(animationName, true, true, false, false));
    }

    public override Vector3 GetMoveDirection() =>
        playerCamera.GetMoveDirection(input).normalized;

    private void HandleRunStamina(float moveAmount)
    {
        if (!state.IsRunning || moveAmount <= 0f) return;
        if (!state.IsCombatStance) return;
        state.DrainStamina(state.RunStaminaDrainRate * Time.deltaTime);
    }
}