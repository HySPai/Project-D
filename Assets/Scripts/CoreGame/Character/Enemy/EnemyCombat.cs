using UnityEngine;

public class EnemyCombat : CharacterCombatBase
{
    [Header("Enemy Detection")]
    [SerializeField] private float detectionRange = 12f;   // tầm phát hiện để LOCK
    [SerializeField] private float loseTargetRange = 16f;  // vượt quá thì UNLOCK (hysteresis tránh nhấp nháy)
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private float eyeHeight = 1.5f;

    private readonly Collider[] _candidates = new Collider[16];

    public bool HasTarget => lockOnTransform != null;

    public void Initialize(EnemyState state)
    {
        base.Initialize(state);
    }

    // Enemy KHÔNG dùng auto-unlock theo viewport camera của base (cái đó dành cho player).
    // Nó tự nhận diện mục tiêu theo tầm + đường ngắm của chính nó.
    protected override void Update()
    {
        if (state != null && state.IsDead)
        {
            if (lockOnTransform != null) ClearLockTarget();
            return;
        }

        if (lockOnTransform == null)
            TryAcquireTarget();
        else
            MaintainTarget();
    }

    private void TryAcquireTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, detectionRange, _candidates, lockOnLayer);

        Transform best = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Transform candidate = _candidates[i].transform;
            var targetState = candidate.GetComponentInParent<CharacterStateBase>();

            // bỏ qua null, chính mình, và target đã chết
            if (targetState == null || targetState == state || targetState.IsDead)
                continue;

            if (requireLineOfSight && !HasLineOfSight(candidate))
                continue;

            float sqr = (candidate.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = candidate;
            }
        }

        if (best != null)
            SetLockTarget(best);
    }

    private void MaintainTarget()
    {
        var targetState = lockOnTransform.GetComponentInParent<CharacterStateBase>();

        if (targetState == null || targetState.IsDead)
        {
            ClearLockTarget();
            return;
        }

        if (Vector3.Distance(transform.position, lockOnTransform.position) > loseTargetRange)
            ClearLockTarget();
    }

    private bool HasLineOfSight(Transform target)
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPoint = target.position + Vector3.up * eyeHeight;
        Vector3 dir = targetPoint - origin;

        // có obstacle chắn giữa enemy và target -> coi như không thấy
        return !Physics.Raycast(origin, dir.normalized, dir.magnitude, state.ObstacleLayer);
    }

    public override void Attack()
    {
    }
}