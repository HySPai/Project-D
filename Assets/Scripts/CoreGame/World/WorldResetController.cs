using UnityEngine;

public class WorldResetController : MonoBehaviour
{
    [SerializeField] private CharacterControllerBase[] characters;
    [SerializeField] private Turret[] turrets;

    /// <summary>Puts the world back to its resting state without reloading the scene; the player is left where they stand when includePlayer is false.</summary>
    public void ResetWorld(bool includePlayer = true)
    {
        Time.timeScale = 1f;
        SCR_Pool.DeactivateAllObject();

        for (int i = 0; i < turrets.Length; i++)
            turrets[i].ResetTurret();

        for (int i = 0; i < characters.Length; i++)
        {
            if (!includePlayer && characters[i] is PlayerController) continue;
            characters[i].ResetToSpawn();
        }

        GameEvent.OnWorldResetCompleted?.Invoke();
    }
}
