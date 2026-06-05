using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    RectTransform rectTransform;
    Rect safeArea;
    Vector2 minAnchor;
    Vector2 maxAnchor;
    [SerializeField] private bool NeedBannerAds = false;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        safeArea = Screen.safeArea;
        minAnchor = safeArea.position;
        maxAnchor = minAnchor + safeArea.size;

        minAnchor.x /= Screen.width;
        minAnchor.y /= Screen.height;
        maxAnchor.x /= Screen.width;
        maxAnchor.y /= Screen.height;

        rectTransform.anchorMin = minAnchor;
        rectTransform.anchorMax = maxAnchor;
    }
    //private void Start()
    //{
    //    if (!NeedBannerAds || GameSave.PlayerLevel < IntergrateMAX.Instance.bannerConfig.levelStart || GameSave.NoAds) 
    //        return;
    //    // resize for banner  
    //    rectTransform = GetComponent<RectTransform>();
    //    minAnchor = rectTransform.anchorMin;
    //    minAnchor.y = minAnchor.y + 0.1f;
    //    rectTransform.anchorMin = minAnchor;
    //}
}
