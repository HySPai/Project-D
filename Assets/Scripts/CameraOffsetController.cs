using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>Shifts the follow camera's screen framing with the camera stick or the lock-on target, zooms out to keep a locked target in view and kicks the camera on hits.</summary>
public class CameraOffsetController : MonoBehaviour
{
    private const float ViewportToScreenOffset = 0.5f;
    private const float MaxViewportDistance = 1.41421356f;
    private const float MinSqrDirection = 0.0001f;
    private const float HalfCycle = 0.5f;

    [SerializeField] private Camera outputCamera;
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private CinemachinePositionComposer composer;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CinemachineCameraOffset kickOffset;
    [SerializeField] private CameraFeelProfile profile;

    private PlayerState _state;
    private CharacterCombatBase _combat;

    private Vector2 _currentScreenPosition;
    private Camera _cam;

    private bool _zoomSettled;
    private bool _ready;

    private Tween _kickTween;

    private void Start()
    {
        if (profile == null)
        {
            Debug.LogError($"{nameof(CameraOffsetController)} on {name}: CameraFeelProfile is not assigned.", this);
            return;
        }

        _state = (PlayerState)playerController.GetState;
        _combat = playerController.GetCombat;

        if (outputCamera == null)
        {
            Debug.LogError($"{nameof(CameraOffsetController)} on {name}: output Camera is not assigned.", this);
            return;
        }

        _currentScreenPosition = Vector2.zero;
        _cam = outputCamera;

        var lens = virtualCamera.Lens;
        lens.OrthographicSize = profile.DefaultOrthographicSize;
        virtualCamera.Lens = lens;

        _zoomSettled = true;

        if (kickOffset != null)
            kickOffset.Offset = Vector3.zero;

        if (_combat != null)
            _combat.OnHitLanded += HandleHitLanded;

        _ready = true;
    }

    private void OnDestroy()
    {
        if (_combat != null)
            _combat.OnHitLanded -= HandleHitLanded;

        _kickTween?.Kill();
    }

    private void LateUpdate()
    {
        if (!_ready) return;

        bool isLocked = _combat.LockOnTransform != null;

        if (isLocked)
        {
            Transform playerTransform = virtualCamera.Follow;
            if (playerTransform != null)
            {
                Vector3 playerVP = _cam.WorldToViewportPoint(playerTransform.position);
                Vector3 targetVP = _cam.WorldToViewportPoint(_combat.LockOnTransform.position);
                UpdateLockOnScreenPosition(playerVP, targetVP);
                UpdateLockOnZoom(playerVP, targetVP);
            }
        }
        else
        {
            UpdateFreeLookScreenPosition();

            if (!_zoomSettled)
                ResetZoom();
        }

        ApplyScreenPosition();
    }

    private void UpdateLockOnScreenPosition(Vector3 playerVP, Vector3 targetVP)
    {
        Vector2 vpDelta = new Vector2(
            (targetVP.x - playerVP.x) * -ViewportToScreenOffset,
            (targetVP.y - playerVP.y) * ViewportToScreenOffset
        );

        Vector2 targetScreenPos = new Vector2(
            Mathf.Clamp(vpDelta.x, -profile.LockOnMaxOffsetX, profile.LockOnMaxOffsetX),
            Mathf.Clamp(vpDelta.y, -profile.LockOnMaxOffsetY, profile.LockOnMaxOffsetY)
        );

        _currentScreenPosition = Vector2.Lerp(
            _currentScreenPosition,
            targetScreenPos,
            profile.LockOnSmoothSpeed * Time.deltaTime
        );
    }

    private void UpdateLockOnZoom(Vector3 playerVP, Vector3 targetVP)
    {
        float vpDistance = new Vector2(
            targetVP.x - playerVP.x,
            targetVP.y - playerVP.y
        ).magnitude;

        float defaultSize = profile.DefaultOrthographicSize;
        float threshold = profile.ZoomThresholdViewport;

        float targetSize;
        if (vpDistance > threshold)
        {
            float excess = vpDistance - threshold;
            float maxExcess = MaxViewportDistance - threshold;
            float t = Mathf.Clamp01(excess / maxExcess);
            targetSize = Mathf.Lerp(defaultSize, profile.MaxOrthographicSize, t);
        }
        else
        {
            targetSize = defaultSize;
        }

        var lens = virtualCamera.Lens;
        lens.OrthographicSize = Mathf.Lerp(
            lens.OrthographicSize,
            targetSize,
            profile.ZoomSmoothSpeed * Time.deltaTime
        );
        virtualCamera.Lens = lens;

        _zoomSettled = false;
    }

    private void ResetZoom()
    {
        float defaultSize = profile.DefaultOrthographicSize;

        var lens = virtualCamera.Lens;
        float newSize = Mathf.Lerp(
            lens.OrthographicSize,
            defaultSize,
            profile.ZoomSmoothSpeed * Time.deltaTime
        );

        if (Mathf.Abs(newSize - defaultSize) < profile.ZoomEpsilon)
        {
            newSize = defaultSize;
            _zoomSettled = true;
        }

        lens.OrthographicSize = newSize;
        virtualCamera.Lens = lens;
    }

    private void UpdateFreeLookScreenPosition()
    {
        Vector2 input = _state.CurrentCameraInput;

        Vector2 targetScreenPos = new Vector2(
            -input.x * profile.MaxOffsetX,
            input.y * profile.MaxOffsetY
        );

        _currentScreenPosition = Vector2.Lerp(
            _currentScreenPosition,
            targetScreenPos,
            profile.FreeLookSmoothSpeed * Time.deltaTime
        );
    }

    private void ApplyScreenPosition()
    {
        var composition = composer.Composition;
        composition.ScreenPosition = _currentScreenPosition;
        composer.Composition = composition;
    }

    private void HandleHitLanded(CharacterStateBase target, DamageResult result)
    {
        if (kickOffset == null) return;

        float distance = profile.HitKickDistance;
        if (distance <= 0f) return;

        Transform playerTransform = playerController.transform;

        Vector3 direction = Vector3.zero;
        if (target != null)
        {
            direction = target.transform.position - playerTransform.position;
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < MinSqrDirection)
        {
            direction = playerTransform.forward;
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < MinSqrDirection) return;

        direction.Normalize();

        if (result.targetDied)
            distance *= profile.KillKickMultiplier;

        Vector3 peak = virtualCamera.transform.InverseTransformDirection(direction * distance);
        peak.z = 0f;

        _kickTween?.Kill();
        kickOffset.Offset = Vector3.zero;

        _kickTween = DOTween.To(() => kickOffset.Offset, v => kickOffset.Offset = v, peak, profile.HitKickDuration * HalfCycle)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo)
            .SetUpdate(true)
            .SetLink(gameObject)
            .OnKill(ResetKickOffset);
    }

    private void ResetKickOffset()
    {
        if (kickOffset != null)
            kickOffset.Offset = Vector3.zero;
    }
}
