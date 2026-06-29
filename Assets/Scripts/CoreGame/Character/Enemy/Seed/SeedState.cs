using UnityEngine;
using UnityEngine.AI;

// State của Seed: kế thừa EnemyState (target/vision/home/knockback/stun)
// + tham số đi dạo và hàm chọn điểm dạo ngẫu nhiên quanh home.
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

    // Chọn 1 điểm ngẫu nhiên trong bán kính WanderRadius quanh home, ép về NavMesh.
    public Vector3 GetRandomWanderPoint()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector2 r = Random.insideUnitCircle * WanderRadius;
            Vector3 candidate = HomePosition + new Vector3(r.x, 0f, r.y);
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                return hit.position;
        }
        return HomePosition; // không tìm được điểm hợp lệ -> đứng yên tại home
    }
}
