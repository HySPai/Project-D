using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(Button))]
public class PressAndHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Hold Settings")]
    [SerializeField] private float holdDuration = 0.5f; // Thời gian cần giữ để kích hoạt
    [SerializeField] private float repeatRate = 0.1f; // Tần suất lặp lại event khi giữ
    
    [Header("Visual Feedback")]
    [SerializeField] private bool useVisualFeedback = true;
    [SerializeField] private float scaleWhileHolding = 0.95f;

    private Button button;
    private bool isHolding = false;
    private Vector3 originalScale;
    private WaitForSeconds waitForRepeat;
    private Coroutine holdCoroutine;

    private void Start()
    {
        button = GetComponent<Button>();
        waitForRepeat = new WaitForSeconds(repeatRate);
        originalScale=transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;

        isHolding = true;
        holdCoroutine = StartCoroutine(HoldCoroutine());

        if (useVisualFeedback)
        {
            transform.localScale = originalScale * scaleWhileHolding;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopHolding();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // StopHolding();
    }

    private void StopHolding()
    {
        if (isHolding)
        {
            isHolding = false;
            if (holdCoroutine != null)
            {
                StopCoroutine(holdCoroutine);
                holdCoroutine = null;
            }

            if (useVisualFeedback)
            {
                transform.localScale = originalScale;
            }
        }
    }

    private IEnumerator HoldCoroutine()
    {
        // Đợi thời gian holdDuration trước khi bắt đầu
        yield return new WaitForSeconds(holdDuration);

        // Lặp lại event khi đang giữ
        while (isHolding && button.interactable)
        {
            button.onClick.Invoke();
            yield return waitForRepeat;
        }
    }

    private void OnDisable()
    {
        StopHolding();
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        holdDuration = Mathf.Max(0.1f, holdDuration);
        repeatRate = Mathf.Max(0.05f, repeatRate);
        scaleWhileHolding = Mathf.Clamp(scaleWhileHolding, 0.5f, 1f);
    }
    #endif
}
