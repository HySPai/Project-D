using UnityEngine;

public class CameraZone : MonoBehaviour
{
    [SerializeField] private int priority;
    [SerializeField] private Vector3 areaCenter;
    [SerializeField] private Vector3 areaSize = new Vector3(20f, 6f, 20f);

    [Header("Bounds")]
    [SerializeField] private bool confine = true;
    [SerializeField] private Vector3 boundsCenter;
    [SerializeField] private Vector3 boundsSize = new Vector3(12f, 0f, 12f);
    [SerializeField] private float boundsSoftness = 3f;

    [Header("Framing")]
    [SerializeField] private bool shapeFraming;
    [SerializeField] private float zoomOffset;
    [SerializeField] private Vector3 focusLead;
    [SerializeField] private bool useRamp;
    [SerializeField] private Vector3 rampStart;
    [SerializeField] private Vector3 rampEnd;

    public int Priority => priority;
    public bool Confines => confine;
    public bool ShapesFraming => shapeFraming;

    /// <summary>True when the point is inside the area that activates this zone.</summary>
    public bool Contains(Vector3 worldPoint) =>
        CameraZoneMath.Contains(transform.worldToLocalMatrix, areaCenter, areaSize, worldPoint);

    /// <summary>Eases the camera focus to a stop inside this zone's bounds on the ground plane.</summary>
    public Vector3 ClampFocus(Vector3 worldPoint) =>
        CameraZoneMath.SoftClampXZ(
            transform.worldToLocalMatrix, transform.localToWorldMatrix, boundsCenter, boundsSize, boundsSoftness, worldPoint);

    /// <summary>How strongly this zone's framing applies at a point: 1 everywhere, or 0 to 1 along the ramp.</summary>
    public float FramingWeight(Vector3 worldPoint)
    {
        if (!useRamp) return 1f;
        return CameraZoneMath.RampWeight(transform.InverseTransformPoint(worldPoint), rampStart, rampEnd);
    }

    /// <summary>Extra orthographic size at full weight.</summary>
    public float ZoomOffset => zoomOffset;

    /// <summary>World-space shift of the camera focus at full weight, e.g. toward a view.</summary>
    public Vector3 WorldFocusLead => transform.TransformVector(focusLead);

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = shapeFraming ? new Color(1f, 0.6f, 0.1f, 0.6f) : new Color(0.2f, 0.8f, 1f, 0.4f);
        Gizmos.DrawWireCube(areaCenter, areaSize);

        if (shapeFraming && useRamp)
        {
            Gizmos.DrawLine(rampStart, rampEnd);
            Gizmos.DrawWireSphere(rampEnd, 0.3f);
        }

        if (!confine) return;
        Vector3 flat = new Vector3(boundsSize.x, 0.05f, boundsSize.z);
        Gizmos.color = new Color(1f, 0.2f, 0.6f, 0.9f);
        Gizmos.DrawWireCube(boundsCenter, flat);

        float soft = Mathf.Max(0f, boundsSoftness);
        Gizmos.color = new Color(1f, 0.2f, 0.6f, 0.3f);
        Gizmos.DrawWireCube(boundsCenter, new Vector3(Mathf.Max(0f, flat.x - soft * 2f), 0.05f, Mathf.Max(0f, flat.z - soft * 2f)));
    }
}
