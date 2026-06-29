using UnityEngine;

// Spider: né lại gần player (kite), giữ khoảng cách + lượn vòng,
// khi đủ điều kiện thì đánh tại chỗ (giữ nguyên khoảng cách, không lao vào).
public class SpiderController : EnemyController
{
    // EnemyState thực tế là SpiderState (đã gán component SpiderState trên prefab).
    public SpiderState SpiderState => (SpiderState)EnemyState;

    protected override void BuildStates()
    {
        IdleState = new EnemyIdleState(this);
        ChaseState = new SpiderKiteState(this);        // "đuổi" = giữ khoảng cách + lượn
        ReturnState = new EnemyReturnState(this);
        AttackState = new SpiderAttackState(this);       // "tấn công" = đánh tại chỗ, giữ khoảng cách
    }
}