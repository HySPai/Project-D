using UnityEngine;

public class EnemyState : CharacterStateBase
{
    private EnemyController enemy;

    protected override void Die()
    {
        base.Die();
        enemy = owner as EnemyController;
        enemy.GetLegsAnim.enabled = false;
    }
}