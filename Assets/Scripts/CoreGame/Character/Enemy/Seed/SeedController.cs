using UnityEngine;

// Seed: đi dạo quanh chỗ spawn khi không có player; phát hiện player thì LAO THẲNG
// tới vị trí player -> đánh -> chờ hết cooldown -> lại lao tới vị trí player mới.
// Tái dùng EnemyController, chỉ thay nội dung các "slot" state qua BuildStates().
public class SeedController : EnemyController
{
    public SeedState SeedState => (SeedState)EnemyState;
    public SeedAnimation SeedAnim => (SeedAnimation)GetAnimation;

    protected override void BuildStates()
    {
        IdleState   = new SeedWanderState(this);   // "đứng/idle" -> đi dạo quanh spawn
        ChaseState  = new SeedChargeState(this);   // "đuổi" -> lao thẳng tới rồi đánh
        ReturnState = new EnemyReturnState(this);  // mất player -> về home rồi dạo tiếp
        AttackState = new SeedAttackState(this);   // đánh tại chỗ 1 nhịp rồi về charge
    }
}
