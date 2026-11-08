using UnityEngine;

[CreateAssetMenu(fileName = "SO_SeedStats", menuName = "Game/Stats/Seed Stats")]
public class SO_SeedStats : SO_EnemyStats
{
    [Header("Wander (Đi dạo quanh chỗ spawn)")]
    /// <summary>bán kính tối đa quanh home được phép đi tới</summary>
    public float wanderRadius = 6f;
    /// <summary>tốc độ đi dạo (đi bộ -&gt; Horizontal 0)</summary>
    public float wanderSpeed = 2f;
    /// <summary>dừng nghỉ tối thiểu giữa 2 lần đi</summary>
    public float wanderPauseMin = 1f;
    /// <summary>dừng nghỉ tối đa</summary>
    public float wanderPauseMax = 3f;
    /// <summary>coi như tới điểm dạo</summary>
    public float wanderArriveThreshold = 0.4f;

    [Header("Charge (Lao thẳng tới vị trí player)")]
    /// <summary>coi như tới điểm đã chốt</summary>
    public float chargeArriveThreshold = 0.5f;
}
