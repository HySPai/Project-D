using UnityEngine;

public abstract class CharacterBase : MonoBehaviour, IDamageable
{
    [SerializeField] protected float maxHp = 100;

    protected float currentHp;

    protected virtual void Awake()
    {
        currentHp = maxHp;
    }

    public virtual void TakeDamage(float damage)
    {
        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    protected abstract void Die();
}