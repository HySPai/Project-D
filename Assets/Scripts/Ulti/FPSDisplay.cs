using UnityEngine;

/// <summary>Shows a smoothed frame rate in the top-left corner; the label text is rebuilt only a few times per second.</summary>
public class FPSDisplay : MonoBehaviour
{
    private const float Smoothing = 0.1f;
    private const float RefreshInterval = 0.25f;
    private const int FontSize = 30;

    private static readonly Rect LabelRect = new Rect(10f, 10f, 150f, 50f);

    private float deltaTime;
    private float nextRefresh;
    private string label = string.Empty;
    private GUIStyle style;

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * Smoothing;
        if (Time.unscaledTime < nextRefresh || deltaTime <= 0f) return;

        nextRefresh = Time.unscaledTime + RefreshInterval;
        label = "FPS: " + Mathf.CeilToInt(1f / deltaTime);
    }

    private void OnGUI()
    {
        style ??= new GUIStyle { fontSize = FontSize, normal = { textColor = Color.white } };
        GUI.Label(LabelRect, label, style);
    }
}
