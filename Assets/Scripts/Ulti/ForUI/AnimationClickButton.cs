using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class AnimationClickButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Animation Settings")]
    [SerializeField] private float scaleOnClick = 0.8f;
    [SerializeField] private float animationDuration = 0.1f;
    [SerializeField] private Ease easeType = Ease.OutBack;

    private Vector3 originalScale;
    private Button button;
    private Sequence currentAnimation;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        // Kill animation khi disable để tránh lỗi
        KillAnimation();
        // Reset scale về giá trị ban đầu
        transform.localScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;

        // Kill animation hiện tại nếu có
        KillAnimation();

        // Tạo animation mới
        currentAnimation = DOTween.Sequence()
            .Append(transform.DOScale(originalScale * scaleOnClick, animationDuration)
            .SetEase(easeType));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;

        // Kill animation hiện tại nếu có
        KillAnimation();

        // Tạo animation mới
        currentAnimation = DOTween.Sequence()
            .Append(transform.DOScale(originalScale, animationDuration)
            .SetEase(easeType));
    }

    private void KillAnimation()
    {
        if (currentAnimation != null && currentAnimation.IsPlaying())
        {
            currentAnimation.Kill();
            currentAnimation = null;
        }
    }
}
