using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    [SerializeField] private Collider damageCollider;
    public Collider GetCollider => damageCollider;

    private CharacterStateBase ownerState;
    private float currentDamage;

    public void Initialize(CharacterStateBase ownerState)
    {
        this.ownerState = ownerState;
    }

    public void SetDamage(float damage)
    {
        currentDamage = damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        CharacterStateBase targetState = other.GetComponent<CharacterStateBase>();
        if (targetState == null) return;
        if (targetState == ownerState) return;

        targetState.TakeDamage(currentDamage);
    }
}