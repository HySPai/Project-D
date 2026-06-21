using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : CharacterMovementBase
{
    [Header("NavMesh")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float stoppingDistance = 1.8f;
    [SerializeField] private bool useRunSpeed = false;

    private EnemyState state;

    public float CurrentNormalizedSpeed { get; private set; }

    public void Initialize(EnemyState state)
    {
        this.state = state;

        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.updateRotation = true;
            agent.stoppingDistance = stoppingDistance;
            agent.speed = useRunSpeed ? state.RunSpeed : state.MoveSpeed;
        }
    }

    // EnemyController gọi mỗi frame, truyền target hiện tại (null = không có).
    public void Tick(Transform target)
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            CurrentNormalizedSpeed = 0f;
            return;
        }

        bool canChase =
            target != null &&
            state != null &&
            !state.IsDead &&
            state.CanMove &&
            !state.IsAttacking;   // đang đánh thì dừng để không xung đột root motion

        if (canChase)
        {
            agent.speed = useRunSpeed ? state.RunSpeed : state.MoveSpeed;
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }
        else
        {
            StopAgent();
        }

        CurrentNormalizedSpeed = agent.speed > 0.01f
            ? Mathf.Clamp01(agent.velocity.magnitude / agent.speed)
            : 0f;
    }

    private void StopAgent()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        if (agent.hasPath) agent.ResetPath();
    }

    public override void SetInput(Vector2 input) { }
    public override void Move() { }
    public override void Roll() { }

    public override Vector3 GetMoveDirection()
    {
        if (agent == null || agent.velocity.sqrMagnitude < 0.0001f)
            return Vector3.zero;
        return agent.velocity.normalized;
    }
}