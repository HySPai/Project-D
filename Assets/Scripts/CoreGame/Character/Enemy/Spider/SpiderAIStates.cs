using UnityEngine;

// Các state riêng của Spider. Đều nhận SpiderController để lấy được SpiderState
// (chứa tham số kite) và các thành phần dùng chung (EnemyMovement, EnemyCombat...).
//
// Lắp vào FSM qua SpiderController.BuildStates():
//   ChaseState  = SpiderKiteState     ("đuổi" = giữ khoảng cách + lượn vòng)
//   AttackState = SpiderAttackState   ("tấn công" = đánh tại chỗ, KHÔNG lao vào)
// nên vẫn tái dùng nguyên bộ chuyển trạng thái Idle -> Chase -> Attack sẵn có.

#region SpiderKiteState (giữ khoảng cách & lượn quanh player)
public class SpiderKiteState : IEnemyState
{
    private readonly SpiderController ctx;

    private int strafeDir;       // +1 / -1: chiều lượn quanh player
    private float nextFlipTime;  // thời điểm đổi chiều lượn kế tiếp

    public SpiderKiteState(SpiderController ctx) => this.ctx = ctx;

    public void Enter()
    {
        // Tự xoay người về phía player (agent không tự xoay theo hướng đi nữa).
        ctx.EnemyMovement.SetAutoRotation(false);

        strafeDir = Random.value < 0.5f ? -1 : 1;
        ScheduleFlip();
    }

    public void Tick()
    {
        SpiderState s = ctx.SpiderState;
        Transform player = s.Target;

        // Mất mục tiêu -> quay về tổ.
        if (player == null)
        {
            ctx.ChangeState(ctx.ReturnState);
            return;
        }

        float dist = Vector3.Distance(ctx.transform.position, player.position);

        // Player chạy quá xa -> bỏ cuộc.
        if (dist > s.GiveUpDistance)
        {
            ctx.ChangeState(ctx.ReturnState);
            return;
        }

        // SẴN SÀNG ĐÁNH (hết cooldown) -> tiến dần vào player để đánh.
        if (ctx.EnemyCombat.CanAttack)
        {
            if (dist <= s.AttackRange)
            {
                ctx.ChangeState(ctx.AttackState); // đã vào tầm -> đánh tại chỗ
                return;
            }

            // Tiến thẳng vào player, dừng ngay khi sắp vào tầm đánh.
            ctx.EnemyMovement.Chase(player, s.ChaseSpeed, s.AttackRange * 0.9f);
            ctx.EnemyMovement.FaceTowards(player.position, s.TurnSpeed);
            return;
        }

        // ĐANG COOLDOWN -> giữ khoảng cách + lượn vòng quanh player.

        // Thỉnh thoảng đổi chiều lượn cho đỡ máy móc.
        if (Time.time >= nextFlipTime)
        {
            strafeDir = -strafeDir;
            ScheduleFlip();
        }

        // Giữ ở vành preferredDistance và lượn theo tiếp tuyến; nếu player ép sát,
        // thành phần radial trong KiteAround tự đẩy Spider ra.
        ctx.EnemyMovement.KiteAround(player, s.PreferredDistance, s.KiteSpeed, strafeDir, s.StrafeLookAhead);
        ctx.EnemyMovement.FaceTowards(player.position, s.TurnSpeed);
    }

    public void Exit()
    {
        // Bật lại tự xoay theo hướng đi (dùng RotateSpeed) cho các state Idle/Return.
        ctx.EnemyMovement.SetAutoRotation(true);
    }

    private void ScheduleFlip()
    {
        SpiderState s = ctx.SpiderState;
        nextFlipTime = Time.time + Random.Range(s.StrafeFlipMin, s.StrafeFlipMax);
    }
}
#endregion

#region SpiderAttackState (đánh tại chỗ, giữ khoảng cách - KHÔNG lao vào)
public class SpiderAttackState : IEnemyState
{
    private readonly SpiderController ctx;
    private bool struck; // đòn đã thực sự kích hoạt chưa

    public SpiderAttackState(SpiderController ctx) => this.ctx = ctx;

    public void Enter()
    {
        // Đứng yên tại vị trí kite hiện tại rồi đánh — KHÔNG tiến lại gần player.
        ctx.EnemyMovement.SetAutoRotation(false);
        ctx.EnemyMovement.BeginRootMotion(); // dừng agent, giữ nguyên vị trí

        // Khi vào đây nhện đã ngoảnh sẵn về player (kite xoay liên tục).
        ctx.EnemyCombat.Attack();
        struck = ctx.EnemyState.IsAttacking;
    }

    public void Tick()
    {
        Transform player = ctx.SpiderState.Target;
        if (player == null)
        {
            ctx.ChangeState(ctx.ReturnState);
            return;
        }

        // Không tung được đòn -> về kite ngay.
        if (!struck)
        {
            ctx.ChangeState(ctx.ChaseState);
            return;
        }

        // Đòn diễn xong (ResetActionFlag clear IsAttacking) -> quay lại giữ khoảng cách.
        if (!ctx.EnemyState.IsAttacking)
            ctx.ChangeState(ctx.ChaseState);
    }

    public void Exit()
    {
        ctx.EnemyMovement.EndRootMotion();
    }
}
#endregion