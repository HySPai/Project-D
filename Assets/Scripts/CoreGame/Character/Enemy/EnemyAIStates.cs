using UnityEngine;
public interface IEnemyState
{
    void Enter();
    void Tick();
    void Exit();
}

// Đứng tại home, quét tìm player. Thấy -> Chase.
public class EnemyIdleState : IEnemyState
{
    private readonly EnemyController ctx;
    public EnemyIdleState(EnemyController ctx) => this.ctx = ctx;

    public void Enter()
    {
        ctx.EnemyMovement.Stop();
        ctx.EnemyState.ClearTarget();
    }

    public void Tick()
    {
        var target = ctx.Vision.DetectTarget();
        if (target != null)
        {
            ctx.EnemyState.SetTarget(target);
            ctx.ChangeState(ctx.ChaseState);
        }
    }

    public void Exit() { }
}

// Đuổi target. Player vượt quá GiveUpDistance -> Return.
public class EnemyChaseState : IEnemyState
{
    private readonly EnemyController ctx;
    public EnemyChaseState(EnemyController ctx) => this.ctx = ctx;

    public void Enter() { }

    public void Tick()
    {
        var s = ctx.EnemyState;
        var target = s.Target;

        if (target == null) { ctx.ChangeState(ctx.ReturnState); return; }

        float dist = Vector3.Distance(ctx.transform.position, target.position);
        if (dist > s.GiveUpDistance)         // player chạy thoát
        {
            ctx.ChangeState(ctx.ReturnState);
            return;
        }
        if (dist <= s.AttackRange)
        {
            ctx.ChangeState(ctx.AttackState);
            return;
        }

        ctx.EnemyMovement.Chase(target, s.ChaseSpeed, s.StoppingDistance);
        ctx.EnemyMovement.Chase(target, s.ChaseSpeed, s.StoppingDistance);
    }

    public void Exit() { }
}

// Quay về home. Trên đường về thấy player lại -> Chase. Về tới nơi -> Idle.
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
        var target = ctx.Vision.DetectTarget();
        if (target != null)
        {
            ctx.EnemyState.SetTarget(target);
            ctx.ChangeState(ctx.ChaseState);
            return;
        }

        if (ctx.EnemyMovement.HasReachedDestination(ctx.EnemyState.HomeReachedThreshold))
            ctx.ChangeState(ctx.IdleState);
    }

    public void Exit() { }
}
// Đứng đánh trong tầm. Player ra xa -> Chase. Mất target -> Return.
public class EnemyAttackState : IEnemyState
{
    private const float ExitBuffer = 0.5f; // hysteresis chống rung ở ranh giới tầm đánh

    private readonly EnemyController ctx;
    public EnemyAttackState(EnemyController ctx) => this.ctx = ctx;

    public void Enter() => ctx.EnemyMovement.BeginRootMotion();
    public void Exit() => ctx.EnemyMovement.EndRootMotion();

    public void Tick()
    {
        var s = ctx.EnemyState;
        var target = s.Target;

        if (target == null) { ctx.ChangeState(ctx.ReturnState); return; }

        // Đang diễn đòn -> để anim/root motion chạy, không can thiệp.
        if (s.IsAttacking) return;

        float dist = Vector3.Distance(ctx.transform.position, target.position);

        // Player thoát khỏi tầm đánh -> đuổi tiếp.
        if (dist > s.AttackRange + ExitBuffer)
        {
            ctx.ChangeState(ctx.ChaseState);
            return;
        }

        // Trong tầm: xoay theo target giữa các đòn, đủ cooldown thì tung đòn.
        FaceTarget(target);
        if (ctx.EnemyCombat.CanAttack)
            ctx.EnemyCombat.Attack();
    }

    private void FaceTarget(Transform target)
    {
        Vector3 dir = target.position - ctx.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        ctx.transform.rotation = Quaternion.Slerp(
            ctx.transform.rotation, Quaternion.LookRotation(dir),
            ctx.EnemyState.RotateSpeed * Time.deltaTime);
    }
}