using Unity.Cinemachine;
using UnityEngine;

public class CameraOffsetController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private CinemachinePositionComposer composer;
    [SerializeField] private PlayerController playerController;

    [Header("Free Look Screen Position")]
    [SerializeField] private float maxOffsetX = 0.15f;
    [SerializeField] private float maxOffsetY = 0.15f;
    [SerializeField] private float freeLookSmoothSpeed = 5f;

    [Header("Lock On Screen Position")]
    [SerializeField] private float lockOnSmoothSpeed = 5f;
    [SerializeField] private float lockOnMaxOffsetX = 0.3f;
    [SerializeField] private float lockOnMaxOffsetY = 0.3f;

    [Header("Lock On Lens Zoom")]
    [SerializeField] private float defaultOrthographicSize = 4f;
    [SerializeField] private float maxOrthographicSize = 8f;
    [SerializeField] private float zoomSmoothSpeed = 3f;
    [SerializeField] private float zoomThresholdViewport = 0.6f;

    [Header("Settle Threshold")]
    [SerializeField] private float screenPositionEpsilon = 0.001f;
    [SerializeField] private float zoomEpsilon = 0.01f;

    private PlayerState _state;
    private CharacterCombatBase _combat;

    private Vector2 _currentScreenPosition;
    private Camera _cam;

    private bool _screenPositionSettled;
    private bool _zoomSettled;

    private void Start()
    {
        _state = (PlayerState)playerController.GetState;
        _combat = playerController.GetCombat;

        _currentScreenPosition = Vector2.zero;
        _cam = Camera.main;

        var lens = virtualCamera.Lens;
        lens.OrthographicSize = defaultOrthographicSize;
        virtualCamera.Lens = lens;

        _screenPositionSettled = true;
        _zoomSettled = true;
    }

    private void LateUpdate()
    {
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

        if (!_screenPositionSettled || isLocked)
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
            Mathf.Clamp(vpDelta.x, -lockOnMaxOffsetX, lockOnMaxOffsetX),
            Mathf.Clamp(vpDelta.y, -lockOnMaxOffsetY, lockOnMaxOffsetY)
        );

        _currentScreenPosition = Vector2.Lerp(
            _currentScreenPosition,
            targetScreenPos,
            lockOnSmoothSpeed * Time.deltaTime
        );

        _screenPositionSettled = false;
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

        float targetSize;
        if (vpDistance > zoomThresholdViewport)
        {
            float excess = vpDistance - zoomThresholdViewport;
            float maxExcess = Mathf.Sqrt(2f) - zoomThresholdViewport;
            float t = Mathf.Clamp01(excess / maxExcess);
            targetSize = Mathf.Lerp(defaultOrthographicSize, maxOrthographicSize, t);
        }
        else
        {
            targetSize = defaultOrthographicSize;
        }

        var lens = virtualCamera.Lens;
        lens.OrthographicSize = Mathf.Lerp(
            lens.OrthographicSize,
            targetSize,
            zoomSmoothSpeed * Time.deltaTime
        );
        virtualCamera.Lens = lens;

        _zoomSettled = false;
    }

    private void ResetZoom()
    {
        var lens = virtualCamera.Lens;
        float newSize = Mathf.Lerp(
            lens.OrthographicSize,
            defaultOrthographicSize,
            zoomSmoothSpeed * Time.deltaTime
        );

        if (Mathf.Abs(newSize - defaultOrthographicSize) < zoomEpsilon)
        {
            newSize = defaultOrthographicSize;
            _zoomSettled = true;
        }

        lens.OrthographicSize = newSize;
        virtualCamera.Lens = lens;
    }

    private void UpdateFreeLookScreenPosition()
    {
        Vector2 input = _state.CurrentCameraInput;

        Vector2 targetScreenPos = new Vector2(
            -input.x * maxOffsetX,
            input.y * maxOffsetY
        );

        _currentScreenPosition = Vector2.Lerp(
            _currentScreenPosition,
            targetScreenPos,
            freeLookSmoothSpeed * Time.deltaTime
        );

        if ((_currentScreenPosition - targetScreenPos).sqrMagnitude < screenPositionEpsilon * screenPositionEpsilon)
        {
            _currentScreenPosition = targetScreenPos;
            _screenPositionSettled = true;
        }
    }

    private void ApplyScreenPosition()
    {
        var composition = composer.Composition;
        composition.ScreenPosition = _currentScreenPosition;
        composer.Composition = composition;
    }
}