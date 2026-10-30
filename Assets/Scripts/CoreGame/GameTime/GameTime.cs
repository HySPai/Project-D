using UnityEngine;

/// <summary>Single owner of Time.timeScale: combines the pause and slow-motion requests so one system never overwrites another.</summary>
public static class GameTime
{
    private static bool paused;
    private static float slowMotionScale = 1f;
    private static float baseFixedDeltaTime = -1f;

    public static bool IsPaused => paused;
    public static float SlowMotionScale => slowMotionScale;

    /// <summary>Freezes or resumes the game, e.g. while a menu that stops time is open.</summary>
    public static void SetPaused(bool value)
    {
        paused = value;
        Apply();
    }

    /// <summary>Slows the game down to scale until ClearSlowMotion; a pause still wins over it.</summary>
    public static void SetSlowMotion(float scale)
    {
        slowMotionScale = Mathf.Clamp01(scale);
        Apply();
    }

    /// <summary>Ends any slow motion and goes back to normal speed unless paused.</summary>
    public static void ClearSlowMotion() => SetSlowMotion(1f);

    /// <summary>Drops every request and runs the game at normal speed, as a world reset does.</summary>
    public static void ResetAll()
    {
        paused = false;
        slowMotionScale = 1f;
        Apply();
    }

    /// <summary>The time scale that results from a pause flag and a slow-motion scale.</summary>
    public static float Resolve(bool isPaused, float slowScale) => isPaused ? 0f : Mathf.Clamp01(slowScale);

    private static void Apply()
    {
        if (baseFixedDeltaTime < 0f) baseFixedDeltaTime = Time.fixedDeltaTime;

        float scale = Resolve(paused, slowMotionScale);
        Time.timeScale = scale;
        if (scale > 0f) Time.fixedDeltaTime = baseFixedDeltaTime * scale;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        paused = false;
        slowMotionScale = 1f;
        baseFixedDeltaTime = -1f;
    }
}
