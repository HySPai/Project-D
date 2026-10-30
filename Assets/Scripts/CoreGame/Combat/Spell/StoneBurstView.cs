using DG.Tweening;
using UnityEngine;

public class StoneBurstView : MonoBehaviour
{
    [SerializeField] private Transform[] spikes;

    [Header("Layout")]
    [SerializeField] private int[] rowCounts = { 3, 5, 7 };
    [SerializeField] private float[] rowDistances = { 0.35f, 0.68f, 1f };
    [SerializeField] private float outwardTilt = 18f;
    [SerializeField, Range(0f, 1f)] private float exposedRatio = 0.7f;

    [Header("Timing")]
    [SerializeField] private float rowDelay = 0.06f;
    [SerializeField] private float riseTime = 0.12f;
    [SerializeField] private float holdTime = 0.5f;
    [SerializeField] private float sinkTime = 0.3f;

    private Sequence sequence;

    /// <summary>Raises the stone spikes out of the ground row by row, filling an arc of arcDegrees centred on direction out to range, then sinks them back.</summary>
    public void Play(Vector3 origin, Vector3 direction, float range, float arcDegrees)
    {
        sequence?.Kill();
        gameObject.SetActive(true);
        transform.SetPositionAndRotation(origin, Quaternion.LookRotation(direction));

        sequence = DOTween.Sequence().SetLink(gameObject);

        int index = 0;
        for (int row = 0; row < rowCounts.Length && row < rowDistances.Length; row++)
        {
            int count = rowCounts[row];
            float distance = range * rowDistances[row];
            float delay = row * rowDelay;

            for (int i = 0; i < count && index < spikes.Length; i++, index++)
            {
                float t = count > 1 ? i / (count - 1f) : 0.5f;
                float angle = Mathf.Lerp(-arcDegrees * 0.5f, arcDegrees * 0.5f, t);
                RaiseSpike(spikes[index], origin, direction, angle, distance, delay);
            }
        }

        for (; index < spikes.Length; index++)
            spikes[index].gameObject.SetActive(false);

        sequence.OnComplete(() => gameObject.SetActive(false));
    }

    private void RaiseSpike(Transform spike, Vector3 origin, Vector3 direction, float angle, float distance, float delay)
    {
        spike.gameObject.SetActive(true);

        Vector3 flat = Quaternion.AngleAxis(angle, Vector3.up) * direction;
        Vector3 ground = origin + flat * distance;
        float height = spike.localScale.y;

        Vector3 buried = ground - Vector3.up * (height * 0.5f);
        Vector3 raised = ground + Vector3.up * (height * (exposedRatio - 0.5f));

        spike.SetPositionAndRotation(buried, Quaternion.LookRotation(flat) * Quaternion.Euler(outwardTilt, 0f, 0f) * Quaternion.Euler(0f, 45f, 0f));

        sequence.Insert(delay, spike.DOMove(raised, riseTime).SetEase(Ease.OutBack));
        sequence.Insert(delay + riseTime + holdTime, spike.DOMove(buried, sinkTime).SetEase(Ease.InQuad));
    }
}
