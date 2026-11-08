using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NavMesh locomotion for enemies. The agent moves the body while this component turns it at RotateSpeed, so every
/// enemy turns the same way. Moving targets are re-pathed on an interval instead of every frame.
/// </summary>
public class EnemyMovement : CharacterMovementBase
{
    private const float DefaultRotateSpeed = 12f;
    private const float MinSqrVelocity = 0.0001f;

    [Header("Pathing")]
    [Min(0f)]
    [SerializeField] private float repathInterval = 0.2f;
    [Min(0f)]
    [SerializeField] private float repathMinDistance = 0.3f;

    private NavMeshAgent agent;
    private EnemyState state;
    private bool autoRotateToMovement = true;

    private float baseSpeed = -1f;
    private float speedMultiplier = 1f;
    private Vector3 lastDestination;
    private float nextRepathTime;

    /// <summary>Hooks the agent up; position is driven by the agent and rotation by this component.</summary>
    public void Initialize(EnemyState state, NavMeshAgent agent)
    {
        this.state = state;
        this.agent = agent;

        agent.updatePosition = true;
        agent.updateRotation = false;
        autoRotateToMovement = true;
    }

    private bool HasAgent => agent != null && agent.isOnNavMesh;

    private void Update()
    {
        if (!autoRotateToMovement || !HasAgent) return;

        Vector3 v = agent.velocity;
        v.y = 0f;
        if (v.sqrMagnitude < MinSqrVelocity) return;

        float rotSpeed = state != null ? state.RotateSpeed : DefaultRotateSpeed;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(v), rotSpeed * Time.deltaTime);
    }

    public override void SetInput(Vector2 input) { }
    public override void Move() { }
    public override void Roll() { }

    #region Speed
    /// <summary>Status factor on every move speed, e.g. 0.6 while slowed; applies at once to the current path.</summary>
    public void SetSpeedMultiplier(float value)
    {
        if (Mathf.Approximately(value, speedMultiplier)) return;

        speedMultiplier = value;
        if (agent != null && baseSpeed >= 0f)
            agent.speed = baseSpeed * speedMultiplier;
    }

    private void ApplySpeed(float speed)
    {
        if (Mathf.Approximately(speed, baseSpeed)) return;

        baseSpeed = speed;
        agent.speed = speed * speedMultiplier;
    }
    #endregion

    #region Locomotion
    /// <summary>Drags the enemy by delta along the NavMesh, e.g. toward the centre of a vortex.</summary>
    public void Pull(Vector3 delta)
    {
        if (!HasAgent) return;

        delta.y = 0f;
        agent.Move(delta);
    }

    /// <summary>Follows a moving target, re-pathing only when it has moved far enough and the interval has passed.</summary>
    public void Chase(Transform target, float speed, float stoppingDistance)
    {
        if (target == null) return;
        Follow(target.position, speed, stoppingDistance);
    }

    /// <summary>Walks to a fixed point right away.</summary>
    public void MoveTo(Vector3 destination, float speed)
    {
        if (!HasAgent) return;

        PrepareMove(speed, 0f);
        SetDestinationNow(destination);
        nextRepathTime = Time.time;
    }

    /// <summary>Stops and clears the current path.</summary>
    public void Stop()
    {
        if (!HasAgent) return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    /// <summary>True once the agent is within threshold (or its stopping distance) of the destination.</summary>
    public bool HasReachedDestination(float threshold)
    {
        if (!HasAgent || agent.pathPending) return false;
        return agent.remainingDistance <= Mathf.Max(threshold, agent.stoppingDistance);
    }

    /// <summary>Actual speed over the current max speed, 0..1, for blending walk and idle.</summary>
    public float NormalizedSpeed => agent == null || agent.speed <= 0.01f ? 0f : Mathf.Clamp01(agent.velocity.magnitude / agent.speed);

    public override Vector3 GetMoveDirection()
    {
        if (agent == null) return Vector3.zero;

        Vector3 v = agent.velocity;
        v.y = 0f;
        return v.sqrMagnitude > MinSqrVelocity ? v.normalized : Vector3.zero;
    }

    private void Follow(Vector3 destination, float speed, float stoppingDistance)
    {
        if (!HasAgent) return;

        PrepareMove(speed, stoppingDistance);

        bool hasRoute = agent.hasPath || agent.pathPending;
        if (hasRoute && Time.time < nextRepathTime) return;
        if (hasRoute && (destination - lastDestination).sqrMagnitude < repathMinDistance * repathMinDistance) return;

        SetDestinationNow(destination);
    }

    private void PrepareMove(float speed, float stoppingDistance)
    {
        ApplySpeed(speed);
        if (!Mathf.Approximately(agent.stoppingDistance, stoppingDistance)) agent.stoppingDistance = stoppingDistance;
        if (agent.isStopped) agent.isStopped = false;
    }

    private void SetDestinationNow(Vector3 destination)
    {
        agent.SetDestination(destination);
        lastDestination = destination;
        nextRepathTime = Time.time + repathInterval;
    }
    #endregion

    #region Root Motion
    /// <summary>Hands the body to the attack animation: the agent stops and no longer moves the transform.</summary>
    public void BeginRootMotion()
    {
        if (!HasAgent) return;

        agent.isStopped = true;
        agent.ResetPath();
        agent.updatePosition = false;
        autoRotateToMovement = false;
    }

    /// <summary>Takes the body back from the animation and snaps the agent to where it ended up.</summary>
    public void EndRootMotion()
    {
        if (!HasAgent) return;

        agent.Warp(transform.position);
        agent.updatePosition = true;
        autoRotateToMovement = true;
    }
    #endregion

    /// <summary>Clears knockback, slow and path state for a respawn.</summary>
    public void ResetMovement()
    {
        CancelKnockback();
        knockbackActive = false;
        autoRotateToMovement = true;
        speedMultiplier = 1f;
        baseSpeed = -1f;

        if (agent == null) return;
        agent.updatePosition = true;
        if (!agent.isOnNavMesh) return;
        agent.isStopped = false;
        agent.ResetPath();
    }

    #region Manual Rotation
    /// <summary>Turns facing the movement direction on or off; states that aim on their own turn it off.</summary>
    public void SetAutoRotation(bool enabled) => autoRotateToMovement = enabled;

    /// <summary>Turns toward worldTarget on the ground plane at rotSpeed.</summary>
    public void FaceTowards(Vector3 worldTarget, float rotSpeed)
    {
        Vector3 dir = worldTarget - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < MinSqrVelocity) return;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotSpeed * Time.deltaTime);
    }
    #endregion

    #region Kite
    /// <summary>
    /// Keeps the enemy on a ring of preferredDistance around target and slides along it; strafeDir +1 or -1 picks the
    /// direction. Standing too close pushes the ring point outward so the enemy backs off on its own.
    /// </summary>
    public void KiteAround(Transform target, float preferredDistance, float speed, int strafeDir, float lookAhead)
    {
        if (target == null) return;

        Vector3 center = target.position;
        Vector3 toMe = transform.position - center;
        toMe.y = 0f;

        Vector3 radial = toMe.sqrMagnitude > MinSqrVelocity ? toMe.normalized : -target.forward;
        Vector3 tangent = Vector3.Cross(Vector3.up, radial) * strafeDir;
        Follow(center + radial * preferredDistance + tangent * lookAhead, speed, 0f);
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

        Vector3 dir = direction;
        dir.y = 0f;
        if (dir.sqrMagnitude < MinSqrVelocity) dir = -transform.forward;
        dir.Normalize();

        if (!knockbackActive) autoRotateBeforeKnockback = autoRotateToMovement;

        CancelKnockback();
        knockbackCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
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
            float remaining = 1f - elapsed / duration;
            float step = distance * (dt / duration) * (remaining * 2f);

            if (agent.isOnNavMesh)
                agent.Move(dir * step);
            else
                transform.position += dir * step;

            elapsed += dt;

            if (await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow()) return;
        }

        EndKnockback();
    }

    private void EndKnockback()
    {
        if (!knockbackActive) return;

        if (HasAgent) agent.isStopped = false;
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
