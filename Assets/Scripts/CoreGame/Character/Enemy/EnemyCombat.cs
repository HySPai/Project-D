using UnityEngine;

public class EnemyCombat : CharacterCombatBase
{
    private CharacterControllerBase character;
    private EnemyState enemyState;

    public void Initialize(CharacterStateBase state, CharacterControllerBase character)
    {
        base.Initialize(state);
        this.character = character;
        this.enemyState = state as EnemyState;

        // Cho phép đánh ngay đòn đầu (không phải chờ hết 1 cooldown khi vừa spawn).
        lastAttackTime = -enemyState.AttackCooldown;
    }

    public bool CanAttack => Time.time - lastAttackTime >= enemyState.AttackCooldown;

    public override void Attack()
    {
        if (state == null || state.IsDead) return;
        if (state.IsAttacking) return;
        if (!CanAttack) return;

        lastAttackTime = Time.time;

        colAttack?.SetDamage(enemyState.AttackDamage);
        PlayAttackAnimation(enemyState.AttackAnimationName, character);
    }
}