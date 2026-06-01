using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    [SerializeField] private float damage = 25f;

    private CharacterStateBase ownerState;

    private void Awake()
    {
        ownerState = GetComponentInParent<CharacterStateBase>();
    }

    private void OnTriggerEnter(Collider other)
    {
        CharacterStateBase targetState =
            other.GetComponentInParent<CharacterStateBase>();

        if (targetState == null)
            return;

        if (targetState == ownerState)
            return;

        targetState.TakeDamage(damage);
    }
}