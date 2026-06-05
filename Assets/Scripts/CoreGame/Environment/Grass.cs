using UnityEngine;
using DG.Tweening;

public class Grass : MonoBehaviour
{
    [Header("Rotate Settings")]
    public float maxAngle = 25f;
    public float duration = 0.3f;
    public float returnDuration = 0.15f;

    private Quaternion originalRotation;
    private Tween currentTween;

    private void Awake()
    {
        originalRotation = transform.localRotation;
    }

    public void Bend(Vector3 playerPos)
    {
        Vector3 dir = (transform.position - playerPos).normalized;

        float angleX = dir.z * maxAngle;
        float angleZ = -dir.x * maxAngle;

        Quaternion targetRotation = Quaternion.Euler(angleX, 0f, angleZ);

        currentTween?.Kill();
        currentTween = transform.DOLocalRotateQuaternion(targetRotation, duration)
            .SetEase(Ease.OutQuad);
    }

    public void ResetGrass()
    {
        currentTween?.Kill();
        currentTween = transform.DOLocalRotateQuaternion(originalRotation, returnDuration)
            .SetEase(Ease.OutBack);
    }
}