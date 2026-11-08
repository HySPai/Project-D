using UnityEngine;

/// <summary>
/// Answers "which target can this enemy see" without deciding behaviour. Scans on a fixed interval with a random
/// phase per enemy so a crowd never queries physics on the same frame.
/// </summary>
public class EnemyVision : MonoBehaviour
{
    private const float TargetAimHeight = 0.9f;

    [Header("Eye")]
    [SerializeField] private Transform eye;

    [Header("Layers")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Scan")]
    [Min(0f)]
    [SerializeField] private float scanInterval = 0.15f;

    private readonly Collider[] buffer = new Collider[8];

    private float sightRange;
    private float sightRangeSqr;
    private float halfFovCos;
    private float eyeHeight;
    private float nextScanTime;

    /// <summary>Sets the sight cone from the enemy stats and staggers the first scan.</summary>
    public void Initialize(float sightRange, float fovAngle, float eyeHeight)
    {
        this.sightRange = sightRange;
        sightRangeSqr = sightRange * sightRange;
        halfFovCos = Mathf.Cos(fovAngle * 0.5f * Mathf.Deg2Rad);
        this.eyeHeight = eyeHeight;
        nextScanTime = Time.time + Random.Range(0f, scanInterval);
    }

    private Vector3 EyePosition => eye != null ? eye.position : transform.position + Vector3.up * eyeHeight;

    /// <summary>Nearest visible target, or null when none is seen or the scan is not due yet this frame.</summary>
    public Transform DetectTarget()
    {
        if (Time.time < nextScanTime) return null;
        nextScanTime = Time.time + scanInterval;

        int count = Physics.OverlapSphereNonAlloc(transform.position, sightRange, buffer, targetLayer, QueryTriggerInteraction.Ignore);

        Transform nearest = null;
        float nearestSqr = float.MaxValue;
        Vector3 position = transform.position;

        for (int i = 0; i < count; i++)
        {
            Transform candidate = buffer[i].transform;
            if (!CanSee(candidate)) continue;

            float sqr = (candidate.position - position).sqrMagnitude;
            if (sqr >= nearestSqr) continue;

            nearestSqr = sqr;
            nearest = candidate;
        }
        return nearest;
    }

    /// <summary>True when target is in range, inside the field of view and not hidden behind an obstacle.</summary>
    public bool CanSee(Transform target)
    {
        if (target == null) return false;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        float sqr = toTarget.sqrMagnitude;
        if (sqr > sightRangeSqr) return false;
        if (sqr > 0.0001f && Vector3.Dot(transform.forward, toTarget / Mathf.Sqrt(sqr)) < halfFovCos) return false;

        Vector3 origin = EyePosition;
        Vector3 ray = target.position + Vector3.up * TargetAimHeight - origin;
        float length = ray.magnitude;
        return length <= 0.0001f || !Physics.Raycast(origin, ray / length, length, obstacleLayer, QueryTriggerInteraction.Ignore);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (sightRange <= 0f) return;

        float fovAngle = Mathf.Acos(halfFovCos) * Mathf.Rad2Deg * 2f;
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Vector3 l = Quaternion.Euler(0f, -fovAngle * 0.5f, 0f) * transform.forward;
        Vector3 r = Quaternion.Euler(0f, fovAngle * 0.5f, 0f) * transform.forward;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, l * sightRange);
        Gizmos.DrawRay(transform.position, r * sightRange);
    }
#endif
}
