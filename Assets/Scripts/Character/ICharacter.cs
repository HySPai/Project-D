using UnityEngine;

public interface ICharacter
{
    CharacterStateBase State { get; }
    CharacterMovementBase Movement { get; }
    CharacterCombatBase Combat { get; }
    CharacterAnimationBase Animation { get; }
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