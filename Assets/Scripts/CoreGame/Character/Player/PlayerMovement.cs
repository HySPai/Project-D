using UnityEngine;

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

        // Đang diễn action có root motion → OnAnimatorMove lo di chuyển + gravity.
        if (state.ApplyRootMotion) return;

        // Bị khoá di chuyển nhưng vẫn phải chịu gravity (vd đang đứng diễn anim không root motion).
        if (!state.CanMove)
        {
            cc.Move(state.GetGravityDelta());
            return;
        }

        Vector3 move = playerCamera.GetMoveDirection(input);
        float moveAmount = Mathf.Clamp01(input.magnitude / state.FullSpeedInputThreshold);

        if (move.sqrMagnitude > 0.001f)
            move.Normalize();

        anim.SetMoving(moveAmount > 0.01f);

        HandleRunStamina(moveAmount);
        UpdateMoveSpeed(moveAmount);

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

        state.SetTargetMoveSpeed(targetSpeed);

        if (moveAmount <= 0f)
        {
            state.SetCurrentMoveSpeed(0f);
            return;
        }

        float smoothSpeed = state.MoveSmoothSpeed * state.MoveSpeed;
        state.SetCurrentMoveSpeed(Mathf.MoveTowards(
            state.CurrentMoveSpeed, state.TargetMoveSpeed, smoothSpeed * Time.deltaTime));
    }

    public override void Roll()
    {
        if (state == null || state.IsDead) return;
        if (state.IsRolling) return;
        if (state.IsAttacking) return;
        if (!state.HasStamina) return;

        Vector3 moveDirection = playerCamera.GetMoveDirection(input);
        if (moveDirection.sqrMagnitude <= 0.01f)
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
        state.DrainStamina(state.RunStaminaDrainRate * Time.deltaTime);
    }
}