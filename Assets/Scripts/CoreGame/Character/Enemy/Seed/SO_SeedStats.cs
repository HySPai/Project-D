using UnityEngine;

// Stats riêng cho Seed: kế thừa toàn bộ stats enemy (vision/attack/knockback/stun...)
// và thêm tham số cho hành vi "đi dạo quanh chỗ spawn" + "lao thẳng tới đánh".
[CreateAssetMenu(fileName = "SO_SeedStats", menuName = "Game/Stats/Seed Stats")]
public class SO_SeedStats : SO_EnemyStats
{
    [Header("Wander (Đi dạo quanh chỗ spawn)")]
    public float wanderRadius = 6f;          // bán kính tối đa quanh home được phép đi tới
    public float wanderSpeed = 2f;           // tốc độ đi dạo (đi bộ -> Horizontal 0)
    public float wanderPauseMin = 1f;        // dừng nghỉ tối thiểu giữa 2 lần đi
    public float wanderPauseMax = 3f;        // dừng nghỉ tối đa
    public float wanderArriveThreshold = 0.4f; // coi như tới điểm dạo

    [Header("Charge (Lao thẳng tới vị trí player)")]
    public float chargeArriveThreshold = 0.5f; // coi như tới điểm đã chốt
    // Lưu ý: tốc độ lao dùng 'chaseSpeed' (kế thừa từ SO_EnemyStats).
}
