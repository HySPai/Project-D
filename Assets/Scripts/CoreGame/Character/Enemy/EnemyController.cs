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

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        state.Initialize(this);
        movement.Initialize(state, agent);
        combat.Initialize(state, this);
        anim.Initialize(state);
        vision.Initialize(state.SightRange, state.FieldOfViewAngle, state.EyeHeight);

        BuildStates();
        ChangeState(IdleState);
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

        // Đang bị đẩy lùi -> tạm dừng FSM, để coroutine knockback toàn quyền điều khiển.
        if (!state.IsKnockedBack)
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
    public void OnKnockback(Vector3 sourcePosition)
    {
        if (state.IsDead) return;

        // Thoát sạch leap/attack (gọi Exit) rồi về "slot Chase" để hồi lại tự nhiên.
        ChangeState(ChaseState);
        state.BeginKnockback(state.KnockbackDuration);
        movement.Knockback(sourcePosition, state.KnockbackDistance, state.KnockbackDuration);
    }
}