using UnityEngine;

public class PlayerAnimation : CharacterAnimationBase
{
    [SerializeField] private string moveParameter = "Move";

    private float currentMoveValue;
    private PlayerState playerState;
    private Rigidbody rb;
    private CapsuleCollider capsule;

    public void Initialize(PlayerState state)
    {
        this.state = state;
        playerState = state;
        base.Initialize(state);

        rb      = state.Owner.Rigidbody;
        capsule = state.Owner.Capsule;
    }

    public override void UpdateAnimation(float moveAmount)
    {
        animator.applyRootMotion = state.ApplyRootMotion;

        float targetMoveValue = ResolveTargetMoveValue(moveAmount);

        currentMoveValue = Mathf.MoveTowards(
            currentMoveValue,
            targetMoveValue,
            state.MoveSmoothSpeed * Time.deltaTime
        );

        animator.SetFloat(moveParameter, currentMoveValue);
    }

    private void OnAnimatorMove()
    {
        if (state == null || !state.ApplyRootMotion) return;

        Vector3 delta = ScaledRootMotionDelta();
        Vector3 targetPos = rb.position + delta;

        // Bám theo mặt đất thực tế: xử lý cả bước LÊN (thang/dốc) lẫn bước XUỐNG,
        // thay vì giữ nguyên y ngang của root motion.
        if (!TryResolveGroundedPosition(targetPos, out Vector3 groundedPos))
            return;

        // Chỉ chặn khi gặp vật cản CAO hơn tầm bước lên (tường thật).
        if (IsBlockedByObstacle(groundedPos))
            return;

        rb.MovePosition(groundedPos);
        rb.MoveRotation(rb.rotation * animator.deltaRotation);
    }

    // Thay cho IsGroundReachable: vừa kiểm tra vừa snap y vào mặt đất.
    // Trả về false nếu không có mặt đất hợp lệ (vực sâu / bậc quá cao).
    private bool TryResolveGroundedPosition(Vector3 position, out Vector3 grounded)
    {
        grounded = position;

        // Bắt đầu raycast từ TRÊN cao đủ để phát hiện được cả bậc đi lên.
        float up = playerState.MaxStepUpHeight + 0.2f;
        Vector3 origin = position + Vector3.up * up;
        float maxDistance = up + playerState.MaxStepDownHeight + 0.2f;

        if (!Physics.Raycast(
                origin,
                Vector3.down,
                out RaycastHit groundHit,
                maxDistance,
                playerState.GroundLayer))
            return false;

        float heightDiff = groundHit.point.y - position.y; // >0: bước lên, <0: bước xuống

        if (heightDiff > playerState.MaxStepUpHeight) return false; // tường/bậc quá cao
        if (-heightDiff > playerState.MaxStepDownHeight) return false; // mép vực

        // Snap y vào đúng mặt đất -> leo bậc thang/dốc mượt mà.
        grounded = new Vector3(position.x, groundHit.point.y, position.z);
        return true;
    }

    private bool IsBlockedByObstacle(Vector3 groundedPosition)
    {
        // Nâng đáy capsule lên qua tầm bước lên: bậc thang/dốc nằm dưới ngưỡng này
        // KHÔNG bị tính là vật cản, chỉ tường thật mới chặn.
        float skin = playerState.MaxStepUpHeight;
        Vector3 bottom = groundedPosition + Vector3.up * (skin + capsule.radius);
        Vector3 top = groundedPosition + Vector3.up * (capsule.height - capsule.radius);

        if (top.y < bottom.y) top = bottom; // an toàn với capsule thấp

        return Physics.CheckCapsule(
            bottom,
            top,
            capsule.radius * 0.9f,
            playerState.GroundLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    private float ResolveTargetMoveValue(float moveAmount)
    {
        if (state == null)       return 0f;
        if (state.IsAttacking)   return 0f;
        return moveAmount;
    }

    private Vector3 ScaledRootMotionDelta()
    {
        Vector3 delta = animator.deltaPosition;

        if (state.IsRolling)
            delta *= state.RollDistanceMultiplier;

        return delta;
    }
}