using UnityEngine;

/// <summary>Camera-relative movement basis for the player, and the camera used for lock-on view checks.</summary>
public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Camera viewCamera;

    private Vector3 cachedForward;
    private Vector3 cachedRight;

    /// <summary>Movement basis transform: cameraTransform when assigned, otherwise the view camera's transform.</summary>
    public Transform CameraTransform =>
        cameraTransform != null ? cameraTransform : viewCamera != null ? viewCamera.transform : null;

    /// <summary>Camera that decides whether a lock-on target is on screen.</summary>
    public Camera ViewCamera => viewCamera;

    public Vector3 Forward => cachedForward;
    public Vector3 Right => cachedRight;

    private void Awake()
    {
        CacheDirections();
    }

    private void CacheDirections()
    {
        Transform basis = CameraTransform;
        if (basis == null) return;

        Vector3 f = basis.forward;
        f.y = 0f;
        cachedForward = f.normalized;

        Vector3 r = basis.right;
        r.y = 0f;
        cachedRight = r.normalized;
    }

    /// <summary>World-space move direction for a stick input, on the cached camera basis.</summary>
    public Vector3 GetMoveDirection(Vector2 input)
    {
        return cachedRight * input.x + cachedForward * input.y;
    }
}
