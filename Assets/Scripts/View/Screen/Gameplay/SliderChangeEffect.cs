using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives a 0-1 slider with a trailing bar. Small continuous changes (regen, drain) are applied directly; a sudden drop
/// snaps the fill and lets the trail catch up after a delay, a sudden gain shows the trail first and tweens the fill up.
/// </summary>
public class SliderChangeEffect : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private RectTransform trail;
    [SerializeField] private Image trailImage;

    [Header("Sudden Change")]
    [SerializeField, Range(0f, 1f)] private float suddenChangeThreshold = 0.02f;

    [Header("Decrease")]
    [SerializeField] private Color decreaseColor = new Color(1f, 0.95f, 0.75f, 1f);
    [SerializeField] private float decreaseDelay = 0.35f;
    [SerializeField] private float decreaseDuration = 0.4f;

    [Header("Increase")]
    [SerializeField] private Color increaseColor = new Color(1f, 1f, 1f, 0.55f);
    [SerializeField] private float increaseDuration = 0.3f;

    private Tween fillTween;
    private Tween trailTween;
    private float trailValue;
    private float lastTarget;
    private float fillFrom;
    private float fillProgress;
    private float trailFrom;
    private float trailProgress;
    private bool initialized;

    private DOGetter<float> fillProgressGetter;
    private DOSetter<float> fillProgressSetter;
    private TweenCallback fillKilled;
    private DOGetter<float> trailProgressGetter;
    private DOSetter<float> trailProgressSetter;
    private TweenCallback trailKilled;

    private void CacheDelegates()
    {
        fillProgressGetter = () => fillProgress;
        fillProgressSetter = ApplyFillProgress;
        fillKilled = () => fillTween = null;
        trailProgressGetter = () => trailProgress;
        trailProgressSetter = ApplyTrailProgress;
        trailKilled = () => trailTween = null;
    }

    private void OnDestroy()
    {
        fillTween?.Kill();
        trailTween?.Kill();
    }

    /// <summary>Moves the bar to a 0-1 value, animating only changes larger than the sudden-change threshold.</summary>
    public void SetValue(float target)
    {
        target = Mathf.Clamp01(target);

        if (!initialized)
        {
            CacheDelegates();
            Snap(target);
            return;
        }

        float previous = lastTarget;
        if (target == previous) return;

        lastTarget = target;
        float delta = target - previous;

        if (delta < 0f) Decrease(target, previous, -delta > suddenChangeThreshold);
        else Increase(target, delta > suddenChangeThreshold);
    }

    private void Snap(float target)
    {
        initialized = true;
        lastTarget = target;

        fillTween?.Kill();
        trailTween?.Kill();

        slider.value = target;
        SetTrail(target);
    }

    private void Decrease(float target, float previous, bool sudden)
    {
        fillTween?.Kill();
        slider.value = target;

        if (!sudden)
        {
            if (trailTween == null) SetTrail(target);
            return;
        }

        if (trailImage != null) trailImage.color = decreaseColor;

        trailFrom = Mathf.Max(trailValue, previous);
        SetTrail(trailFrom);
        trailProgress = 0f;

        trailTween?.Kill();
        trailTween = DOTween.To(trailProgressGetter, trailProgressSetter, 1f, decreaseDuration)
            .SetDelay(decreaseDelay)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject)
            .OnKill(trailKilled);
    }

    private void Increase(float target, bool sudden)
    {
        if (!sudden && fillTween == null)
        {
            slider.value = target;
            if (trailTween == null) SetTrail(target);
            return;
        }

        trailTween?.Kill();
        if (trailImage != null) trailImage.color = increaseColor;
        SetTrail(target);

        if (!sudden) return;

        fillFrom = slider.value;
        fillProgress = 0f;

        fillTween?.Kill();
        fillTween = DOTween.To(fillProgressGetter, fillProgressSetter, 1f, increaseDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject)
            .OnKill(fillKilled);
    }

    private void ApplyFillProgress(float progress)
    {
        fillProgress = progress;
        slider.value = Mathf.LerpUnclamped(fillFrom, lastTarget, progress);
    }

    private void ApplyTrailProgress(float progress)
    {
        trailProgress = progress;
        SetTrail(Mathf.LerpUnclamped(trailFrom, slider.value, progress));
    }

    private void SetTrail(float value)
    {
        trailValue = value;
        if (trail == null) return;

        Vector2 anchorMax = trail.anchorMax;
        if (anchorMax.x == value) return;

        anchorMax.x = value;
        trail.anchorMax = anchorMax;
    }
}
