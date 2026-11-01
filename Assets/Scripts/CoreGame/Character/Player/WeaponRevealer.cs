using DG.Tweening;
using UnityEngine;

/// <summary>Drives the StylizedPainterly reveal cut on one weapon mesh, from its start end to its far end.</summary>
public class WeaponRevealer : MonoBehaviour
{
    private static readonly int RevealProgressId = Shader.PropertyToID("_RevealProgress");
    private static readonly int RevealAxisId = Shader.PropertyToID("_RevealAxis");
    private static readonly int RevealRangeId = Shader.PropertyToID("_RevealRange");
    private static readonly int RevealEdgeWidthId = Shader.PropertyToID("_RevealEdgeWidth");
    private static readonly int RevealEdgeColorId = Shader.PropertyToID("_RevealEdgeColor");
    private static readonly int RevealNoiseScaleId = Shader.PropertyToID("_RevealNoiseScale");
    private static readonly int RevealNoiseStrengthId = Shader.PropertyToID("_RevealNoiseStrength");

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private Vector3 revealAxis = Vector3.up;

    private MaterialPropertyBlock block;
    private Tween tween;
    private float progress = 1f;

    public void Initialize(SO_PlayerStanceConfig config)
    {
        block = new MaterialPropertyBlock();

        Vector3 axis = revealAxis.sqrMagnitude > 0.0001f ? revealAxis.normalized : Vector3.up;
        Vector2 range = ProjectBounds(meshFilter.sharedMesh.bounds, axis);

        targetRenderer.GetPropertyBlock(block);
        block.SetVector(RevealAxisId, axis);
        block.SetVector(RevealRangeId, range);
        block.SetFloat(RevealEdgeWidthId, config.revealEdgeWidth);
        block.SetColor(RevealEdgeColorId, config.revealEdgeColor);
        block.SetFloat(RevealNoiseScaleId, config.revealNoiseScale);
        block.SetFloat(RevealNoiseStrengthId, config.revealNoiseStrength);
        targetRenderer.SetPropertyBlock(block);
    }

    public void SetProgress(float value)
    {
        tween?.Kill();
        ApplyProgress(value);
    }

    public void Reveal(float delay, float duration) => TweenTo(1f, delay, duration, Ease.OutQuad);

    public void Dissolve(float delay, float duration) => TweenTo(0f, delay, duration, Ease.InQuad);

    private void TweenTo(float target, float delay, float duration, Ease ease)
    {
        tween?.Kill();
        tween = DOTween.To(() => progress, ApplyProgress, target, duration)
            .SetDelay(delay)
            .SetEase(ease)
            .SetLink(gameObject);
    }

    private void ApplyProgress(float value)
    {
        progress = Mathf.Clamp01(value);
        targetRenderer.enabled = progress > 0f;

        targetRenderer.GetPropertyBlock(block);
        block.SetFloat(RevealProgressId, progress);
        targetRenderer.SetPropertyBlock(block);
    }

    private static Vector2 ProjectBounds(Bounds bounds, Vector3 axis)
    {
        Vector3 extents = bounds.extents;
        float center = Vector3.Dot(bounds.center, axis);
        float radius = Mathf.Abs(axis.x) * extents.x + Mathf.Abs(axis.y) * extents.y + Mathf.Abs(axis.z) * extents.z;
        return new Vector2(center - radius, center + radius);
    }

    private void OnDestroy() => tween?.Kill();
}
