using DG.Tweening;
using UnityEngine;

public class WindVortexView : MonoBehaviour
{
    private const float GoldenAngle = 137.5f;

    [SerializeField] private Transform spinner;
    [SerializeField] private Transform[] streaks;
    [SerializeField] private TrailRenderer[] trails;

    private SO_WindVortexData data;
    private Vector3[] restPositions;
    private Sequence sequence;
    private float elapsed;

    /// <summary>Opens the funnel to the vortex radius for its lifetime, then shrinks it away a little before the vortex closes.</summary>
    public void Play(SO_WindVortexData data)
    {
        this.data = data;
        LayoutStreaks();

        sequence?.Kill();
        elapsed = 0f;
        spinner.localScale = Vector3.zero;
        gameObject.SetActive(true);

        foreach (TrailRenderer trail in trails)
        {
            trail.time = data.trailTime;
            trail.Clear();
        }

        float shrinkStart = Mathf.Max(data.growTime, data.lifetime - data.shrinkTime);
        sequence = DOTween.Sequence().SetLink(gameObject)
            .Append(spinner.DOScale(Vector3.one, data.growTime).SetEase(Ease.OutBack))
            .Insert(shrinkStart, spinner.DOScale(Vector3.zero, data.shrinkTime).SetEase(Ease.InQuad));
    }

    private void LayoutStreaks()
    {
        int count = streaks.Length;
        if (restPositions == null || restPositions.Length != count)
            restPositions = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            float t = count > 1 ? i / (count - 1f) : 0f;
            float r = Mathf.Lerp(data.radius * data.baseRadiusRatio, data.radius, t);
            float angle = i * GoldenAngle * Mathf.Deg2Rad;

            restPositions[i] = new Vector3(Mathf.Cos(angle) * r, t * data.visualHeight, Mathf.Sin(angle) * r);
            streaks[i].localPosition = restPositions[i];
        }
    }

    private void Update()
    {
        if (data == null || restPositions == null) return;

        float dt = Time.deltaTime;
        elapsed += dt;
        spinner.Rotate(0f, data.spinSpeed * dt, 0f, Space.Self);

        for (int i = 0; i < streaks.Length; i++)
        {
            Vector3 rest = restPositions[i];
            rest.y += Mathf.Sin(elapsed * data.bobSpeed + i) * data.bobAmplitude;
            streaks[i].localPosition = rest;
        }
    }
}
