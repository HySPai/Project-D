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

    public IEnemyState IdleState { get; private set; }
    public IEnemyState ChaseState { get; private set; }
    public IEnemyState ReturnState { get; private set; }
    private IEnemyState currentState;
    public IEnemyState AttackState { get; private set; }

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        state.Initialize(this);
        movement.Initialize(state, agent);
        combat.Initialize(state, this);
        anim.Initialize(state);
        vision.Initialize(state.SightRange, state.FieldOfViewAngle, state.EyeHeight);

        IdleState = new EnemyIdleState(this);
        ChaseState = new EnemyChaseState(this);
        ReturnState = new EnemyReturnState(this);
        AttackState = new EnemyAttackState(this);

        ChangeState(IdleState);
    }

    private void Update()
    {
        if (state.IsDead) { movement.Stop(); return; }

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
}