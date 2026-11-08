using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>One heart icon of the health bar; pops when filled and shrinks away when emptied.</summary>
public class Heart : MonoBehaviour
{
    [SerializeField] private Image fullHeart;

    [Header("Anim")]
    [SerializeField] private float shrinkDuration = 0.25f;
    [SerializeField] private float popScale = 1.3f;
    [SerializeField] private float popDuration = 0.3f;

    private bool isFilled = true;
    private Tween tween;
    private TweenCallback hideFullHeart;

    /// <summary>Sets the initial state without animating.</summary>
    public void Init(bool filled)
    {
        isFilled = filled;
        tween?.Kill();

        if (fullHeart != null)
        {
            fullHeart.enabled = filled;
            fullHeart.transform.localScale = Vector3.one;
        }
    }

    /// <summary>Animates to filled or empty when the state changes.</summary>
    public void SetFilled(bool filled)
    {
        if (isFilled == filled) return;
        isFilled = filled;

        if (filled) Fill();
        else Empty();
    }

    private void Empty()
    {
        if (fullHeart == null) return;

        tween?.Kill();
        Transform t = fullHeart.transform;
        fullHeart.enabled = true;
        tween = t.DOScale(0f, shrinkDuration)
            .SetEase(Ease.InBack)
            .SetLink(gameObject)
            .OnComplete(hideFullHeart ??= HideFullHeart);
    }

    private void HideFullHeart() => fullHeart.enabled = false;

    private void Fill()
    {
        if (fullHeart == null) return;

        tween?.Kill();
        Transform t = fullHeart.transform;
        fullHeart.enabled = true;
        t.localScale = Vector3.one;

        tween = DOTween.Sequence()
            .Append(t.DOScale(popScale, popDuration * 0.5f).SetEase(Ease.OutQuad))
            .Append(t.DOScale(1f, popDuration * 0.5f).SetEase(Ease.InQuad))
            .SetLink(gameObject);
    }
}