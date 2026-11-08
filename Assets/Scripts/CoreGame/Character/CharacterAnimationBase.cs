using UnityEngine;

/// <summary>Shared animator driver: locomotion parameters, action playback by state hash, and root motion with edge guard.</summary>
public abstract class CharacterAnimationBase : MonoBehaviour
{
    private const float ActionCrossFade = 0.2f;
    private const float DirectionEpsilon = 0.0001f;

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
    protected int isMoveHash;
    private bool isMoving;
    private bool isMovingSet;

    private float playbackSpeed = 1f;
    private float playbackScale = 1f;

    #region Init
    public virtual void Initialize(CharacterStateBase state)
    {
        this.state = state;
        cc = state.Owner.CharacterController;

        if (animator == null)
            Debug.LogError($"{name}: thiếu Animator reference!", this);

        horizontalHash = Animator.StringToHash(horizontalParameter);
        verticalHash = Animator.StringToHash(verticalParameter);
        isMoveHash = Animator.StringToHash(isMoveParameter);
        isMovingSet = false;
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

            if (worldDir.sqrMagnitude > DirectionEpsilon)
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

    public virtual void SetMoving(bool moving)
    {
        if (isMovingSet && moving == isMoving) return;

        isMoving = moving;
        isMovingSet = true;
        animator.SetBool(isMoveHash, moving);
    }
    #endregion

    #region Play Animation
    /// <summary>Cross-fades into a state by name. Prefer the hash overload or Play(AnimationAction) on hot paths.</summary>
    public void PlayTargetAnimation(
        string targetAnimation,
        bool isPerformingAction,
        bool applyRootMotion = true,
        bool canRotate = false,
        bool canMove = false)
    {
        PlayTargetAnimation(Animator.StringToHash(targetAnimation), isPerformingAction, applyRootMotion, canRotate, canMove);
    }

    /// <summary>Cross-fades into a state by its Animator.StringToHash value and applies the action flags.</summary>
    public virtual void PlayTargetAnimation(
        int targetAnimationHash,
        bool isPerformingAction,
        bool applyRootMotion = true,
        bool canRotate = false,
        bool canMove = false)
    {
        if (state == null) return;

        SetPlaybackSpeed(1f);
        animator.CrossFade(targetAnimationHash, ActionCrossFade);
        state.SetAttacking(isPerformingAction);
        state.SetApplyRootMotion(applyRootMotion);
        state.SetCanRotate(canRotate);
        state.SetCanMove(canMove);
    }

    /// <summary>Base playback speed of the current action, e.g. an attack's animationSpeed.</summary>
    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = speed;
        animator.speed = playbackSpeed * playbackScale;
    }

    /// <summary>Status factor on top of every playback speed, e.g. 0.6 while slowed.</summary>
    public void SetPlaybackScale(float scale)
    {
        if (Mathf.Approximately(scale, playbackScale)) return;

        playbackScale = scale;
        if (animator != null)
            animator.speed = playbackSpeed * playbackScale;
    }

    /// <summary>Returns the animator to its default state with neutral playback speed.</summary>
    public void ResetAnimator()
    {
        if (animator == null) return;

        playbackSpeed = 1f;
        playbackScale = 1f;
        animator.speed = 1f;
        isMovingSet = false;
        animator.Rebind();
        animator.Update(0f);
    }

    /// <summary>Plays an AnimationAction using its precomputed state hash.</summary>
    public void Play(in AnimationAction action)
    {
        PlayTargetAnimation(
            action.hash, action.isAction, action.rootMotion,
            action.canRotate, action.canMove);
    }
    #endregion

    #region Root Motion
    protected virtual void OnAnimatorMove()
    {
        if (state == null || !state.ApplyRootMotion) return;

        Vector3 delta = ScaledRootMotionDelta();

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
