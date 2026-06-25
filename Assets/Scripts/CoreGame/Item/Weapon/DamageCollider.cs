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
        if (currentDamage <= 0f) return;

        // Lấy state ở parent để hitbox đặt ở child vẫn resolve được.
        CharacterStateBase targetState = other.GetComponentInParent<CharacterStateBase>();
        if (targetState == null) return;
        if (targetState == ownerState) return;

        // Nguồn damage = vị trí vật mang collider (vũ khí, viên đạn) để tính hướng hit.
        targetState.TakeDamage(currentDamage, transform.position);
    }
}