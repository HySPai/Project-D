using Sirenix.OdinInspector;
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
    protected CharacterController cc;

    protected int horizontalHash;
    protected int verticalHash;

    #region Init
    public virtual void Initialize(CharacterStateBase state)
    {
        this.state = state;
        cc = state.Owner.CharacterController;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.LogError($"{name}: thiếu Animator reference!", this);

        horizontalHash = Animator.StringToHash(horizontalParameter);
        verticalHash = Animator.StringToHash(verticalParameter);
    }
    #endregion

    #region Locomotion Update
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

    protected virtual Transform GetLockOnTarget()
    {
        CharacterCombatBase combat = state.Owner.GetCombat;
        return combat != null ? combat.LockOnTransform : null;
    }

    public virtual void SetMoving(bool isMoving)
    {
        animator.SetBool(isMoveParameter, isMoving);
    }
    #endregion

    #region Play Animation
    public virtual void PlayTargetAnimation(
        string targetAnimation,
        bool isPerformingAction,
        bool applyRootMotion = true,
        bool canRotate = false,
        bool canMove = false)
    {
        if (state == null) return;

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
    #endregion

    #region Root Motion
    protected virtual void OnAnimatorMove()
    {
        if (state == null || !state.ApplyRootMotion) return;

        Vector3 delta = ScaledRootMotionDelta();

        // Tách phương ngang để chặn rơi xuống vực, giữ nguyên phương dọc cho gravity.
        Vector3 horizontal = new Vector3(delta.x, 0f, delta.z);
        horizontal = state.ResolveEdgeGuard(horizontal);

        Vector3 finalDelta = horizontal + state.GetGravityDelta();

        cc.Move(finalDelta);
        transform.rotation *= animator.deltaRotation;
    }

    protected virtual Vector3 ScaledRootMotionDelta()
    {
        Vector3 delta = animator.deltaPosition;
        if (state.IsRolling)
            delta *= state.RollDistanceMultiplier;
        return delta;
    }
    #endregion
}