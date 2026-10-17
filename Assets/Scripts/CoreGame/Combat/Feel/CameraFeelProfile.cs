using UnityEngine;

[CreateAssetMenu(fileName = "CameraFeelProfile", menuName = "Game/Combat/Camera Feel Profile")]
public class CameraFeelProfile : ScriptableObject
{
    [Header("Free Look Screen Position")]
    [SerializeField, Range(0f, 0.5f)] private float maxOffsetX = 0.15f;
    [SerializeField, Range(0f, 0.5f)] private float maxOffsetY = 0.15f;
    [SerializeField, Range(0f, 20f)] private float freeLookSmoothSpeed = 5f;

    [Header("Lock On Screen Position")]
    [SerializeField, Range(0f, 20f)] private float lockOnSmoothSpeed = 5f;
    [SerializeField, Range(0f, 0.5f)] private float lockOnMaxOffsetX = 0.3f;
    [SerializeField, Range(0f, 0.5f)] private float lockOnMaxOffsetY = 0.3f;

    [Header("Lock On Lens Zoom")]
    [SerializeField, Range(0.5f, 20f)] private float defaultOrthographicSize = 4f;
    [SerializeField, Range(0.5f, 30f)] private float maxOrthographicSize = 8f;
    [SerializeField, Range(0f, 20f)] private float zoomSmoothSpeed = 3f;
    [SerializeField, Range(0f, 1.4f)] private float zoomThresholdViewport = 0.6f;

    [Header("Settle Threshold")]
    [SerializeField, Range(0.0001f, 0.1f)] private float zoomEpsilon = 0.01f;

    [Header("Hit Kick")]
    [SerializeField, Range(0f, 0.3f)] private float hitKickDistance = 0f;
    [SerializeField, Range(0.03f, 0.2f)] private float hitKickDuration = 0.08f;
    [SerializeField, Range(1f, 3f)] private float killKickMultiplier = 1.5f;

    public float MaxOffsetX => maxOffsetX;
    public float MaxOffsetY => maxOffsetY;
    public float FreeLookSmoothSpeed => freeLookSmoothSpeed;

    public float LockOnSmoothSpeed => lockOnSmoothSpeed;
    public float LockOnMaxOffsetX => lockOnMaxOffsetX;
    public float LockOnMaxOffsetY => lockOnMaxOffsetY;

    public float DefaultOrthographicSize => defaultOrthographicSize;
    public float MaxOrthographicSize => maxOrthographicSize;
    public float ZoomSmoothSpeed => zoomSmoothSpeed;
    public float ZoomThresholdViewport => zoomThresholdViewport;

    public float ZoomEpsilon => zoomEpsilon;

    public float HitKickDistance => hitKickDistance;
    public float HitKickDuration => hitKickDuration;
    public float KillKickMultiplier => killKickMultiplier;
}
