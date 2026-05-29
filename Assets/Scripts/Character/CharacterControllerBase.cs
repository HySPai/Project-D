using UnityEngine;

public abstract class CharacterControllerBase : MonoBehaviour, ICharacter
{
    public abstract CharacterStateBase State { get; }
    public abstract CharacterMovementBase Movement { get; }
    public abstract CharacterCombatBase Combat { get; }
    public abstract CharacterAnimationBase Animation { get; }
}