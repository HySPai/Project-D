using UnityEngine;
using DG.Tweening;

public class AnimationScaleLoop : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private float scaleFrom = 1f;
    [SerializeField] private float scaleTo = 1.2f;
    [SerializeField] private float scaleDuration = 0.5f;
    [SerializeField] private float interval = 0.5f;
    [SerializeField] private float initialDelay = 0f;
    [SerializeField] private Ease easeType = Ease.Linear;
    [SerializeField] private bool isEnable = false;
    private Sequence scaleSequence;



    private void OnEnable()
    {
        if (isEnable)
        {
            StartScaleAnimation();
        }
    }

    private void OnDisable()
    {
        KillSequence();
    }

    private void OnDestroy()
    {
        KillSequence();
    }

    public void StartScaleAnimation()
    {
        KillSequence();

        scaleSequence = DOTween.Sequence()
            .AppendInterval(initialDelay)

            .Append(transform.DOScale(scaleTo, scaleDuration)
                .SetEase(easeType))
            .Append(transform.DOScale(scaleFrom, scaleDuration)
                .SetEase(Ease.Linear))
                            .AppendInterval(interval)
            .SetLoops(-1); // Loop vô hạn
    }

    public void KillSequence()
    {
        if (scaleSequence != null)
        {
            scaleSequence.Kill();
            scaleSequence = null;
        }
    }

}
