using UnityEngine;

public abstract class CharacterStateBase : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float maxHp = 100f;

    [SerializeField] protected float currentHp;

    public float MaxHp => maxHp;
    public float CurrentHp => currentHp;

    [Header("Movement Config")]
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float runSpeed = 8f;
    [SerializeField] protected float rotateSpeed = 12f;

    public float MoveSpeed => moveSpeed;
    public float RunSpeed => runSpeed;
    public float RotateSpeed => rotateSpeed;

    [Header("Layout")]
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;

    public LayerMask GroundLayer => groundLayer;
    public LayerMask ObstacleLayer => obstacleLayer;

    [Header("Animation Config")]
    [SerializeField] protected float moveSmoothSpeed = 8f;

    public float MoveSmoothSpeed => moveSmoothSpeed;

    [Header("Runtime Movement")]
    [SerializeField] protected float currentMoveSpeed;
    [SerializeField] protected float targetMoveSpeed;

    public float CurrentMoveSpeed => currentMoveSpeed;
    public float TargetMoveSpeed => targetMoveSpeed;

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

    [Header("Character Permissions")]
    [SerializeField] private bool applyRootMotion;
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canRotate = true;

    public bool ApplyRootMotion => applyRootMotion;
    public bool CanMove => canMove;
    public bool CanRotate => canRotate;

    [Header("Root Motion")]
    [SerializeField] protected float rollDistanceMultiplier = 1f;

    public float RollDistanceMultiplier => rollDistanceMultiplier;

    protected CharacterControllerBase owner;

    public CharacterControllerBase Owner => owner;

    public virtual void Initialize(CharacterControllerBase owner)
    {
        this.owner = owner;
    }

    protected virtual void Awake()
    {
        currentHp = maxHp;
        currentMoveSpeed = 0f;
        targetMoveSpeed = 0f;
    }
    #region State Setters
    public virtual void SetRunning(bool value)
    {
        isRunning = value;
    }

    public virtual void SetAttacking(bool value)
    {
        isAttacking = value;
    }

    public virtual void SetRolling(bool value)
    {
        isRolling = value;
    }
    #endregion

    #region Permission Setters

    public void SetApplyRootMotion(bool value)
    {
        applyRootMotion = value;
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    public void SetCanRotate(bool value)
    {
        canRotate = value;
    }

    #endregion

    #region Runtime Setters

    public virtual void SetCurrentMoveSpeed(float value)
    {
        currentMoveSpeed = value;
    }

    public virtual void SetTargetMoveSpeed(float value)
    {
        targetMoveSpeed = value;
    }
    #endregion

    #region Config Setters

    public virtual void SetMoveSpeed(float value)
    {
        moveSpeed = value;
    }

    public virtual void SetRunSpeed(float value)
    {
        runSpeed = value;
    }

    public virtual void SetRotateSpeed(float value)
    {
        rotateSpeed = value;
    }

    #endregion

    #region Health

    public virtual void TakeDamage(float damage)
    {
        if (isDead)
        {
            return;
        }

        if (isInvulnerable)
        {
            return;
        }

        currentHp -= damage;

        if (currentHp <= 0f)
        {
            currentHp = 0f;
            Die();
        }
    }

    public virtual void EnableIsInvulnerable()
    {
        isInvulnerable = true;
    }

    public virtual void DisableIsInvulnerable()
    {
        isInvulnerable = false;
    }

    protected virtual void Die()
    {
        isDead = true;
    }

    #endregion
}