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

#region SeedChargeState (lao thẳng tới điểm đã chốt rồi đánh khi đến nơi)
public class SeedChargeState : IEnemyState
{
    private readonly SeedController ctx;
    private bool charging;   // đang lao tới điểm đã chốt
    private bool waiting;    // đang đứng chờ hết cooldown

    public SeedChargeState(SeedController ctx) => this.ctx = ctx;

    public void Enter()
    {
        charging = false;
        waiting = false;
        // Horizontal do BeginCharge/BeginWait set trong Tick.
    }

    public void Tick()
    {
        SeedState s = ctx.SeedState;
        Transform player = s.Target;
        if (player == null) { ctx.ChangeState(ctx.ReturnState); return; }

        float distToPlayer = Vector3.Distance(ctx.transform.position, player.position);
        if (distToPlayer > s.GiveUpDistance) { ctx.ChangeState(ctx.ReturnState); return; }

        // Chưa hết cooldown -> đứng chờ, ngoảnh mặt về player.
        if (!ctx.EnemyCombat.CanAttack)
        {
            BeginWait();
            ctx.EnemyMovement.FaceTowards(player.position, s.RotateSpeed);
            return;
        }

        // Hết cooldown mà chưa lao -> CHỐT vị trí player rồi lao thẳng tới.
        if (!charging)
        {
            BeginCharge();
            return;
        }

        // Đang lao: TỚI GẦN ĐIỂM ĐÃ CHỐT -> đánh (dù player có dời đi hay không).
        // Hoặc player lọt vào tầm giữa đường -> đánh luôn cho nhạy.
        bool reachedSpot = ctx.EnemyMovement.HasReachedDestination(s.ChargeArriveThreshold);
        bool playerInRange = distToPlayer <= s.AttackRange;
        if (reachedSpot || playerInRange)
            ctx.ChangeState(ctx.AttackState);
    }

    public void Exit()
    {
        // Về Return/Attack: đưa Horizontal về 0 và trả lại tự xoay cho locomotion.
        ctx.SeedAnim.SetHorizontalTarget(0f);
        ctx.EnemyMovement.SetAutoRotation(true);
    }

    private void BeginCharge()
    {
        charging = true;
        waiting = false;
        ctx.EnemyMovement.SetAutoRotation(true);   // lao tới đâu xoay tới đó
        ctx.SeedAnim.SetHorizontalTarget(1f);      // đuổi -> Horizontal 1

        // CHỐT vị trí player tại thời điểm này rồi chạy thẳng tới (không bám đuổi).
        Vector3 chargeDest = ctx.SeedState.Target.position;
        ctx.EnemyMovement.MoveTo(chargeDest, ctx.SeedState.ChaseSpeed);
    }

    private void BeginWait()
    {
        if (waiting) return; // đã đang chờ
        waiting = true;
        charging = false;
        ctx.EnemyMovement.SetAutoRotation(false);  // tự ngoảnh về player bằng FaceTowards
        ctx.EnemyMovement.Stop();
        ctx.SeedAnim.SetHorizontalTarget(0f);      // đứng chờ -> Horizontal 0, isMoving false
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