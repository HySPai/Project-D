using System;
using UnityEngine;

public abstract class CharacterStateBase : MonoBehaviour
{
    #region Stats
    [Header("Stats")]
    [SerializeField] protected CharacterStats stats;

    protected float maxHp;
    protected float moveSpeed;
    protected float runSpeed;
    protected float rotateSpeed;

    [SerializeField] protected float currentHp;

    public event Action<float, float> OnHpChanged;

    public float MaxHp => maxHp;
    public float CurrentHp => currentHp;
    public float MoveSpeed => moveSpeed;
    public float RunSpeed => runSpeed;
    public float RotateSpeed => rotateSpeed;
    #endregion

    #region Layers
    [Header("Layers")]
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;

    public LayerMask GroundLayer => groundLayer;
    public LayerMask ObstacleLayer => obstacleLayer;
    #endregion

    #region Animation Config
    [Header("Animation Config")]
    [SerializeField] protected float moveSmoothSpeed = 8f;

    public float MoveSmoothSpeed => moveSmoothSpeed;
    #endregion

    #region Runtime Movement
    [Header("Runtime Movement")]
    [SerializeField] protected float currentMoveSpeed;
    [SerializeField] protected float targetMoveSpeed;

    public float CurrentMoveSpeed => currentMoveSpeed;
    public float TargetMoveSpeed => targetMoveSpeed;
    #endregion

    #region State Flags
    [Header("State Flags")]
    [SerializeField] protected bool isDead;
    [SerializeField] protected bool isAttacking;
    [SerializeField] protected bool isRunning;
    [SerializeField] protected bool isRolling;
    [SerializeField] protected bool isInvulnerable;

    public bool IsDead => isDead;
    public bool IsAttacking => isAttacking;
    public bool IsRunning => isRunning;
    public bool IsRolling => isRolling;
    public bool IsInvulnerable => isInvulnerable;
    #endregion

    #region Permissions
    [Header("Permissions")]
    [SerializeField] private bool applyRootMotion;
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canRotate = true;

    public bool ApplyRootMotion => applyRootMotion;
    public bool CanMove => canMove;
    public bool CanRotate => canRotate;
    #endregion

    #region Root Motion
    [Header("Root Motion")]
    [SerializeField] protected float rollDistanceMultiplier = 1f;

    public float RollDistanceMultiplier => rollDistanceMultiplier;
    #endregion

    #region Ground & Gravity
    [Header("Ground & Gravity")]
    [SerializeField] protected float gravityForce = -5.55f;
    [SerializeField] protected float groundCheckSphereRadius = 0.3f;
    [SerializeField] protected float groundedYVelocity = -20f;  // lực ghim xuống đất khi grounded
    [SerializeField] protected float fallStartYVelocity = -5f;  // lực rơi ban đầu khi vừa rời đất
    [SerializeField] protected float maxStepDownHeight = 1f;    // bước xuống tối đa (dùng cho edge guard)
    [SerializeField] protected float edgeCheckDistance = 1.5f;  // tầm raycast kiểm tra mép vực

    public float MaxStepDownHeight => maxStepDownHeight;

    protected Vector3 yVelocity;
    protected bool fallingVelocityHasBeenSet;
    protected float inAirTimer;
    protected bool isGrounded = true;

    public bool IsGrounded => isGrounded;
    public Vector3 YVelocity => yVelocity;
    public float InAirTimer => inAirTimer;
    #endregion

    #region Owner / Animations
    protected CharacterControllerBase owner;
    public CharacterControllerBase Owner => owner;

    protected virtual AnimationAction DeathAnimation => CharacterAnimations.Death;
    #endregion

    #region Init
    public virtual void Initialize(CharacterControllerBase owner)
    {
        this.owner = owner;
    }

    protected virtual void Awake()
    {
        InitializeStats();
        currentHp = maxHp;
        currentMoveSpeed = 0f;
        targetMoveSpeed = 0f;
    }

    protected virtual void InitializeStats()
    {
        if (stats == null)
        {
            Debug.LogError($"{name}: chưa gán CharacterStats", this);
            return;
        }

        maxHp = stats.maxHp;
        moveSpeed = stats.moveSpeed;
        runSpeed = stats.runSpeed;
        rotateSpeed = stats.rotateSpeed;
    }
    #endregion

    #region Ground Check & Gravity
    // Gọi mỗi frame từ controller (đầu Update) để cập nhật grounded + vận tốc dọc.
    public virtual void HandleGroundCheckAndGravity()
    {
        isGrounded = Physics.CheckSphere(
            transform.position, groundCheckSphereRadius, groundLayer,
            QueryTriggerInteraction.Ignore);

        if (isGrounded)
        {
            if (yVelocity.y < 0f)
            {
                inAirTimer = 0f;
                fallingVelocityHasBeenSet = false;
                yVelocity.y = groundedYVelocity;
            }
        }
        else
        {
            if (!fallingVelocityHasBeenSet)
            {
                fallingVelocityHasBeenSet = true;
                yVelocity.y = fallStartYVelocity;
            }
            inAirTimer += Time.deltaTime;
            yVelocity.y += gravityForce * Time.deltaTime;
        }
    }

    // Vận tốc dọc theo frame, để controller/animation đưa vào CharacterController.Move().
    public Vector3 GetGravityDelta() => yVelocity * Time.deltaTime;

    // Chặn bước ra mép vực: kiểm tra từng trục ngang, trục nào không có đất hợp lệ thì bỏ.
    // Dùng chung cho cả di chuyển thường (PlayerMovement) lẫn root motion (OnAnimatorMove).
    public virtual Vector3 ResolveEdgeGuard(Vector3 horizontalDelta)
    {
        // Đang ở trên không thì không chặn (đang rơi sẵn).
        if (!isGrounded) return horizontalDelta;

        Vector3 origin = transform.position;

        if (Mathf.Abs(horizontalDelta.x) > 0.0001f)
        {
            Vector3 xTarget = origin + new Vector3(horizontalDelta.x, 0f, 0f);
            if (!HasGroundBelow(xTarget))
                horizontalDelta.x = 0f;
        }

        if (Mathf.Abs(horizontalDelta.z) > 0.0001f)
        {
            Vector3 zTarget = origin + new Vector3(0f, 0f, horizontalDelta.z);
            if (!HasGroundBelow(zTarget))
                horizontalDelta.z = 0f;
        }

        return horizontalDelta;
    }

    protected bool HasGroundBelow(Vector3 position)
    {
        Vector3 rayOrigin = position + Vector3.up * 0.2f;

        if (!Physics.Raycast(
                rayOrigin, Vector3.down, out RaycastHit hit,
                edgeCheckDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        float heightDiff = transform.position.y - hit.point.y;
        return heightDiff <= maxStepDownHeight;
    }
    #endregion

    #region State Setters
    public virtual void SetRunning(bool value) => isRunning = value;
    public virtual void SetAttacking(bool value) => isAttacking = value;
    public virtual void SetRolling(bool value) => isRolling = value;
    #endregion

    #region Permission Setters
    public void SetApplyRootMotion(bool value) => applyRootMotion = value;
    public void SetCanMove(bool value) => canMove = value;
    public void SetCanRotate(bool value) => canRotate = value;
    #endregion

    #region Runtime Setters
    public virtual void SetCurrentMoveSpeed(float value) => currentMoveSpeed = value;
    public virtual void SetTargetMoveSpeed(float value) => targetMoveSpeed = value;
    #endregion

    #region Config Setters
    public virtual void SetMoveSpeed(float value) => moveSpeed = value;
    public virtual void SetRunSpeed(float value) => runSpeed = value;
    public virtual void SetRotateSpeed(float value) => rotateSpeed = value;
    #endregion

    #region Health
    protected void RaiseHpChanged() => OnHpChanged?.Invoke(currentHp, maxHp);

    public virtual void TakeDamage(float damage, Vector3 sourcePosition)
    {
        if (isDead) return;
        if (isInvulnerable) return;

        currentHp -= damage;

        if (currentHp <= 0f)
        {
            currentHp = 0f;
            Die();
        }
        else
        {
            PlayHitReaction(sourcePosition);
        }

        RaiseHpChanged();
    }

    // Overload không có nguồn — mặc định coi đòn đến từ phía trước.
    public virtual void TakeDamage(float damage)
    {
        TakeDamage(damage, transform.position + transform.forward);
    }

    public virtual void EnableIsInvulnerable() => isInvulnerable = true;
    public virtual void DisableIsInvulnerable() => isInvulnerable = false;

    protected virtual void Die()
    {
        isDead = true;

        // Chết thì tắt mọi DamageCollider để xác không còn gây sát thương.
        var combat = owner != null ? owner.GetCombat : null;
        if (combat != null)
            combat.DisableAllDamageColliders();

        var animation = owner != null ? owner.GetAnimation : null;
        if (animation != null)
            animation.Play(DeathAnimation);
        else
            Debug.LogWarning($"{name}: Die() nhưng thiếu owner/Animation để play death anim", this);
    }
    #endregion

    #region Hit Reaction
    protected enum HitDirection { Forward, Backward, Left, Right }

    protected virtual void PlayHitReaction(Vector3 damageSourcePosition)
    {
        var animation = owner != null ? owner.GetAnimation : null;
        if (animation == null) return;

        // null = character này không có hit reaction -> bỏ qua.
        AnimationAction? hit = ResolveHitReaction(damageSourcePosition);
        if (hit.HasValue)
            animation.Play(hit.Value);
    }

    // HOOK chính để override:
    // - Mặc định: hit theo 4 hướng (Player).
    // - Override trả 1 clip cố định nếu character chỉ có 1 anim.
    // - Override trả null nếu character không có hit reaction.
    protected virtual AnimationAction? ResolveHitReaction(Vector3 damageSourcePosition)
    {
        return GetHitAnimation(ComputeHitDirection(damageSourcePosition));
    }

    // Tính hướng bị đánh dựa trên vị trí nguồn damage.
    protected HitDirection ComputeHitDirection(Vector3 damageSourcePosition)
    {
        Vector3 toSource = damageSourcePosition - transform.position;
        toSource.y = 0f;

        if (toSource.sqrMagnitude < 0.0001f)
            return HitDirection.Forward;

        Vector3 local = transform.InverseTransformDirection(toSource.normalized);

        if (Mathf.Abs(local.z) >= Mathf.Abs(local.x))
            return local.z >= 0f ? HitDirection.Forward : HitDirection.Backward;

        return local.x >= 0f ? HitDirection.Left : HitDirection.Right;
    }

    protected virtual AnimationAction GetHitAnimation(HitDirection dir)
    {
        switch (dir)
        {
            case HitDirection.Forward: return CharacterAnimations.HitForward;
            case HitDirection.Backward: return CharacterAnimations.HitBackward;
            case HitDirection.Left: return CharacterAnimations.HitLeft;
            case HitDirection.Right: return CharacterAnimations.HitRight;
            default: return CharacterAnimations.HitForward;
        }
    }
    #endregion

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, groundCheckSphereRadius);
    }
#endif
}