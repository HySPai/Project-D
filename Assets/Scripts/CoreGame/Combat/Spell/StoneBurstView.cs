using DG.Tweening;
using UnityEngine;

public class StoneBurstView : MonoBehaviour
{
    [SerializeField] private Transform[] spikes;

    private Sequence sequence;

    /// <summary>Raises the stone spikes out of the ground row by row, filling the skill's arc out to its range, then sinks them back.</summary>
    public void Play(Vector3 origin, Vector3 direction, SO_StoneBurstData data)
    {
        sequence?.Kill();
        gameObject.SetActive(true);
        transform.SetPositionAndRotation(origin, Quaternion.LookRotation(direction));

        sequence = DOTween.Sequence().SetLink(gameObject);

        int index = 0;
        for (int row = 0; row < data.rowCounts.Length && row < data.rowDistances.Length; row++)
        {
            int count = data.rowCounts[row];
            float distance = data.range * data.rowDistances[row];
            float delay = row * data.rowDelay;

            for (int i = 0; i < count && index < spikes.Length; i++, index++)
            {
                float t = count > 1 ? i / (count - 1f) : 0.5f;
                float angle = Mathf.Lerp(-data.arc * 0.5f, data.arc * 0.5f, t);
                RaiseSpike(spikes[index], origin, direction, angle, distance, delay, data);
            }
        }

        for (; index < spikes.Length; index++)
            spikes[index].gameObject.SetActive(false);

        sequence.OnComplete(() => gameObject.SetActive(false));
    }

    private void RaiseSpike(Transform spike, Vector3 origin, Vector3 direction, float angle, float distance, float delay, SO_StoneBurstData data)
    {
        Vector3 flat = Quaternion.AngleAxis(angle, Vector3.up) * direction;

        if (!TryFindGround(origin + flat * distance, origin.y, data, out Vector3 ground))
        {
            spike.gameObject.SetActive(false);
            return;
        }

        spike.gameObject.SetActive(true);
        float height = spike.localScale.y;

        Vector3 buried = ground - Vector3.up * (height * 0.5f);
        Vector3 raised = ground + Vector3.up * (height * (data.exposedRatio - 0.5f));

        spike.SetPositionAndRotation(buried, Quaternion.LookRotation(flat) * Quaternion.Euler(data.outwardTilt, 0f, 0f) * Quaternion.Euler(0f, 45f, 0f));

        sequence.Insert(delay, spike.DOMove(raised, data.riseTime).SetEase(Ease.OutBack));
        sequence.Insert(delay + data.riseTime + data.holdTime, spike.DOMove(buried, data.sinkTime).SetEase(Ease.InQuad));
    }

    /// <summary>Finds the ground under point within maxRise above and maxDrop below casterHeight; fails inside or right against a wall, or over a drop that is too deep.</summary>
    private static bool TryFindGround(Vector3 point, float casterHeight, SO_StoneBurstData data, out Vector3 ground)
    {
        Vector3 start = new Vector3(point.x, casterHeight + data.maxRise, point.z);
        ground = default;

        if (Physics.CheckSphere(start, data.probeRadius, data.groundLayer, QueryTriggerInteraction.Ignore))
            return false;

        if (!Physics.Raycast(start, Vector3.down, out RaycastHit hit, data.maxRise + data.maxDrop, data.groundLayer, QueryTriggerInteraction.Ignore))
            return false;

        ground = hit.point;
        Vector3 clearance = ground + Vector3.up * data.clearanceHeight;
        return !Physics.CheckSphere(clearance, data.clearanceRadius, data.groundLayer, QueryTriggerInteraction.Ignore);
    }
}
