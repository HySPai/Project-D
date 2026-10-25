using Unity.Cinemachine;
using UnityEngine;

public class CameraZoneExtension : CinemachineExtension
{
    [SerializeField] private CameraZoneDirector director;
    [SerializeField] private float followSharpness = 5f;
    [SerializeField] private float zoomSharpness = 1.5f;

    private Vector3 smoothedFocus;
    private float smoothedZoom;
    private bool snapNext = true;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body) return;

        Transform follow = vcam.Follow;
        if (follow == null || director == null) return;

        CameraZoneResult result = director.Resolve(follow.position);

        if (snapNext || deltaTime < 0f)
        {
            smoothedFocus = result.Focus;
            smoothedZoom = result.ZoomOffset;
            snapNext = false;
        }
        else
        {
            smoothedFocus = CameraZoneMath.Damp(smoothedFocus, result.Focus, followSharpness, deltaTime);
            smoothedZoom = CameraZoneMath.Damp(smoothedZoom, result.ZoomOffset, zoomSharpness, deltaTime);
        }

        state.PositionCorrection += smoothedFocus - follow.position;

        LensSettings lens = state.Lens;
        lens.OrthographicSize = Mathf.Max(0.1f, lens.OrthographicSize + smoothedZoom);
        state.Lens = lens;
    }

    public override void OnTargetObjectWarped(CinemachineVirtualCameraBase vcam, Transform target, Vector3 positionDelta)
    {
        base.OnTargetObjectWarped(vcam, target, positionDelta);
        snapNext = true;
    }
}
