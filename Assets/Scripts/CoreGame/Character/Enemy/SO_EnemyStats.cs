using UnityEngine;

[CreateAssetMenu(fileName = "SO_EnemyStats", menuName = "Game/Stats/Enemy Stats")]
public class SO_EnemyStats : CharacterStats
{
    [Header("Vision (Tầm nhìn)")]
    /// <summary>bán kính phát hiện</summary>
    public float sightRange = 12f;
    /// <summary>góc nhìn (độ, tổng 2 bên)</summary>
    public float fieldOfViewAngle = 110f;
    /// <summary>cao độ "mắt" để raycast line-of-sight</summary>
    public float eyeHeight = 1.6f;

    [Header("Chase (Đuổi theo)")]
    /// <summary>tốc độ khi đuổi</summary>
    public float chaseSpeed = 4.5f;
    /// <summary>dừng cách target</summary>
    public float stoppingDistance = 1.8f;
    /// <summary>player vượt xa hơn -&gt; bỏ cuộc</summary>
    public float giveUpDistance = 18f;

    [Header("Return (Quay về)")]
    /// <summary>tốc độ khi quay về</summary>
    public float returnSpeed = 3f;
    public float homeReachedThreshold = 0.4f;

    [Header("Attack (Tấn công)")]
    public string attackAnimationName = "Attack_01";
    public float attackRange = 2f;
    public float attackDamage = 1f;
    public bool attackUnblockable = false;
    /// <summary>giãn cách giữa 2 đòn</summary>
    public float attackCooldown = 1.5f;

    [Header("Knockback (Bị đẩy lùi khi trúng đòn)")]
    /// <summary>quãng đường bị đẩy lùi</summary>
    public float knockbackDistance = 1.5f;
    /// <summary>thời gian đẩy lùi (nên ~ độ dài clip hit)</summary>
    public float knockbackDuration = 0.25f;
    /// <summary>thời gian choáng (FSM đứng im) - nên &gt;= knockbackDuration</summary>
    public float stunDuration = 0.5f;

    [Header("Poise")]
    public float poiseMax = 0f;
    public float poiseRecoverDelay = 2f;
    public float poiseRecoverRate = 30f;
    public float poiseBreakStun = 1.5f;

    [Header("Boss")]
    public bool isBoss = false;

    [Header("Feel")]
    public EnemyCombatFeelProfile combatFeel;
    public HitBloodProfile hitBlood;
    public DeathDissolveProfile deathDissolve;
}