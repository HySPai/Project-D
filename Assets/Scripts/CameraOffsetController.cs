using Unity.Cinemachine;
using UnityEngine;

public class CameraOffsetController : MonoBehaviour
{
    [SerializeField] private CinemachinePositionComposer composer;
    [SerializeField] private PlayerState state;

    [Header("Screen Position")]
    [SerializeField] private float maxOffsetX = 0.15f;
    [SerializeField] private float maxOffsetY = 0.15f;
    [SerializeField] private float smoothSpeed = 5f;

    private Vector2 currentScreenPosition;

    private void Start()
    {
        currentScreenPosition = composer.Composition.ScreenPosition;
    }

    private void LateUpdate()
    {
        Vector2 input = state.CurrentCameraInput;

        Vector2 targetScreenPosition = new Vector2(
            -input.x * maxOffsetX,
            input.y * maxOffsetY);

        currentScreenPosition = Vector2.Lerp(
            currentScreenPosition,
            targetScreenPosition,
            smoothSpeed * Time.deltaTime);

        var composition = composer.Composition;
        composition.ScreenPosition = currentScreenPosition;
        composer.Composition = composition;
    }
}