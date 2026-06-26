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

    #region Animation
    protected override AnimationAction? ResolveHitReaction(Vector3 damageSourcePosition) =>
        CharacterAnimations.TakeDamage;
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