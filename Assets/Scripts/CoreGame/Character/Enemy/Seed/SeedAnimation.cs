using UnityEngine;

// Seed dùng tham số Horizontal làm trục đi-dạo(0) / đuổi(1).
// isMoving do EnemyController set theo tốc độ thực của agent (giống cách Player bật isMoving).
public class SeedAnimation : EnemyAnimation
{
    private float horizontalTarget;

    public void SetHorizontalTarget(float value) => horizontalTarget = value;

    protected override void UpdateLocomotionParameters(float moveAmount)
    {
        // Chỉ điều khiển Horizontal (mượt hoá như locomotion thường); isMoving lo ở controller.
        animator.SetFloat(horizontalHash, horizontalTarget, movementDampTime, Time.deltaTime);
    }
}
