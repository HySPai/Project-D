using UnityEngine;

public class EnemyState : CharacterStateBase
{
    private EnemyController enemy;

    protected override AnimationAction GetHitAnimation(HitDirection dir)
    {
        switch (dir)
        {
            case HitDirection.Forward: return CharacterAnimations.HitForward;
            case HitDirection.Backward: return CharacterAnimations.HitBackward;
            case HitDirection.Left: return CharacterAnimations.HitLeft;
            case HitDirection.Right: return CharacterAnimations.HitRight;
            default: return CharacterAnimations.HitForward;
        }
    }

    protected override void Die()
    {
        base.Die();
        enemy = owner as EnemyController;
        enemy.GetLegsAnim.enabled = false;
    }
}