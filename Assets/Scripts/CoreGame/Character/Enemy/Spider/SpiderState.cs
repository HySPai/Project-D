using UnityEngine;

// State của Spider: kế thừa toàn bộ EnemyState (target/vision/knockback...)
// chỉ thêm các tham số phục vụ kite.
public class SpiderState : EnemyState
{
    private SO_SpiderStats spiderStats;

    private SO_SpiderStats SpiderStats =>
        spiderStats != null ? spiderStats : (spiderStats = stats as SO_SpiderStats);

    public float PreferredDistance => SpiderStats.preferredDistance;
    public float KiteSpeed => SpiderStats.kiteSpeed;
    public float StrafeLookAhead => SpiderStats.strafeLookAhead;
    public float StrafeFlipMin => SpiderStats.strafeFlipMin;
    public float StrafeFlipMax => SpiderStats.strafeFlipMax;
    public float TurnSpeed => SpiderStats.turnSpeed;

    protected override void InitializeStats()
    {
        base.InitializeStats();
        if (SpiderStats == null)
            Debug.LogError($"{name}: stats phải là SO_SpiderStats", this);
    }
}