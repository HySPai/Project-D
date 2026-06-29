using UnityEngine;

[CreateAssetMenu(fileName = "SO_EnemyStats", menuName = "Game/Stats/Enemy Stats")]
public class SO_EnemyStats : CharacterStats
{
    [Header("Vision (Tầm nhìn)")]
    public float sightRange = 12f;          // bán kính phát hiện
    public float fieldOfViewAngle = 110f;   // góc nhìn (độ, tổng 2 bên)
    public float eyeHeight = 1.6f;          // cao độ "mắt" để raycast line-of-sight

    [Header("Chase (Đuổi theo)")]
    public float chaseSpeed = 4.5f;         // tốc độ khi đuổi
    public float stoppingDistance = 1.8f;   // dừng cách target
    public float giveUpDistance = 18f;      // player vượt xa hơn -> bỏ cuộc

    [Header("Return (Quay về)")]
    public float returnSpeed = 3f;          // tốc độ khi quay về
    public float homeReachedThreshold = 0.4f;

    [Header("Attack (Tấn công)")]
    public string attackAnimationName = "Attack_01";
    public float attackRange = 2f;
    public float attackDamage = 1f;
    public float attackCooldown = 1.5f;   // giãn cách giữa 2 đòn

    [Header("Knockback (Bị đẩy lùi khi trúng đòn)")]
    public float knockbackDistance = 1.5f;  // quãng đường bị đẩy lùi
    public float knockbackDuration = 0.25f; // thời gian đẩy lùi (nên ~ độ dài clip hit)
}