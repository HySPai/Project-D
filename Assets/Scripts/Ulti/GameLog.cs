using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Development-only logging. Every call, including building its message string, is stripped by the compiler from
/// release builds, so gameplay code can log freely without paying for it in shipping builds.
/// </summary>
public static class GameLog
{
    /// <summary>Logs an informational message in the editor and development builds only.</summary>
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Info(string message, Object context = null) => Debug.Log(message, context);

    /// <summary>Logs a warning in the editor and development builds only.</summary>
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Warn(string message, Object context = null) => Debug.LogWarning(message, context);
}
