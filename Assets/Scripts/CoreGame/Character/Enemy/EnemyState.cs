using UnityEngine;

public class EnemyState : CharacterStateBase
{
    private SO_EnemyStats enemyStats;

    // Lazy: stats có sẵn lúc deserialize nên không phụ thuộc thứ tự Awake.
    private SO_EnemyStats EnemyStats =>
        enemyStats != null ? enemyStats : (enemyStats = stats as SO_EnemyStats);

    #region Home (vị trí gốc)
    [Header("Home")]
    [SerializeField] private Vector3 homePosition;
    [SerializeField] private Quaternion homeRotation;

    public Vector3 HomePosition => homePosition;
    public Quaternion HomeRotation => homeRotation;
    #endregion

    #region Target
    private Transform target;
    public Transform Target => target;
    public bool HasTarget => target != null;

    public void SetTarget(Transform value) => target = value;
    public void ClearTarget() => target = null;
    #endregion

    #region Vision / Chase config (đọc từ SO_EnemyStats)
    public float SightRange => EnemyStats.sightRange;
    public float FieldOfViewAngle => EnemyStats.fieldOfViewAngle;
    public float EyeHeight => EnemyStats.eyeHeight;
    public float ChaseSpeed => EnemyStats.chaseSpeed;
    public float StoppingDistance => EnemyStats.stoppingDistance;
    public float GiveUpDistance => EnemyStats.giveUpDistance;
    public float ReturnSpeed => EnemyStats.returnSpeed;
    public float HomeReachedThreshold => EnemyStats.homeReachedThreshold;
    public string AttackAnimationName => EnemyStats.attackAnimationName;
    public float AttackRange => EnemyStats.attackRange;
    public float AttackDamage => EnemyStats.attackDamage;
    public float AttackCooldown => EnemyStats.attackCooldown;
    #endregion

    #region Knockback & Stun (đẩy lùi + choáng khi trúng đòn)
    public float KnockbackDistance => EnemyStats.knockbackDistance;
    public float KnockbackDuration => EnemyStats.knockbackDuration;
    public float StunDuration => EnemyStats.stunDuration;

    private float stunEndTime;

    // Trong lúc choáng, FSM "đứng im" (không đuổi/đánh) để player dễ chém liên tục.
    // Mỗi lần trúng đòn lại gia hạn -> bị đánh dồn thì choáng kéo dài.
    public bool IsStunned => Time.time < stunEndTime;
    public void BeginStun(float duration) => stunEndTime = Time.time + duration;
    #endregion

    #region Animation
    // Hit reaction của enemy: 1 clip cố định, rootMotion = false.
    // Lý do: lực đẩy lùi do NavMeshAgent điều khiển, nếu để rootMotion = true thì
    // OnAnimatorMove cũng đẩy nhân vật -> 2 hệ thống đánh nhau.
    private static readonly AnimationAction HitReaction =
        new AnimationAction("TakeDamage", true, false, false, false);

    protected override AnimationAction? ResolveHitReaction(Vector3 damageSourcePosition) =>
        HitReaction;

    // Sau khi diễn anim trúng đòn -> kích hoạt đẩy lùi.
    protected override void PlayHitReaction(Vector3 damageSourcePosition)
    {
        base.PlayHitReaction(damageSourcePosition);
        (owner as EnemyController)?.OnKnockback(damageSourcePosition);
    }
    #endregion

    protected override void InitializeStats()
    {
        base.InitializeStats();
        if (EnemyStats == null)
            Debug.LogError($"{name}: stats phải là SO_EnemyStats", this);
    }

    public override void Initialize(CharacterControllerBase owner)
    {
        base.Initialize(owner);
        // Ghi nhớ điểm spawn để biết đường quay về.
        homePosition = transform.position;
        homeRotation = transform.rotation;
    }

    protected override void Die()
    {
        base.Die();

        var cc = owner != null ? owner.CharacterController : null;
        if (cc != null) cc.enabled = false;

        var enemyController = owner as EnemyController;
        if (enemyController != null && enemyController.GetLegsAnim != null)
            enemyController.GetLegsAnim.enabled = false;
    }
}