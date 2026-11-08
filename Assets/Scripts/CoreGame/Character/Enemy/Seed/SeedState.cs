using UnityEngine;
using UnityEngine.AI;

public class SeedState : EnemyState
{
    private SO_SeedStats seedStats;

    private SO_SeedStats SeedStats =>
        seedStats != null ? seedStats : (seedStats = stats as SO_SeedStats);

    public float WanderRadius => SeedStats.wanderRadius;
    public float WanderSpeed => SeedStats.wanderSpeed;
    public float WanderPauseMin => SeedStats.wanderPauseMin;
    public float WanderPauseMax => SeedStats.wanderPauseMax;
    public float WanderArriveThreshold => SeedStats.wanderArriveThreshold;
    public float ChargeArriveThreshold => SeedStats.chargeArriveThreshold;

    protected override void InitializeStats()
    {
        base.InitializeStats();
        if (SeedStats == null)
            Debug.LogError($"{name}: stats phải là SO_SeedStats", this);
    }

    public Vector3 GetRandomWanderPoint()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector2 r = Random.insideUnitCircle * WanderRadius;
            Vector3 candidate = HomePosition + new Vector3(r.x, 0f, r.y);
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                return hit.position;
        }
        return HomePosition;
    }
}
