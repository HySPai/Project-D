using UnityEngine;

public abstract class CharacterCombatBase : MonoBehaviour
{
    protected CharacterStateBase state;
    [SerializeField] protected Collider colAttack;

    public virtual void Initialize(CharacterStateBase state)
    {
        this.state = state;
    }

    public abstract void Attack();

    public virtual void EndAttack()
    {
        if (state != null)
        {
            state.SetAttacking(false);
        }
    }
}