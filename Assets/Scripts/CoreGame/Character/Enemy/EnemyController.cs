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

        float slow = slowEffect != null ? slowEffect.SpeedMultiplier : 1f;
        movement.SetSpeedMultiplier(slow);
        anim.SetPlaybackScale(slow);

        if (!state.IsStunned && !IsDisabledByVortex)
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

        if (state.IsHeld) return;

        // Đẩy lùi theo HƯỚNG NGƯỜI CHƠI đang nhìn (không dựa vào vị trí nguồn damage).
        Vector3 pushDir = ResolveKnockbackDirection(sourcePosition);
        movement.Knockback(pushDir, state.KnockbackDistance * Mathf.Max(0f, knockbackScale), state.KnockbackDuration);
    }

    private bool IsDisabledByVortex => state.IsHeld && !state.IsBoss;

    /// <summary>Poise reached 0: a long stun that also leaves the enemy open to a counter hit.</summary>
    public void OnPoiseBroken(float duration)
    {
        if (state.IsDead) return;

        ChangeState(ChaseState);
        state.InterruptAction();
        state.BeginStun(duration);
        state.MarkOffBalance(duration);
        Debug.Log($"[Thế] {name} vỡ thế, choáng {duration:0.0}s");
    }

    /// <summary>Elemental stun, e.g. from Stone Burst: the enemy cannot act for duration seconds.</summary>
    public void ApplyStun(float duration)
    {
        if (state.IsDead || duration <= 0f) return;

        ChangeState(ChaseState);
        state.InterruptAction();
        state.BeginStun(duration);
    }

    /// <summary>Pushes the enemy straight away from origin, e.g. the Water Ring; skipped while held by a vortex.</summary>
    public void PushAway(Vector3 origin, float distance)
    {
        if (state.IsDead || state.IsHeld || distance <= 0f) return;

        Vector3 away = transform.position - origin;
        away.y = 0f;
        if (away.sqrMagnitude < 0.0001f) away = -transform.forward;

        movement.Knockback(away.normalized, distance, state.KnockbackDuration);
    }

    /// <summary>Keeps the enemy inside a vortex for duration and drags it by pull; a non-boss stops attacking.</summary>
    public void HoldInVortex(float duration, Vector3 pull)
    {
        if (state.IsDead) return;

        bool wasDisabled = IsDisabledByVortex;
        state.Hold(duration);

        if (!wasDisabled && IsDisabledByVortex)
        {
            ChangeState(ChaseState);
            state.InterruptAction();
            movement.Stop();
        }

        movement.Pull(pull);
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