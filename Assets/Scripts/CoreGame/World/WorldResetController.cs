using UnityEngine;

public class WorldResetController : MonoBehaviour
{
    [SerializeField] private CharacterControllerBase[] characters;
    [SerializeField] private Turret[] turrets;

    /// <summary>Puts the whole world back to its resting state without reloading the scene.</summary>
    public void ResetWorld()
    {
        Time.timeScale = 1f;
        SCR_Pool.DeactivateAllObject();

        for (int i = 0; i < turrets.Length; i++)
            turrets[i].ResetTurret();

        for (int i = 0; i < characters.Length; i++)
            characters[i].ResetToSpawn();

        GameEvent.OnWorldResetCompleted?.Invoke();
    }
}
