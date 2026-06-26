using UnityEngine;

// Tách riêng phần "nhìn" khỏi AI: chỉ trả lời "thấy ai không", không quyết định hành vi.
public class EnemyVision : MonoBehaviour
{
    [Header("Eye (gốc raycast)")]
    [SerializeField] private Transform eye;            // null -> dùng transform + eyeHeight

    [Header("Layers")]
    [SerializeField] private LayerMask targetLayer;    // layer của Player
    [SerializeField] private LayerMask obstacleLayer;  // vật cản chắn tầm nhìn

    private float sightRange;
    private float fovAngle;
    private float eyeHeight;

    private readonly Collider[] buffer = new Collider[8];

    public void Initialize(float sightRange, float fovAngle, float eyeHeight)
    {
        this.sightRange = sightRange;
        this.fovAngle = fovAngle;
        this.eyeHeight = eyeHeight;
    }

    private Vector3 EyePosition =>
        eye != null ? eye.position : transform.position + Vector3.up * eyeHeight;

    // Quét tìm target gần nhất đang nằm trong tầm nhìn. null nếu không thấy.
    public Transform DetectTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, sightRange, buffer, targetLayer,
            QueryTriggerInteraction.Ignore);

        Transform nearest = null;
        float nearestSqr = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Transform candidate = buffer[i].transform;
            if (!CanSee(candidate)) continue;

            float sqr = (candidate.position - transform.position).sqrMagnitude;
            if (sqr < nearestSqr)
            {
                nearestSqr = sqr;
                nearest = candidate;
            }
        }
        return nearest;
    }

    public bool CanSee(Transform target)
    {
        if (target == null) return false;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        // 1) Trong tầm xa?
        if (toTarget.sqrMagnitude > sightRange * sightRange) return false;

        // 2) Trong góc nhìn?
        if (Vector3.Angle(transform.forward, toTarget) > fovAngle * 0.5f) return false;

        // 3) Không bị vật cản che?
        Vector3 origin = EyePosition;
        Vector3 dir = (target.position + Vector3.up * 0.9f) - origin; // ngắm vào thân
        if (Physics.Raycast(origin, dir.normalized, dir.magnitude, obstacleLayer,
                QueryTriggerInteraction.Ignore))
            return false;

        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (sightRange <= 0f) return; // chỉ hiện khi đang Play (đã Initialize)
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Vector3 l = Quaternion.Euler(0, -fovAngle * 0.5f, 0) * transform.forward;
        Vector3 r = Quaternion.Euler(0, fovAngle * 0.5f, 0) * transform.forward;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, l * sightRange);
        Gizmos.DrawRay(transform.position, r * sightRange);
    }
#endif
}