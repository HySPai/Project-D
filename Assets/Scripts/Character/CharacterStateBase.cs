using UnityEngine;

public abstract class CharacterStateBase : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected float maxHp = 100f;

    [Header("Movement Config")]
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float runSpeed = 8f;
    [SerializeField] protected float rotateSpeed = 12f;

    [Header("Animation Config")]
    [SerializeField] protected float moveSmoothSpeed = 8f;

    [Header("Runtime Values")]
    [SerializeField] protected float currentHp;
    [SerializeField] protected float currentMoveSpeed;
    [SerializeField] protected float targetMoveSpeed;

    [Header("Runtime States")]
    [SerializeField] protected bool isDead;
    [SerializeField] protected bool isAttacking;
    [SerializeField] protected bool isRunning;

    [SerializeField] private bool applyRootMotion;
    [SerializeField] private bool canRotate = true;
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool isRolling;

    public bool ApplyRootMotion => applyRootMotion;
    public bool CanRotate => canRotate;
    public bool CanMove => canMove;
    public bool IsRolling => isRolling;

    public void SetApplyRootMotion(bool value)
    {
        applyRootMotion = value;
    }

    public void SetCanRotate(bool value)
    {
        canRotate = value;
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    public void SetRolling(bool value)
    {
        isRolling = value;
    }

    public float MaxHp => maxHp;
    public float CurrentHp => currentHp;

    public float MoveSpeed => moveSpeed;
    public float RunSpeed => runSpeed;
    public float RotateSpeed => rotateSpeed;

    public float MoveSmoothSpeed => moveSmoothSpeed;

    public float CurrentMoveSpeed => currentMoveSpeed;
    public float TargetMoveSpeed => targetMoveSpeed;

    public bool IsDead => isDead;
    public bool IsAttacking => isAttacking;
    public bool IsRunning => isRunning;

    protected virtual void Awake()
    {
        currentHp = maxHp;
        currentMoveSpeed = 0f;
        targetMoveSpeed = 0f;
    }

    public virtual void SetRunning(bool value)
    {
        isRunning = value;
    }

    public virtual void SetAttacking(bool value)
    {
        isAttacking = value;
    }

    public virtual void SetCurrentMoveSpeed(float value)
    {
        currentMoveSpeed = value;
    }

    public virtual void SetTargetMoveSpeed(float value)
    {
        targetMoveSpeed = value;
    }

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

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;

        if (currentHp <= 0f)
        {
            currentHp = 0f;
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
    }
}