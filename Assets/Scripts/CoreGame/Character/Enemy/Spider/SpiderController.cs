using UnityEngine;

public class SpiderController : EnemyController
{
    public SpiderState SpiderState => (SpiderState)EnemyState;

    protected override void BuildStates()
    {
        IdleState = new EnemyIdleState(this);
        ChaseState = new SpiderKiteState(this);
        ReturnState = new EnemyReturnState(this);
        AttackState = new SpiderAttackState(this);
    }
}