using DG.Tweening;
using UnityEngine;

public class WindVortexView : MonoBehaviour
{
    [SerializeField] private Transform spinner;
    [SerializeField] private Transform[] streaks;
    [SerializeField] private TrailRenderer[] trails;

    [Header("Shape")]
    [SerializeField] private float height = 3f;
    [SerializeField, Range(0f, 1f)] private float baseRadiusRatio = 0.25f;

    [Header("Motion")]
    [SerializeField] private float spinSpeed = 420f;
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float bobSpeed = 3f;

    [Header("Timing")]
    [SerializeField] private float growTime = 0.2f;
    [SerializeField] private float shrinkTime = 0.25f;

    private Vector3[] restPositions;
    private Sequence sequence;
    private float elapsed;

    /// <summary>Opens the funnel to radius for lifetime seconds, then shrinks it away a little before the vortex closes.</summary>
    public void Play(float radius, float lifetime)
    {
        LayoutStreaks(radius);

        sequence?.Kill();
        elapsed = 0f;
        spinner.localScale = Vector3.zero;
        gameObject.SetActive(true);

        foreach (TrailRenderer trail in trails)
            trail.Clear();

        float shrinkStart = Mathf.Max(growTime, lifetime - shrinkTime);
        sequence = DOTween.Sequence().SetLink(gameObject)
            .Append(spinner.DOScale(Vector3.one, growTime).SetEase(Ease.OutBack))
            .Insert(shrinkStart, spinner.DOScale(Vector3.zero, shrinkTime).SetEase(Ease.InQuad));
    }

    private void LayoutStreaks(float radius)
    {
        int count = streaks.Length;
        if (restPositions == null || restPositions.Length != count)
            restPositions = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            float t = count > 1 ? i / (count - 1f) : 0f;
            float r = Mathf.Lerp(radius * baseRadiusRatio, radius, t);
            float angle = i * 137.5f * Mathf.Deg2Rad;

            restPositions[i] = new Vector3(Mathf.Cos(angle) * r, t * height, Mathf.Sin(angle) * r);
            streaks[i].localPosition = restPositions[i];
        }
    }

    private void Update()
    {
        if (restPositions == null) return;

        float dt = Time.deltaTime;
        elapsed += dt;
        spinner.Rotate(0f, spinSpeed * dt, 0f, Space.Self);

        for (int i = 0; i < streaks.Length; i++)
        {
            Vector3 rest = restPositions[i];
            rest.y += Mathf.Sin(elapsed * bobSpeed + i) * bobAmplitude;
            streaks[i].localPosition = rest;
        }
    }
}
