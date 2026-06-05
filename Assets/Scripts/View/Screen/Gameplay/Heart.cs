using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Heart : MonoBehaviour
{
    [SerializeField] private Image fullHeart;

    [Header("Anim")]
    [SerializeField] private float shrinkDuration = 0.25f;
    [SerializeField] private float popScale = 1.3f;
    [SerializeField] private float popDuration = 0.3f;

    private bool isFilled = true;
    private Tween tween;

    // set trạng thái ban đầu, không chạy animation
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

    public void SetFilled(bool filled)
    {
        if (isFilled == filled) return;   // không đổi thì khỏi replay anim
        isFilled = filled;

        if (filled) Fill();
        else Empty();
    }

    private void Empty()
    {
        if (fullHeart == null) return;

        tween?.Kill();
        Transform t = fullHeart.transform;
        fullHeart.enabled = true;                 // còn hiện để thấy nó co lại
        tween = t.DOScale(0f, shrinkDuration)
            .SetEase(Ease.InBack)
            .SetLink(gameObject)
            .OnComplete(() => fullHeart.enabled = false);
    }

    private void Fill()
    {
        if (fullHeart == null) return;

        tween?.Kill();
        Transform t = fullHeart.transform;
        fullHeart.enabled = true;
        t.localScale = Vector3.one;

        tween = DOTween.Sequence()
            .Append(t.DOScale(popScale, popDuration * 0.5f).SetEase(Ease.OutQuad)) // phóng to
            .Append(t.DOScale(1f, popDuration * 0.5f).SetEase(Ease.InQuad))        // về 1
            .SetLink(gameObject);
    }
}