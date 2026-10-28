using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SliderChangeEffect : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private RectTransform trail;
    [SerializeField] private Image trailImage;

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
    private bool initialized;

    /// <summary>Moves the bar to a 0–1 value: a drop snaps the fill and lets a trail catch up, a gain shows the trail first and fills up to it.</summary>
    public void SetValue(float target)
    {
        target = Mathf.Clamp01(target);

        if (!initialized)
        {
            Snap(target);
            return;
        }

        if (target < lastTarget) Decrease(target);
        else if (target > lastTarget) Increase(target);

        lastTarget = target;
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

    private void Decrease(float target)
    {
        fillTween?.Kill();
        slider.value = target;

        if (trailImage != null) trailImage.color = decreaseColor;
        if (trailValue < lastTarget) SetTrail(lastTarget);

        trailTween?.Kill();
        trailTween = DOTween.To(() => trailValue, SetTrail, target, decreaseDuration)
            .SetDelay(decreaseDelay)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    private void Increase(float target)
    {
        trailTween?.Kill();
        if (trailImage != null) trailImage.color = increaseColor;
        SetTrail(target);

        fillTween?.Kill();
        fillTween = slider.DOValue(target, increaseDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    private void SetTrail(float value)
    {
        trailValue = value;
        if (trail != null) trail.anchorMax = new Vector2(value, trail.anchorMax.y);
    }
}
