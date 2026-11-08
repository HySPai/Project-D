using UnityEngine;

public class SeedController : EnemyController
{
    public SeedState SeedState => (SeedState)EnemyState;
    public SeedAnimation SeedAnim => (SeedAnimation)GetAnimation;

    protected override void BuildStates()
    {
        IdleState   = new SeedWanderState(this);
        ChaseState  = new SeedChargeState(this);
        ReturnState = new EnemyReturnState(this);
        AttackState = new SeedAttackState(this);
    }
}
