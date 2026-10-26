using UnityEngine;
using UnityEngine.AI;
using FIMSpace.FProceduralAnimation;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : CharacterControllerBase
{
    [SerializeField] private EnemyState state;
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyCombat combat;
    [SerializeField] private EnemyAnimation anim;
    [SerializeField] private EnemyVision vision;
    [SerializeField] private LegsAnimator legsAnim;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private EnemyDeathDissolve deathDissolve;

    public override CharacterStateBase GetState => state;
    public override CharacterMovementBase GetMovement => movement;
    public override CharacterCombatBase GetCombat => combat;
    public override CharacterAnimationBase GetAnimation => anim;

    public LegsAnimator GetLegsAnim => legsAnim;
    public NavMeshAgent Agent => agent;

    // Typed accessor cho các state FSM.
    public EnemyState EnemyState => state;
    public EnemyMovement EnemyMovement => movement;
    public EnemyVision Vision => vision;
    public EnemyCombat EnemyCombat => combat;

    // protected set: cho subclass (Spider) gắn state khác vào cùng "slot".
    public IEnemyState IdleState { get; protected set; }
    public IEnemyState ChaseState { get; protected set; }
    public IEnemyState ReturnState { get; protected set; }
    public IEnemyState AttackState { get; protected set; }
    private IEnemyState currentState;

    /// <summary>True while the enemy is alive and hunting or fighting rather than idle or heading home.</summary>
    public bool IsPursuing => !state.IsDead && currentState != null && currentState != IdleState && currentState != ReturnState;

    protected override void Awake()
    {
        base.Awake();
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        state.Initialize(this);
        movement.Initialize(state, agent);
        combat.Initialize(state, this);
        anim.Initialize(state);
        vision.Initialize(state.SightRange, state.FieldOfViewAngle, state.EyeHeight);

        BuildStates();
        ChangeState(IdleState);

        if (deathDissolve != null)
            deathDissolve.Prepare(state.DeathDissolve);
    }

    protected override void BeforeReset()
    {
        if (deathDissolve != null)
            deathDissolve.ResetVisual();

        currentState?.Exit();
        currentState = null;

        movement.ResetMovement();
    }

    protected override void TeleportTo(Vector3 position, Quaternion rotation)
    {
        characterController.enabled = false;
        if (agent.isOnNavMesh)
            agent.Warp(position);
        else
            transform.position = position;
        transform.rotation = rotation;
        characterController.enabled = true;
    }

    protected override void OnReset()
    {
        if (legsAnim != null)
            legsAnim.enabled = true;

        ChangeState(IdleState);
    }

    /// <summary>Starts the dissolve and disintegrate effect; called from EnemyState.Die.</summary>
    public void PlayDeathDissolve()
    {
        if (deathDissolve != null)
            deathDissolve.Play(state.DeathDissolve);
    }

    // Hành vi mặc định = SpiderKing: tiếp cận thẳng + tấn công.
    // Spider override để thay Chase -> Kite, Attack -> đánh tại chỗ giữ khoảng cách.
    protected virtual void BuildStates()
    {
        IdleState = new EnemyIdleState(this);
        ChaseState = new EnemyChaseState(this);
        ReturnState = new EnemyReturnState(this);
        AttackState = new EnemyAttackState(this);
    }

    private void Update()
    {
        if (state.IsDead) { movement.Stop(); return; }

        // Đang choáng -> tạm dừng FSM (không đuổi/đánh), coroutine knockback lo việc đẩy.
        if (!state.IsStunned)
            currentState?.Tick();

        // Animation chạy theo tốc độ thực của agent.
        float moveAmount = movement.NormalizedSpeed;
        anim.SetMoving(moveAmount > 0.01f);
        anim.UpdateAnimation(moveAmount);
    }

    public void ChangeState(IEnemyState newState)
    {
        if (newState == currentState) return;
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    // Gọi từ EnemyState.PlayHitReaction khi trúng đòn.
    public void OnKnockback(Vector3 sourcePosition, float knockbackScale, float stunScale)
    {
        if (state.IsDead) return;

        // Ngắt mọi đòn/hành động đang diễn (gọi Exit của state hiện tại) rồi về slot Chase.
        ChangeState(ChaseState);

        // Choáng: tạm dừng FSM trong stunDuration -> player dễ chém liên tục.
        state.BeginStun(state.StunDuration * Mathf.Max(0f, stunScale));

        // Đẩy lùi theo HƯỚNG NGƯỜI CHƠI đang nhìn (không dựa vào vị trí nguồn damage).
        Vector3 pushDir = ResolveKnockbackDirection(sourcePosition);
        movement.Knockback(pushDir, state.KnockbackDistance * Mathf.Max(0f, knockbackScale), state.KnockbackDuration);
    }

    public void OnParried(float staggerDuration)
    {
        if (state.IsDead) return;

        ChangeState(ChaseState);
        state.BeginStun(staggerDuration);
        movement.Knockback(ResolveKnockbackDirection(transform.position), state.KnockbackDistance * 0.5f, state.KnockbackDuration);
    }

    // Hướng đẩy = hướng player (Target) đang nhìn. Lúc player vung đòn là đang hướng
    // về phía địch, nên địch bị hất ngược ra sau đúng theo hướng người chơi.
    private Vector3 ResolveKnockbackDirection(Vector3 fallbackSource)
    {
        Transform player = state.Target;
        if (player != null)
        {
            Vector3 f = player.forward; f.y = 0f;
            if (f.sqrMagnitude > 0.0001f) return f.normalized;
        }

        // Chưa khoá được player (vd bị đánh lén lúc chưa phát hiện) -> tạm đẩy ra xa nguồn damage.
        Vector3 away = transform.position - fallbackSource; away.y = 0f;
        return away.sqrMagnitude > 0.0001f ? away.normalized : -transform.forward;
    }
}