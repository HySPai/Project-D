using DG.Tweening;
using UnityEngine;

public class WaterRingView : MonoBehaviour
{
    [SerializeField] private Transform[] outerDrops;
    [SerializeField] private Transform[] innerDrops;

    private Vector3[] outerScales;
    private Vector3[] innerScales;
    private Sequence sequence;

    private void Awake()
    {
        outerScales = CaptureScales(outerDrops);
        innerScales = CaptureScales(innerDrops);
    }

    /// <summary>Sends a wave from the caster's feet out to the ring radius, with a smaller ripple following just behind it.</summary>
    public void Play(Vector3 center, SO_WaterRingData data)
    {
        sequence?.Kill();
        gameObject.SetActive(true);
        transform.SetPositionAndRotation(center + Vector3.up * data.groundOffset, Quaternion.identity);

        sequence = DOTween.Sequence().SetLink(gameObject);
        SpreadRing(outerDrops, outerScales, data.radius, 0f, data.crestHeight, data);
        SpreadRing(innerDrops, innerScales, data.radius * data.innerRadiusRatio, data.innerDelay,
            data.crestHeight * data.innerCrestRatio, data);
        sequence.OnComplete(() => gameObject.SetActive(false));
    }

    private void SpreadRing(Transform[] drops, Vector3[] scales, float radius, float delay, float height, SO_WaterRingData data)
    {
        int count = drops.Length;
        for (int i = 0; i < count; i++)
        {
            float angle = i * 360f / count;
            Vector3 outward = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
            Transform drop = drops[i];

            drop.localPosition = outward * data.startRadius;
            drop.localRotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, outward));
            drop.localScale = Vector3.zero;

            Vector3 end = outward * radius;
            sequence.Insert(delay, drop.DOScale(scales[i], data.spreadTime * data.growRatio).SetEase(Ease.OutQuad));
            sequence.Insert(delay, drop.DOLocalMoveX(end.x, data.spreadTime).SetEase(Ease.OutCubic));
            sequence.Insert(delay, drop.DOLocalMoveZ(end.z, data.spreadTime).SetEase(Ease.OutCubic));
            sequence.Insert(delay, drop.DOLocalMoveY(height, data.spreadTime * 0.5f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo));
            sequence.Insert(delay + data.spreadTime, drop.DOScale(Vector3.zero, data.shrinkTime).SetEase(Ease.InQuad));
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
