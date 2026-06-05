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
    void TakeDamage(float damage);
}
public interface IAttacker
{
    void Attack();
}
public interface IMovable
{
    void Move();
    void SetInput(Vector2 input);
}