using UnityEngine;

public interface ICharacter
{
    CharacterStateBase GetState { get; }
    CharacterMovementBase GetMovement { get; }
    CharacterCombatBase GetCombat { get; }
    CharacterAnimationBase GetAnimation { get; }
}

public interface IDamageable
{
    DamageResult ReceiveDamage(in DamageInfo info);
}
public interface IMovable
{
    void Move();
    void SetInput(Vector2 input);
}