using UnityEngine;

public class PlayerAnimation : CharacterAnimationBase
{
    [SerializeField] private string moveParameter = "Move";

    private float currentMoveValue;
    private PlayerState playerState;
    private Rigidbody rb;
    private CapsuleCollider capsule;

    public void Initialize(PlayerState state)
    {
        this.state = state;
        playerState = state;
        base.Initialize(state);

        rb      = state.Owner.Rigidbody;
        capsule = state.Owner.Capsule;
    }

    public override void UpdateAnimation(float moveAmount)
    {
        animator.applyRootMotion = state.ApplyRootMotion;

        float targetMoveValue = ResolveTargetMoveValue(moveAmount);

        currentMoveValue = Mathf.MoveTowards(
            currentMoveValue,
            targetMoveValue,
            state.MoveSmoothSpeed * Time.deltaTime
        );

        animator.SetFloat(moveParameter, currentMoveValue);
    }

    private void OnAnimatorMove()
    {
        if (state == null || !state.ApplyRootMotion) return;

        Vector3 delta     = ScaledRootMotionDelta();
        Vector3 targetPos = rb.position + delta;

        if (!IsGroundReachable(targetPos)) return;
        if (IsBlockedByObstacle(targetPos)) return;

        rb.MovePosition(targetPos);
        rb.MoveRotation(rb.rotation * animator.deltaRotation);
    }

    private float ResolveTargetMoveValue(float moveAmount)
    {
        if (state == null)       return 0f;
        if (state.IsAttacking)   return 0f;
        return moveAmount;
    }

    private Vector3 ScaledRootMotionDelta()
    {
        Vector3 delta = animator.deltaPosition;

        if (state.IsRolling)
            delta *= state.RollDistanceMultiplier;

        return delta;
    }

    private bool IsGroundReachable(Vector3 position)
    {
        Vector3 origin = position + Vector3.up * 0.2f;

        bool hit = Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit groundHit,
            playerState.GroundCheckDistance,
            playerState.GroundLayer
        );

        if (!hit) return false;

        float heightDiff = position.y - groundHit.point.y;
        return heightDiff <= playerState.MaxStepDownHeight;
    }

    private bool IsBlockedByObstacle(Vector3 position)
    {
        Vector3 bottom = position + Vector3.up * capsule.radius;
        Vector3 top    = position + Vector3.up * (capsule.height - capsule.radius);

        return Physics.CheckCapsule(
            bottom,
            top,
            capsule.radius * 0.9f,
            playerState.GroundLayer,
            QueryTriggerInteraction.Ignore
        );
    }
}