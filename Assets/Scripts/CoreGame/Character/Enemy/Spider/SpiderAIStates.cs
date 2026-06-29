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

        // Trong tầm đánh + hết cooldown -> tấn công NGAY TẠI CHỖ (không lao vào).
        // Lưu ý: đặt attackRange (trong SO) >= preferredDistance để nhện đánh được
        // từ chính vòng kite mà nó đang giữ.
        if (ctx.EnemyCombat.CanAttack && dist <= s.AttackRange)
        {
            ctx.ChangeState(ctx.AttackState);
            return;
        }

        // Thỉnh thoảng đổi chiều lượn cho đỡ máy móc.
        if (Time.time >= nextFlipTime)
        {
            strafeDir = -strafeDir;
            ScheduleFlip();
        }

        // Giữ ở vành preferredDistance và lượn theo tiếp tuyến; nếu player ép sát,
        // thành phần radial trong KiteAround tự đẩy Spider ra -> luôn "né lại gần".
        ctx.EnemyMovement.KiteAround(player, s.PreferredDistance, s.KiteSpeed, strafeDir, s.StrafeLookAhead);
        ctx.EnemyMovement.FaceTowards(player.position, s.TurnSpeed);
    }

    public void Exit()
    {
        // Trả quyền xoay lại cho agent: các state dùng chung (Idle/Return) cần xoay
        // theo hướng di chuyển.
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