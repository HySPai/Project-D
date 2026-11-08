using UnityEngine;

/// <summary>
/// Scene-placed manager reachable through Instance. The instance registers itself in Awake; a duplicate destroys
/// itself. Nothing is created on demand: a missing manager is a scene setup error and is reported once.
/// Prefer passing references through a controller; this exists for the few cross-cutting managers.
/// </summary>
public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static bool reportedMissing;

    /// <summary>The live instance, or null outside play mode or when the scene has none.</summary>
    public static T Instance
    {
        get
        {
            if (instance != null || !Application.isPlaying) return instance;

            instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);
            if (instance == null && !reportedMissing)
            {
                reportedMissing = true;
                Debug.LogError($"{typeof(T).Name} is missing from the scene.");
            }
            return instance;
        }
    }

    /// <summary>True when an instance has registered.</summary>
    public static bool HasInstance => instance != null;

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            reportedMissing = false;
            return;
        }

        if (instance != this) Destroy(this);
    }

    protected virtual void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
