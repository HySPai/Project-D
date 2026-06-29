using System.Collections;
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

    #region Locomotion cơ bản
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
    #endregion

    #region Root Motion (đòn đánh)
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
    #endregion

    #region Rotation thủ công (cho kite khi muốn luôn ngoảnh về player)
    public void SetAutoRotation(bool enabled)
    {
        if (agent != null) agent.updateRotation = enabled;
    }

    public void FaceTowards(Vector3 worldTarget, float rotSpeed)
    {
        Vector3 dir = worldTarget - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.Slerp(
            transform.rotation, Quaternion.LookRotation(dir), rotSpeed * Time.deltaTime);
    }
    #endregion

    #region Kite (vây quanh, giữ khoảng cách)
    // Giữ Spider ở vành bán kính 'preferredDistance' quanh target và lượn theo tiếp tuyến.
    // strafeDir = +1 / -1 quyết định chiều lượn (cùng/ngược chiều kim đồng hồ).
    public void KiteAround(Transform target, float preferredDistance, float speed,
        int strafeDir, float lookAhead)
    {
        if (agent == null || !agent.isOnNavMesh || target == null) return;

        Vector3 center = target.position;
        Vector3 toMe = transform.position - center; toMe.y = 0f;

        Vector3 radial = toMe.sqrMagnitude > 0.0001f ? toMe.normalized : -target.forward;
        Vector3 tangent = Vector3.Cross(Vector3.up, radial) * strafeDir;

        // Điểm đích = trên vành lý tưởng, đẩy thêm theo tiếp tuyến để tạo vòng lượn.
        // Nếu đang quá gần -> radial đẩy ra (ringPoint xa hơn) => tự lùi ra.
        Vector3 ringPoint = center + radial * preferredDistance;
        Vector3 dest = ringPoint + tangent * lookAhead;

        agent.speed = speed;
        agent.stoppingDistance = 0f;
        agent.isStopped = false;
        agent.SetDestination(dest);
    }
    #endregion

    #region Knockback (đẩy lùi khi trúng đòn)
    private Coroutine knockbackRoutine;

    public void Knockback(Vector3 sourcePosition, float distance, float duration)
    {
        if (agent == null) return;

        Vector3 dir = transform.position - sourcePosition; dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = -transform.forward; // không rõ nguồn -> lùi ra sau
        dir.Normalize();

        if (knockbackRoutine != null) StopCoroutine(knockbackRoutine);
        knockbackRoutine = StartCoroutine(KnockbackRoutine(dir, distance, Mathf.Max(0.01f, duration)));
    }

    private IEnumerator KnockbackRoutine(Vector3 dir, float distance, float duration)
    {
        bool priorAutoRot = agent != null && agent.updateRotation;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.updatePosition = true;
        }
        if (agent != null) agent.updateRotation = false;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float dt = Time.deltaTime;
            // Ease-out: mạnh lúc đầu, yếu dần. ∫(1-t)*2 dt = 1 -> tổng ~ distance.
            float remaining = 1f - (elapsed / duration);
            float step = distance * (dt / duration) * (remaining * 2f);

            if (agent != null && agent.isOnNavMesh)
                agent.Move(dir * step); // Move bám NavMesh -> không bị đẩy ra ngoài
            else
                transform.position += dir * step;

            elapsed += dt;
            yield return null;
        }

        if (agent != null)
        {
            if (agent.isOnNavMesh) agent.isStopped = false;
            agent.updateRotation = priorAutoRot; // trả lại trạng thái xoay trước đó
        }
        knockbackRoutine = null;
    }
    #endregion
}