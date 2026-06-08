using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    public Transform CameraTransform => cameraTransform;

    private Vector3 _cachedForward;
    private Vector3 _cachedRight;

    public Vector3 Forward => _cachedForward;
    public Vector3 Right => _cachedRight;

    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        CacheDirections();
    }
    private void CacheDirections()
    {
        if (cameraTransform == null) return;

        Vector3 f = cameraTransform.forward;
        f.y = 0f;
        _cachedForward = f.normalized;

        Vector3 r = cameraTransform.right;
        r.y = 0f;
        _cachedRight = r.normalized;
    }

    public Vector3 GetMoveDirection(Vector2 input)
    {
        return _cachedRight * input.x + _cachedForward * input.y;
    }
}