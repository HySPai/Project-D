using System;
using UnityEngine;

public readonly struct CameraZoneResult
{
    public readonly Vector3 Focus;
    public readonly float ZoomOffset;

    public CameraZoneResult(Vector3 focus, float zoomOffset)
    {
        Focus = focus;
        ZoomOffset = zoomOffset;
    }
}

public class CameraZoneDirector : MonoBehaviour
{
    [SerializeField] private CameraZone[] zones;

    private CameraZone boundsZone;
    private CameraZone framingZone;

    public CameraZone BoundsZone => boundsZone;
    public CameraZone FramingZone => framingZone;
    public float FramingWeight { get; private set; }

    public event Action<CameraZone> OnBoundsZoneChanged;
    public event Action<CameraZone> OnFramingZoneChanged;

    /// <summary>Picks the highest-priority zones containing the point and returns where the camera should aim and how far to zoom.</summary>
    public CameraZoneResult Resolve(Vector3 followPoint)
    {
        CameraZone nextBounds = null;
        CameraZone nextFraming = null;

        for (int i = 0; i < zones.Length; i++)
        {
            CameraZone zone = zones[i];
            if (zone == null || !zone.isActiveAndEnabled || !zone.Contains(followPoint)) continue;

            if (zone.Confines && (nextBounds == null || zone.Priority > nextBounds.Priority))
                nextBounds = zone;

            if (zone.ShapesFraming && (nextFraming == null || zone.Priority > nextFraming.Priority))
                nextFraming = zone;
        }

        if (nextBounds != boundsZone)
        {
            boundsZone = nextBounds;
            OnBoundsZoneChanged?.Invoke(boundsZone);
        }

        if (nextFraming != framingZone)
        {
            framingZone = nextFraming;
            OnFramingZoneChanged?.Invoke(framingZone);
        }

        Vector3 focus = boundsZone != null ? boundsZone.ClampFocus(followPoint) : followPoint;
        float zoom = 0f;
        FramingWeight = 0f;

        if (framingZone != null)
        {
            FramingWeight = framingZone.FramingWeight(followPoint);
            focus += framingZone.WorldFocusLead * FramingWeight;
            zoom = framingZone.ZoomOffset * FramingWeight;
        }

        return new CameraZoneResult(focus, zoom);
    }
}
