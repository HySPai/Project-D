using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class CameraOffsetController : MonoBehaviour
{
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

        _currentScreenPosition = Vector2.zero;
        _cam = Camera.main;

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
            UpdateLockOnScreenPosition();
            UpdateLockOnZoom();
        }
        else
        {
            UpdateFreeLookScreenPosition();

            if (!_zoomSettled)
                ResetZoom();
        }

        ApplyScreenPosition();
    }

    private void UpdateLockOnScreenPosition()
    {
        Transform playerTransform = virtualCamera.Follow;
        if (playerTransform == null) return;

        Vector3 playerVP = _cam.WorldToViewportPoint(playerTransform.position);
        Vector3 targetVP = _cam.WorldToViewportPoint(_combat.LockOnTransform.position);

        Vector2 vpDelta = new Vector2(
            (targetVP.x - playerVP.x) * -0.5f,
            (targetVP.y - playerVP.y) * 0.5f
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

    private void UpdateLockOnZoom()
    {
        Transform playerTransform = virtualCamera.Follow;
        if (playerTransform == null) return;

        Vector3 playerVP = _cam.WorldToViewportPoint(playerTransform.position);
        Vector3 targetVP = _cam.WorldToViewportPoint(_combat.LockOnTransform.position);

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
            float maxExcess = Mathf.Sqrt(2f) - threshold;
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

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = playerTransform.forward;
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 0.0001f) return;

        direction.Normalize();

        if (result.targetDied)
            distance *= profile.KillKickMultiplier;

        Vector3 peak = virtualCamera.transform.InverseTransformDirection(direction * distance);
        peak.z = 0f;

        _kickTween?.Kill();
        kickOffset.Offset = Vector3.zero;

        _kickTween = DOTween.To(() => kickOffset.Offset, v => kickOffset.Offset = v, peak, profile.HitKickDuration * 0.5f)
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
