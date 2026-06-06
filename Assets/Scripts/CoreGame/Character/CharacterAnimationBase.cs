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

    // Đồng bộ root motion mỗi frame. Việc lái blend tree do
    // UpdateAnimatorMovementParameters đảm nhiệm (gọi từ controller).
    public virtual void UpdateAnimation(float moveAmount)
    {
        if (state == null) return;
        animator.applyRootMotion = state.ApplyRootMotion;
    }

    public virtual void UpdateAnimatorMovementParameters(
        float horizontalMovement,
        float verticalMovement,
        bool isSprinting)
    {
        // Đang thực hiện action (attack/roll) -> không lái locomotion, về idle.
        if (state == null || state.IsAttacking)
        {
            horizontalMovement = 0f;
            verticalMovement = 0f;
            isSprinting = false;
        }

        float snappedHorizontal = SnapMovementValue(Mathf.Clamp(horizontalMovement, -1f, 1f));
        float snappedVertical = SnapMovementValue(Mathf.Clamp(verticalMovement, -1f, 1f));

        if (isSprinting)
            snappedVertical = 2f; // khớp motion Sprint (Pos Y = 2)

        animator.SetFloat(horizontalHash, snappedHorizontal, movementDampTime, Time.deltaTime);
        animator.SetFloat(verticalHash, snappedVertical, movementDampTime, Time.deltaTime);
    }

    public virtual void SetMoving(bool isMoving)
    {
        animator.SetBool(isMoveParameter, isMoving);
    }

    // Làm tròn về -1, -0.5, 0, 0.5, 1.
    protected static float SnapMovementValue(float value)
    {
        if (value > 0f && value <= 0.5f) return 0.5f;
        if (value > 0.5f && value <= 1f) return 1f;
        if (value < 0f && value >= -0.5f) return -0.5f;
        if (value < -0.5f && value >= -1f) return -1f;
        return 0f;
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