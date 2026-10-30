using DG.Tweening;
using UnityEngine;

public class WaterRingView : MonoBehaviour
{
    [SerializeField] private Transform[] outerDrops;
    [SerializeField] private Transform[] innerDrops;

    [Header("Shape")]
    [SerializeField] private float startRadius = 0.4f;
    [SerializeField, Range(0f, 1f)] private float innerRadiusRatio = 0.6f;
    [SerializeField] private float crestHeight = 0.5f;
    [SerializeField] private float groundOffset = 0.1f;

    [Header("Timing")]
    [SerializeField] private float spreadTime = 0.4f;
    [SerializeField] private float innerDelay = 0.12f;
    [SerializeField] private float shrinkTime = 0.15f;

    private Vector3[] outerScales;
    private Vector3[] innerScales;
    private Sequence sequence;

    private void Awake()
    {
        outerScales = CaptureScales(outerDrops);
        innerScales = CaptureScales(innerDrops);
    }

    /// <summary>Sends a wave from the caster's feet out to radius, with a smaller ripple following just behind it.</summary>
    public void Play(Vector3 center, float radius)
    {
        sequence?.Kill();
        gameObject.SetActive(true);
        transform.SetPositionAndRotation(center + Vector3.up * groundOffset, Quaternion.identity);

        sequence = DOTween.Sequence().SetLink(gameObject);
        SpreadRing(outerDrops, outerScales, radius, 0f, crestHeight);
        SpreadRing(innerDrops, innerScales, radius * innerRadiusRatio, innerDelay, crestHeight * 0.6f);
        sequence.OnComplete(() => gameObject.SetActive(false));
    }

    private void SpreadRing(Transform[] drops, Vector3[] scales, float radius, float delay, float height)
    {
        int count = drops.Length;
        for (int i = 0; i < count; i++)
        {
            float angle = i * 360f / count;
            Vector3 outward = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
            Transform drop = drops[i];

            drop.localPosition = outward * startRadius;
            drop.localRotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, outward));
            drop.localScale = Vector3.zero;

            Vector3 end = outward * radius;
            sequence.Insert(delay, drop.DOScale(scales[i], spreadTime * 0.3f).SetEase(Ease.OutQuad));
            sequence.Insert(delay, drop.DOLocalMoveX(end.x, spreadTime).SetEase(Ease.OutCubic));
            sequence.Insert(delay, drop.DOLocalMoveZ(end.z, spreadTime).SetEase(Ease.OutCubic));
            sequence.Insert(delay, drop.DOLocalMoveY(height, spreadTime * 0.5f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo));
            sequence.Insert(delay + spreadTime, drop.DOScale(Vector3.zero, shrinkTime).SetEase(Ease.InQuad));
        }
    }

    private static Vector3[] CaptureScales(Transform[] drops)
    {
        var scales = new Vector3[drops.Length];
        for (int i = 0; i < drops.Length; i++)
            scales[i] = drops[i].localScale;
        return scales;
    }
}
