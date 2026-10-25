using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField] private WorldResetController worldReset;

    private void Start()
    {
        Application.targetFrameRate = 60;
        GameEvent.OnPlayerDied += HandlePlayerDied;
        GameEvent.OnRestartRequested += HandleRestartRequested;
    }

    protected override void OnDestroy()
    {
        GameEvent.OnPlayerDied -= HandlePlayerDied;
        GameEvent.OnRestartRequested -= HandleRestartRequested;
        base.OnDestroy();
    }

    private void HandlePlayerDied() => TinySauceSDKController.Instance.OnLose();

    private void HandleRestartRequested() => worldReset.ResetWorld();
}
