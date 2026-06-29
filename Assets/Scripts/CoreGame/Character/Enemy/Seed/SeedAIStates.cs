using UnityEngine;

// Các state riêng của Seed, đều nhận SeedController.
// Lắp vào FSM qua SeedController.BuildStates():
//   IdleState  = SeedWanderState   (đi dạo quanh spawn, Horizontal 0)
//   ChaseState = SeedChargeState   (lao thẳng tới player -> đánh -> chờ cooldown -> lao tiếp, Horizontal 1)
//   AttackState = SeedAttackState  (đánh tại chỗ 1 nhịp rồi về charge)
// ReturnState dùng EnemyReturnState gốc (về home rồi quay lại dạo).

#region SeedWanderState (đi dạo quanh chỗ spawn)
public class SeedWanderState : IEnemyState
{
    private readonly SeedController ctx;

    private bool paused;       // true = đang dừng nghỉ; false = đang đi tới điểm dạo
    private float resumeTime;  // thời điểm hết nghỉ

    public SeedWanderState(SeedController ctx) => this.ctx = ctx;

    public void Enter()
    {
        ctx.EnemyState.ClearTarget();
        ctx.SeedAnim.SetHorizontalTarget(0f);    // đi dạo -> Horizontal 0
        ctx.EnemyMovement.SetAutoRotation(true); // đi tới đâu xoay tới đó
        GoToNewWanderPoint();
    }

    public void Tick()
    {
        // Phát hiện player -> chuyển sang lao tới tấn công.
        Transform target = ctx.Vision.DetectTarget();
        if (target != null)
        {
            ctx.EnemyState.SetTarget(target);
            ctx.ChangeState(ctx.ChaseState);
            return;
        }

        if (paused)
        {
            // Đang nghỉ (đứng yên -> isMoving false do agent dừng) -> hết giờ thì đi tiếp.
            if (Time.time >= resumeTime)
                GoToNewWanderPoint();
            return;
        }

        // Đang đi tới điểm dạo -> tới nơi thì dừng nghỉ một lúc.
        if (ctx.EnemyMovement.HasReachedDestination(ctx.SeedState.WanderArriveThreshold))
        {
            ctx.EnemyMovement.Stop();
            paused = true;
            resumeTime = Time.time +
                Random.Range(ctx.SeedState.WanderPauseMin, ctx.SeedState.WanderPauseMax);
        }
    }

    public void Exit() { }

    private void GoToNewWanderPoint()
    {
        paused = false;
        Vector3 point = ctx.SeedState.GetRandomWanderPoint();
        ctx.EnemyMovement.MoveTo(point, ctx.SeedState.WanderSpeed); // đi bộ tới điểm
    }
}
#endregion

#region SeedChargeState (lao thẳng tới player rồi đánh, KHÔNG bám đuổi)
public class SeedChargeState : IEnemyState
{
    private readonly SeedController ctx;
    private bool charging; // true = đang lao tới; false = đang đứng chờ cooldown

    public SeedChargeState(SeedController ctx) => this.ctx = ctx;

    public void Enter()
    {
        charging = false; // để Tick quyết định: đủ cooldown thì lao, chưa thì đứng chờ
        ctx.SeedAnim.SetHorizontalTarget(1f); // đuổi -> Horizontal 1
    }

    public void Tick()
    {
        SeedState s = ctx.SeedState;
        Transform player = s.Target;

        if (player == null) { ctx.ChangeState(ctx.ReturnState); return; }

        float dist = Vector3.Distance(ctx.transform.position, player.position);
        if (dist > s.GiveUpDistance) { ctx.ChangeState(ctx.ReturnState); return; }

        // Vào tầm + sẵn sàng -> đánh.
        if (ctx.EnemyCombat.CanAttack && dist <= s.AttackRange)
        {
            ctx.ChangeState(ctx.AttackState);
            return;
        }

        if (ctx.EnemyCombat.CanAttack)
        {
            // Sẵn sàng đánh nhưng chưa tới -> LAO THẲNG tới vị trí player đã chốt.
            // Chỉ chốt điểm khi bắt đầu lao và khi đã tới điểm cũ (player dời chỗ),
            // KHÔNG cập nhật mỗi frame -> "đi thẳng đến đó", không bám đuổi.
            if (!charging || ctx.EnemyMovement.HasReachedDestination(s.ChargeArriveThreshold))
                BeginCharge();
        }
        else
        {
            // Chưa hết cooldown -> đứng yên (Horizontal 0, isMoving false), ngoảnh về player.
            BeginWait();
            ctx.EnemyMovement.FaceTowards(player.position, s.RotateSpeed);
        }
    }

    public void Exit()
    {
        // Về Return/Attack thì đưa Horizontal về 0 và trả lại tự xoay cho locomotion.
        ctx.SeedAnim.SetHorizontalTarget(0f);
        ctx.EnemyMovement.SetAutoRotation(true);
    }

    private void BeginCharge()
    {
        charging = true;
        ctx.EnemyMovement.SetAutoRotation(true);   // lao tới đâu xoay tới đó
        ctx.SeedAnim.SetHorizontalTarget(1f);
        Transform player = ctx.SeedState.Target;
        if (player != null)
            ctx.EnemyMovement.MoveTo(player.position, ctx.SeedState.ChaseSpeed); // chốt điểm + chạy
    }

    private void BeginWait()
    {
        if (!charging) return; // đã đang chờ rồi
        charging = false;
        ctx.EnemyMovement.SetAutoRotation(false);  // tự xoay mặt về player bằng FaceTowards
        ctx.EnemyMovement.Stop();
        ctx.SeedAnim.SetHorizontalTarget(0f);      // đứng chờ -> Horizontal 0
    }
}
#endregion

#region SeedAttackState (đánh tại chỗ 1 nhịp rồi về charge)
public class SeedAttackState : IEnemyState
{
    private readonly SeedController ctx;
    private bool struck;

    public SeedAttackState(SeedController ctx) => this.ctx = ctx;

    public void Enter()
    {
        ctx.SeedAnim.SetHorizontalTarget(0f);     // đứng đánh -> Horizontal 0
        ctx.EnemyMovement.SetAutoRotation(false);
        ctx.EnemyMovement.BeginRootMotion();       // dừng agent, đứng tại chỗ đánh
        ctx.EnemyCombat.Attack();
        struck = ctx.EnemyState.IsAttacking;
    }

    public void Tick()
    {
        Transform player = ctx.SeedState.Target;
        if (player == null) { ctx.ChangeState(ctx.ReturnState); return; }

        // Không tung được đòn -> về charge.
        if (!struck) { ctx.ChangeState(ctx.ChaseState); return; }

        // Đánh xong (ResetActionFlag clear IsAttacking) -> về charge:
        // charge sẽ chờ hết cooldown rồi lại lao thẳng tới vị trí player mới.
        if (!ctx.EnemyState.IsAttacking)
            ctx.ChangeState(ctx.ChaseState);
    }

    public void Exit() => ctx.EnemyMovement.EndRootMotion();
}
#endregion