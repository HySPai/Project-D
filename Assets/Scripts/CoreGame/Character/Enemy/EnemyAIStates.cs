using UnityEngine;

/// <summary>One state of an enemy's behaviour machine, driven by EnemyController.</summary>
public interface IEnemyState
{
    void Enter();
    void Tick();
    void Exit();
}

/// <summary>Stands at home and scans for the player; seeing one starts the chase.</summary>
public class EnemyIdleState : IEnemyState
{
    private readonly EnemyController ctx;
    public EnemyIdleState(EnemyController ctx) => this.ctx = ctx;

    public void Enter()
    {
        ctx.EnemyMovement.Stop();
        ctx.EnemyState.ClearTarget();
    }

    public void Tick() => ctx.TryAcquireTarget();

    public void Exit() { }
}

/// <summary>Runs at the target; gives up beyond GiveUpDistance and attacks once in range.</summary>
public class EnemyChaseState : IEnemyState
{
    private readonly EnemyController ctx;
    public EnemyChaseState(EnemyController ctx) => this.ctx = ctx;

    public void Enter() { }

    public void Tick()
    {
        EnemyState s = ctx.EnemyState;
        Transform target = s.Target;
        if (target == null) { ctx.ChangeState(ctx.ReturnState); return; }

        float sqrDist = (ctx.transform.position - target.position).sqrMagnitude;
        if (sqrDist > s.GiveUpDistance * s.GiveUpDistance) { ctx.ChangeState(ctx.ReturnState); return; }
        if (sqrDist <= s.AttackRange * s.AttackRange) { ctx.ChangeState(ctx.AttackState); return; }

        ctx.EnemyMovement.Chase(target, s.ChaseSpeed, s.StoppingDistance);
    }

    public void Exit() { }
}

/// <summary>Walks home; spotting the player on the way restarts the chase, arriving goes back to idle.</summary>
public class EnemyReturnState : IEnemyState
{
    private readonly EnemyController ctx;
    public EnemyReturnState(EnemyController ctx) => this.ctx = ctx;

    public void Enter()
    {
        ctx.EnemyState.ClearTarget();
        ctx.EnemyMovement.MoveTo(ctx.EnemyState.HomePosition, ctx.EnemyState.ReturnSpeed);
    }

    public void Tick()
    {
        if (ctx.TryAcquireTarget()) return;

        if (ctx.EnemyMovement.HasReachedDestination(ctx.EnemyState.HomeReachedThreshold))
            ctx.ChangeState(ctx.IdleState);
    }

    public void Exit() { }
}

/// <summary>Stands and attacks while the target stays in range, turning toward it between swings.</summary>
public class EnemyAttackState : IEnemyState
{
    private const float ExitBuffer = 0.5f;

    private readonly EnemyController ctx;
    public EnemyAttackState(EnemyController ctx) => this.ctx = ctx;

    public void Enter() => ctx.EnemyMovement.BeginRootMotion();
    public void Exit() => ctx.EnemyMovement.EndRootMotion();

    public void Tick()
    {
        EnemyState s = ctx.EnemyState;
        Transform target = s.Target;
        if (target == null) { ctx.ChangeState(ctx.ReturnState); return; }
        if (s.IsAttacking) return;

        float leaveRange = s.AttackRange + ExitBuffer;
        if ((ctx.transform.position - target.position).sqrMagnitude > leaveRange * leaveRange)
        {
            ctx.ChangeState(ctx.ChaseState);
            return;
        }

        ctx.EnemyMovement.FaceTowards(target.position, s.RotateSpeed);
        if (ctx.EnemyCombat.CanAttack)
            ctx.EnemyCombat.Attack();
    }
}
