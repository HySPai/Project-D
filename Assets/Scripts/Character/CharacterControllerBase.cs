using UnityEngine;

public abstract class CharacterControllerBase : MonoBehaviour, ICharacter
{
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected CapsuleCollider capsule;

    public Rigidbody Rigidbody => rb;
    public CapsuleCollider Capsule => capsule;

    public abstract CharacterStateBase GetState { get; }
    public abstract CharacterMovementBase GetMovement { get; }
    public abstract CharacterCombatBase GetCombat { get; }
    public abstract CharacterAnimationBase GetAnimation { get; }
}