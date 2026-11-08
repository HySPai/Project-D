using UnityEngine;

/// <summary>
/// Spider chase slot: while the attack is cooling down it circles the player at PreferredDistance, flipping direction
/// now and then; once ready it closes in and switches to SpiderAttackState inside attack range.
/// </summary>
public class SpiderKiteState : IEnemyState
{
    private const float ApproachStopRatio = 0.9f;

    private readonly SpiderController ctx;
    private int strafeDir;
    private float nextFlipTime;

    public SpiderKiteState(SpiderController ctx) => this.ctx = ctx;

    public void Enter()
    {
        ctx.EnemyMovement.SetAutoRotation(false);
        strafeDir = Random.value < 0.5f ? -1 : 1;
        ScheduleFlip();
    }

    public void Tick()
    {
        SpiderState s = ctx.SpiderState;
        Transform player = s.Target;
        if (player == null) { ctx.ChangeState(ctx.ReturnState); return; }

        float sqrDist = (ctx.transform.position - player.position).sqrMagnitude;
        if (sqrDist > s.GiveUpDistance * s.GiveUpDistance) { ctx.ChangeState(ctx.ReturnState); return; }

        if (ctx.EnemyCombat.CanAttack)
        {
            if (sqrDist <= s.AttackRange * s.AttackRange)
            {
                ctx.ChangeState(ctx.AttackState);
                return;
            }

            ctx.EnemyMovement.Chase(player, s.ChaseSpeed, s.AttackRange * ApproachStopRatio);
            ctx.EnemyMovement.FaceTowards(player.position, s.TurnSpeed);
            return;
        }

        if (Time.time >= nextFlipTime)
        {
            strafeDir = -strafeDir;
            ScheduleFlip();
        }

        ctx.EnemyMovement.KiteAround(player, s.PreferredDistance, s.KiteSpeed, strafeDir, s.StrafeLookAhead);
        ctx.EnemyMovement.FaceTowards(player.position, s.TurnSpeed);
    }

    public void Exit() => ctx.EnemyMovement.SetAutoRotation(true);

    private void ScheduleFlip()
    {
        SpiderState s = ctx.SpiderState;
        nextFlipTime = Time.time + Random.Range(s.StrafeFlipMin, s.StrafeFlipMax);
    }
}

/// <summary>Spider attack slot: strikes in place from where it stands, then goes back to circling.</summary>
public class SpiderAttackState : IEnemyState
{
    private readonly SpiderController ctx;
    private bool struck;

    public SpiderAttackState(SpiderController ctx) => this.ctx = ctx;

    public void Enter()
    {
        ctx.EnemyMovement.SetAutoRotation(false);
        ctx.EnemyMovement.BeginRootMotion();
        ctx.EnemyCombat.Attack();
        struck = ctx.EnemyState.IsAttacking;
    }

    public void Tick()
    {
        if (ctx.SpiderState.Target == null) { ctx.ChangeState(ctx.ReturnState); return; }

        if (!struck || !ctx.EnemyState.IsAttacking)
            ctx.ChangeState(ctx.ChaseState);
    }

    public void Exit() => ctx.EnemyMovement.EndRootMotion();
}
