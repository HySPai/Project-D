using UnityEngine;

/// <summary>Top-level game flow: frame rate, reporting a loss to the optional analytics SDK and respawning on restart.</summary>
public class GameManager : SingletonMonoBehaviour<GameManager>
{
    private const int TargetFrameRate = 60;

    [SerializeField] private SaveController saveController;
    [SerializeField] private TinySauceSDKController analytics;

    private void Start()
    {
        Application.targetFrameRate = TargetFrameRate;
        GameEvent.OnPlayerDied += HandlePlayerDied;
        GameEvent.OnRestartRequested += HandleRestartRequested;
    }

    protected override void OnDestroy()
    {
        GameEvent.OnPlayerDied -= HandlePlayerDied;
        GameEvent.OnRestartRequested -= HandleRestartRequested;
        base.OnDestroy();
    }

    private void HandlePlayerDied()
    {
        if (analytics != null) analytics.OnLose();
    }

    private void HandleRestartRequested() => saveController.Respawn();
}
