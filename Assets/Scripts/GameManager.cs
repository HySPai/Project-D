using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    private bool isRestarting;

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

    private void HandleRestartRequested()
    {
        if (isRestarting) return;
        isRestarting = true;
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
}
