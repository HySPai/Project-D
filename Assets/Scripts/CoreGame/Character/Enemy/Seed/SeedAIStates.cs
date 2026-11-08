using UnityEngine;

/// <summary>Seed idle slot: strolls between random points around its spawn, pausing at each, until it spots the player.</summary>
public class SeedWanderState : IEnemyState
{
    private readonly SeedController ctx;
    private bool paused;
    private float resumeTime;

    public SeedWanderState(SeedController ctx) => this.ctx = ctx;

    public void Enter()
    {
        ctx.EnemyState.ClearTarget();
        ctx.SeedAnim.SetHorizontalTarget(0f);
        ctx.EnemyMovement.SetAutoRotation(true);
        GoToNewWanderPoint();
    }

    public void Tick()
    {
        if (ctx.TryAcquireTarget()) return;

        if (paused)
        {
            if (Time.time >= resumeTime) GoToNewWanderPoint();
            return;
        }

        if (!ctx.EnemyMovement.HasReachedDestination(ctx.SeedState.WanderArriveThreshold)) return;

        ctx.EnemyMovement.Stop();
        paused = true;
        resumeTime = Time.time + Random.Range(ctx.SeedState.WanderPauseMin, ctx.SeedState.WanderPauseMax);
    }

    public void Exit() { }

    private void GoToNewWanderPoint()
    {
        paused = false;
        ctx.EnemyMovement.MoveTo(ctx.SeedState.GetRandomWanderPoint(), ctx.SeedState.WanderSpeed);
    }
}

/// <summary>
/// Seed chase slot: waits facing the player while the attack cools down, then locks the player's position and charges
/// straight at it, attacking on arrival or as soon as the player is in range.
/// </summary>
public class SeedChargeState : IEnemyState
{
    private readonly SeedController ctx;
    private bool charging;
    private bool waiting;

    public SeedChargeState(SeedController ctx) => this.ctx = ctx;

    public void Enter()
    {
        charging = false;
        waiting = false;
    }

    public void Tick()
    {
        SeedState s = ctx.SeedState;
        Transform player = s.Target;
        if (player == null) { ctx.ChangeState(ctx.ReturnState); return; }

        float sqrDist = (ctx.transform.position - player.position).sqrMagnitude;
        if (sqrDist > s.GiveUpDistance * s.GiveUpDistance) { ctx.ChangeState(ctx.ReturnState); return; }

        if (!ctx.EnemyCombat.CanAttack)
        {
            BeginWait();
            ctx.EnemyMovement.FaceTowards(player.position, s.RotateSpeed);
            return;
        }

        if (!charging)
        {
            BeginCharge(player.position);
            return;
        }

        bool reachedSpot = ctx.EnemyMovement.HasReachedDestination(s.ChargeArriveThreshold);
        if (reachedSpot || sqrDist <= s.AttackRange * s.AttackRange)
            ctx.ChangeState(ctx.AttackState);
    }

    public void Exit()
    {
        ctx.SeedAnim.SetHorizontalTarget(0f);
        ctx.EnemyMovement.SetAutoRotation(true);
    }

    private void BeginCharge(Vector3 destination)
    {
        charging = true;
        waiting = false;
        ctx.EnemyMovement.SetAutoRotation(true);
        ctx.SeedAnim.SetHorizontalTarget(1f);
        ctx.EnemyMovement.MoveTo(destination, ctx.SeedState.ChaseSpeed);
    }

    private void BeginWait()
    {
        if (waiting) return;

        waiting = true;
        charging = false;
        ctx.EnemyMovement.SetAutoRotation(false);
        ctx.EnemyMovement.Stop();
        ctx.SeedAnim.SetHorizontalTarget(0f);
    }
}

/// <summary>Seed attack slot: one strike in place, then back to the charge loop.</summary>
public class SeedAttackState : IEnemyState
{
    private readonly SeedController ctx;
    private bool struck;

    public SeedAttackState(SeedController ctx) => this.ctx = ctx;

    public void Enter()
    {
        ctx.SeedAnim.SetHorizontalTarget(0f);
        ctx.EnemyMovement.SetAutoRotation(false);
        ctx.EnemyMovement.BeginRootMotion();
        ctx.EnemyCombat.Attack();
        struck = ctx.EnemyState.IsAttacking;
    }

    public void Tick()
    {
        if (ctx.SeedState.Target == null) { ctx.ChangeState(ctx.ReturnState); return; }

        if (!struck || !ctx.EnemyState.IsAttacking)
            ctx.ChangeState(ctx.ChaseState);
    }

    public void Exit() => ctx.EnemyMovement.EndRootMotion();
}
