using UnityEngine;

public abstract class CharacterStateBase : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected float maxHp = 100;

    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float rotateSpeed = 12f;

    protected float currentHp;

    protected bool isDead;
    protected bool isAttacking;

    public float MaxHp => maxHp;
    public float CurrentHp => currentHp;

    public float MoveSpeed => moveSpeed;
    public float RotateSpeed => rotateSpeed;

    public bool IsDead => isDead;
    public bool IsAttacking => isAttacking;

    protected virtual void Awake()
    {
        currentHp = maxHp;
    }

    public virtual void SetMoveSpeed(float value)
    {
        moveSpeed = value;
    }

    public virtual void SetRotateSpeed(float value)
    {
        rotateSpeed = value;
    }

    public virtual void SetAttacking(bool value)
    {
        isAttacking = value;
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;

        if (currentHp <= 0)
        {
            currentHp = 0;
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
    }
}