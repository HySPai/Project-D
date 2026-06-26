using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : CharacterMovementBase
{
    private NavMeshAgent agent;
    private EnemyState state;

    public void Initialize(EnemyState state, NavMeshAgent agent)
    {
        this.state = state;
        this.agent = agent;

        // Agent tự lo cả di chuyển lẫn xoay khi locomotion.
        agent.updatePosition = true;
        agent.updateRotation = true;
    }

    // Enemy điều hướng bằng NavMesh nên không dùng input vector như Player.
    public override void SetInput(Vector2 input) { }
    public override void Move() { }
    public override void Roll() { }

    public void Chase(Transform target, float speed, float stoppingDistance)
    {
        if (agent == null || !agent.isOnNavMesh || target == null) return;
        agent.speed = speed;
        agent.stoppingDistance = stoppingDistance;
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    public void MoveTo(Vector3 destination, float speed)
    {
        if (agent == null || !agent.isOnNavMesh) return;
        agent.speed = speed;
        agent.stoppingDistance = 0f;
        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    public void Stop()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        agent.isStopped = true;
        agent.ResetPath();
    }

    public bool HasReachedDestination(float threshold)
    {
        if (agent == null || !agent.isOnNavMesh || agent.pathPending) return false;
        return agent.remainingDistance <= Mathf.Max(threshold, agent.stoppingDistance);
    }

    // [0..1] để blend animation đi/đứng.
    public float NormalizedSpeed =>
        (agent == null || agent.speed <= 0.01f)
            ? 0f
            : Mathf.Clamp01(agent.velocity.magnitude / agent.speed);

    public override Vector3 GetMoveDirection()
    {
        if (agent == null) return Vector3.zero;
        Vector3 v = agent.velocity; v.y = 0f;
        return v.sqrMagnitude > 0.0001f ? v.normalized : Vector3.zero;
    }
    public void BeginRootMotion()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        agent.isStopped = true;
        agent.ResetPath();
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    public void EndRootMotion()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        agent.Warp(transform.position);
        agent.updatePosition = true;
        agent.updateRotation = true;
    }
}