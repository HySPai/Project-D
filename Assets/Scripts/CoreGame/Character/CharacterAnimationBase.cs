using UnityEngine;

public abstract class CharacterAnimationBase : MonoBehaviour
{
    [SerializeField] protected Animator animator;

    [Header("Locomotion Parameters")]
    [SerializeField] protected string horizontalParameter = "Horizontal";
    [SerializeField] protected string verticalParameter = "Vertical";
    [SerializeField] protected string isMoveParameter = "IsMove";
    [SerializeField] protected float movementDampTime = 0.1f;

    protected CharacterStateBase state;
    protected bool applyRootMotion;
    protected Rigidbody rb;
    protected CapsuleCollider capsule;

    protected int horizontalHash;
    protected int verticalHash;

    public virtual void Initialize(CharacterStateBase state)
    {
        this.state = state;
        rb = state.Owner.Rigidbody;
        capsule = state.Owner.Capsule;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.LogError($"{name}: thiếu Animator reference!", this);

        horizontalHash = Animator.StringToHash(horizontalParameter);
        verticalHash = Animator.StringToHash(verticalParameter);
    }

    // Cập nhật animation mỗi frame: đồng bộ root motion + lái blend tree locomotion.
    // Controller chỉ cần gọi method này, mọi tính toán nằm ở đây.
    public virtual void UpdateAnimation(float moveAmount)
    {
        if (state == null) return;
        animator.applyRootMotion = state.ApplyRootMotion;
        UpdateLocomotionParameters(moveAmount);
    }

    protected virtual void UpdateLocomotionParameters(float moveAmount)
    {
        float horizontal = 0f;
        float vertical = moveAmount;

        Transform lockTarget = GetLockOnTarget();
        if (lockTarget != null)
        {
            CharacterMovementBase movement = state.Owner.GetMovement;
            Vector3 worldDir = movement != null ? movement.GetMoveDirection() : Vector3.zero;

            if (worldDir.sqrMagnitude > 0.0001f)
            {
                Vector3 local = state.Owner.transform.InverseTransformDirection(worldDir.normalized);
                horizontal = local.x * moveAmount;
                vertical = local.z * moveAmount;
            }
            else
            {
                horizontal = 0f;
                vertical = 0f;
            }
        }

        animator.SetFloat(horizontalHash, horizontal, movementDampTime, Time.deltaTime);
        animator.SetFloat(verticalHash, vertical, movementDampTime, Time.deltaTime);
    }

    // Mục tiêu đang lock (nếu có). Tách riêng để subclass dễ override nếu cần.
    protected virtual Transform GetLockOnTarget()
    {
        CharacterCombatBase combat = state.Owner.GetCombat;
        return combat != null ? combat.LockOnTransform : null;
    }

    public virtual void SetMoving(bool isMoving)
    {
        animator.SetBool(isMoveParameter, isMoving);
    }

    public virtual void ApplyRootMotion(bool value)
    {
        applyRootMotion = value;
    }

    public virtual void PlayTargetAnimation(
        string targetAnimation,
        bool isPerformingAction,
        bool applyRootMotion = true,
        bool canRotate = false,
        bool canMove = false)
    {
        if (state == null) return;
        this.applyRootMotion = applyRootMotion;
        animator.CrossFade(targetAnimation, 0.2f);
        state.SetAttacking(isPerformingAction);
        state.SetApplyRootMotion(applyRootMotion);
        state.SetCanRotate(canRotate);
        state.SetCanMove(canMove);
    }

    public void Play(in AnimationAction action)
    {
        PlayTargetAnimation(
            action.name, action.isAction, action.rootMotion,
            action.canRotate, action.canMove);
    }

    // ---- Root motion + bám đất + chặn xuyên tường (dùng chung) ----
    protected virtual void OnAnimatorMove()
    {
        if (state == null || !state.ApplyRootMotion) return;

        Vector3 delta = ScaledRootMotionDelta();
        Vector3 targetPos = rb.position + delta;

        if (!TryResolveGroundedPosition(targetPos, out Vector3 groundedPos))
            return;
        if (IsBlockedByObstacle(groundedPos))
            return;

        rb.MovePosition(groundedPos);
        rb.MoveRotation(rb.rotation * animator.deltaRotation);
    }

    protected virtual Vector3 ScaledRootMotionDelta()
    {
        Vector3 delta = animator.deltaPosition;
        if (state.IsRolling)
            delta *= state.RollDistanceMultiplier;
        return delta;
    }

    protected bool TryResolveGroundedPosition(Vector3 position, out Vector3 grounded)
    {
        grounded = position;
        float up = state.MaxStepUpHeight + 0.2f;
        Vector3 origin = position + Vector3.up * up;
        float maxDistance = up + state.MaxStepDownHeight + 0.2f;

        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit groundHit, maxDistance, state.GroundLayer))
            return false;

        float heightDiff = groundHit.point.y - position.y;
        if (heightDiff > state.MaxStepUpHeight) return false;
        if (-heightDiff > state.MaxStepDownHeight) return false;

        grounded = new Vector3(position.x, groundHit.point.y, position.z);
        return true;
    }

    protected bool IsBlockedByObstacle(Vector3 groundedPosition)
    {
        float skin = state.MaxStepUpHeight;
        Vector3 bottom = groundedPosition + Vector3.up * (skin + capsule.radius);
        Vector3 top = groundedPosition + Vector3.up * (capsule.height - capsule.radius);
        if (top.y < bottom.y) top = bottom;

        return Physics.CheckCapsule(
            bottom, top, capsule.radius * 0.9f,
            state.GroundLayer, QueryTriggerInteraction.Ignore);
    }
}