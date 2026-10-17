using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    [SerializeField] private Collider damageCollider;
    public Collider GetCollider => damageCollider;

    private CharacterStateBase ownerState;
    private float currentDamage;
    private DamageSource currentSource;
    private ElementType currentElement;
    private bool currentUnblockable;
    private float currentStagger;

    public void Initialize(CharacterStateBase ownerState)
    {
        this.ownerState = ownerState;
    }

    public void SetDamage(
        float points,
        DamageSource source,
        ElementType element = ElementType.None,
        bool unblockable = false,
        float staggerAmount = 0f)
    {
        currentDamage = points;
        currentSource = source;
        currentElement = element;
        currentUnblockable = unblockable;
        currentStagger = staggerAmount;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentDamage <= 0f) return;

        if (!CombatTargetRegistry.TryGet(other, out CharacterControllerBase target)) return;
        if (target == null) return;

        CharacterStateBase targetState = target.GetState;
        if (targetState == null) return;
        if (targetState == ownerState) return;

        CharacterControllerBase attacker = ownerState != null ? ownerState.Owner : null;

        DamageInfo info = new DamageInfo(
            currentDamage, attacker, transform.position,
            currentElement, currentSource, currentUnblockable, currentStagger);
        targetState.ReceiveDamage(in info);
    }
}
