using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QualityControl : MonoBehaviour
{
    private int deviceMem = 5000;
    private const int MEM_SUPER_LOW = 2000;
    private const int MEM_LOW = 3000;
    private const int MEM_MEDIUM = 4500;
    public static RAM_ENUM ramQuality = RAM_ENUM.HIGH;
    private void Awake()
    {
        /*#if !UNITY_EDITOR
                // off toàn bộ log nếu đang chạy trên device thật
                Debug.unityLogger.logEnabled = false;
        #endif*/
#if UNITY_ANDROID
        try
        {
            deviceMem = SystemInfo.systemMemorySize;
            if (deviceMem <= 0) return;
            if (deviceMem < MEM_SUPER_LOW)
            {
                ramQuality = RAM_ENUM.SUPER_LOW;
                QualitySettings.SetQualityLevel(0);
            }
            else if (deviceMem < MEM_LOW)
            {
                ramQuality = RAM_ENUM.LOW;
                QualitySettings.SetQualityLevel(1);
            }
            else if (deviceMem < MEM_MEDIUM)
            {
                ramQuality = RAM_ENUM.MEDIUM;
                QualitySettings.SetQualityLevel(1);
            } 
            else
            {
                ramQuality = RAM_ENUM.HIGH;
                QualitySettings.SetQualityLevel(2);
            }
        }
        catch (System.Exception e)
        {
            ramQuality = RAM_ENUM.HIGH;
        }

#endif

    }
}
public enum RAM_ENUM
{
    SUPER_LOW,
    LOW,
    MEDIUM,
    HIGH
}
