using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    private float deltaTime = 0.0f;

    void Update()
    {
        // Tính thời gian giữa các frame
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        // Tính toán FPS
        float fps = 1.0f / deltaTime;
        string text = $"FPS: {Mathf.Ceil(fps)}";

        // Hiển thị FPS trên góc trái màn hình
        GUIStyle style = new GUIStyle();
        style.fontSize = 30;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(10, 10, 150, 50), text, style);
    }
}
