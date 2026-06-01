using UnityEngine;

public abstract class CharacterControllerBase : MonoBehaviour, ICharacter
{
    public abstract CharacterStateBase GetState { get; }
    public abstract CharacterMovementBase GetMovement { get; }
    public abstract CharacterCombatBase GetCombat { get; }
    public abstract CharacterAnimationBase GetAnimation { get; }
}