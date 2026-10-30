using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : CharacterMovementBase
{
    private NavMeshAgent agent;
    private EnemyState state;

    // Enemy tự xoay bằng RotateSpeed (không để agent tự xoay theo angularSpeed).
    // true  = đang di chuyển thường -> mỗi frame xoay về hướng đi (RotateSpeed).
    // false = state tự lo việc xoay (vd kite/đứng chờ dùng FaceTowards).
    private bool autoRotateToMovement = true;

    public void Initialize(EnemyState state, NavMeshAgent agent)
    {
        this.state = state;
        this.agent = agent;

        // Agent lo DI CHUYỂN; phần XOAY do EnemyMovement tự xử lý theo RotateSpeed.
        agent.updatePosition = true;
        agent.updateRotation = false;
        autoRotateToMovement = true;
    }

    // Xoay enemy về hướng đang đi, dùng RotateSpeed (đồng nhất với FaceTowards).
    private void Update()
    {
        if (!autoRotateToMovement) return;
        if (agent == null || !agent.isOnNavMesh) return;

        Vector3 v = agent.velocity; v.y = 0f;
        if (v.sqrMagnitude < 0.0001f) return; // đứng yên -> giữ nguyên hướng

        float rotSpeed = state != null ? state.RotateSpeed : 12f;
        transform.rotation = Quaternion.Slerp(
            transform.rotation, Quaternion.LookRotation(v), rotSpeed * Time.deltaTime);
    }

    // Enemy điều hướng bằng NavMesh nên không dùng input vector như Player.
    public override void SetInput(Vector2 input) { }
    public override void Move() { }
    public override void Roll() { }

    #region Speed Multiplier
    private float baseSpeed;
    private float speedMultiplier = 1f;

    /// <summary>Status factor on every move speed, e.g. 0.6 while slowed; applies at once to the current path.</summary>
    public void SetSpeedMultiplier(float value)
    {
        if (Mathf.Approximately(value, speedMultiplier)) return;

        speedMultiplier = value;
        if (agent != null)
            agent.speed = baseSpeed * speedMultiplier;
    }

    private void ApplySpeed(float speed)
    {
        baseSpeed = speed;
        agent.speed = speed * speedMultiplier;
    }
    #endregion

    /// <summary>Drags the enemy by delta along the NavMesh, e.g. toward the centre of a vortex.</summary>
    public void Pull(Vector3 delta)
    {
        if (agent == null || !agent.isOnNavMesh) return;

        delta.y = 0f;
        agent.Move(delta);
    }

    #region Locomotion cơ bản
    public void Chase(Transform target, float speed, float stoppingDistance)
    {
        if (agent == null || !agent.isOnNavMesh || target == null) return;
        ApplySpeed(speed);
        agent.stoppingDistance = stoppingDistance;
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    public void MoveTo(Vector3 destination, float speed)
    {
        if (agent == null || !agent.isOnNavMesh) return;
        ApplySpeed(speed);
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
        autoRotateToMovement = false; // lúc đánh không tự xoay; state tự ngoảnh nếu cần
    }

    public void EndRootMotion()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        agent.Warp(transform.position);
        agent.updatePosition = true;
        autoRotateToMovement = true;  // trả lại tự xoay cho locomotion kế tiếp
    }
    #endregion

    public void ResetMovement()
    {
        CancelKnockback();
        knockbackActive = false;
        autoRotateToMovement = true;
        speedMultiplier = 1f;

        if (agent == null) return;
        agent.updatePosition = true;
        if (!agent.isOnNavMesh) return;
        agent.isStopped = false;
        agent.ResetPath();
    }

    #region Rotation thủ công
    // Bật/tắt việc tự xoay về hướng đi. Tắt khi state muốn tự ngoảnh (kite/đứng chờ).
    public void SetAutoRotation(bool enabled)
    {
        autoRotateToMovement = enabled;
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

        ApplySpeed(speed);
        agent.stoppingDistance = 0f;
        agent.isStopped = false;
        agent.SetDestination(dest);
    }
    #endregion

    #region Knockback
    private CancellationTokenSource knockbackCts;
    private bool knockbackActive;
    private bool autoRotateBeforeKnockback;

    /// <summary>Pushes the enemy along direction over duration with an ease-out; a new push restarts it but keeps the original rotation mode.</summary>
    public void Knockback(Vector3 direction, float distance, float duration)
    {
        if (agent == null) return;

        Vector3 dir = direction; dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = -transform.forward;
        dir.Normalize();

        if (!knockbackActive) autoRotateBeforeKnockback = autoRotateToMovement;

        CancelKnockback();
        knockbackCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        KnockbackAsync(dir, distance, Mathf.Max(0.01f, duration), knockbackCts.Token).Forget();
    }

    private async UniTaskVoid KnockbackAsync(Vector3 dir, float distance, float duration, CancellationToken token)
    {
        knockbackActive = true;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.updatePosition = true;
        }
        autoRotateToMovement = false;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float dt = Time.deltaTime;
            float remaining = 1f - (elapsed / duration);
            float step = distance * (dt / duration) * (remaining * 2f);

            if (agent.isOnNavMesh)
                agent.Move(dir * step);
            else
                transform.position += dir * step;

            elapsed += dt;

            bool canceled = await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow();
            if (canceled) return;
        }

        EndKnockback();
    }

    private void EndKnockback()
    {
        if (!knockbackActive) return;

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = false;
        autoRotateToMovement = autoRotateBeforeKnockback;
        knockbackActive = false;
    }

    private void CancelKnockback()
    {
        if (knockbackCts == null) return;

        knockbackCts.Cancel();
        knockbackCts.Dispose();
        knockbackCts = null;
    }

    private void OnDisable()
    {
        CancelKnockback();
        EndKnockback();
    }

    private void OnDestroy() => CancelKnockback();
    #endregion
}