using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public abstract class CharacterControllerBase : MonoBehaviour, ICharacter
{
    [SerializeField] protected CharacterController characterController;

    public CharacterController CharacterController => characterController;

    public abstract CharacterStateBase GetState { get; }
    public abstract CharacterMovementBase GetMovement { get; }
    public abstract CharacterCombatBase GetCombat { get; }
    public abstract CharacterAnimationBase GetAnimation { get; }

    protected virtual void Reset()
    {
        characterController = GetComponent<CharacterController>();
    }
}