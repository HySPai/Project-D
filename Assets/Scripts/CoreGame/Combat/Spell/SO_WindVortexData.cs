using UnityEngine;

[CreateAssetMenu(fileName = "SO_WindVortexData", menuName = "Game/Spell/Wind Vortex Data")]
public class SO_WindVortexData : ScriptableObject
{
    [Header("Damage")]
    public float damagePerTick = 8f;
    public float tickInterval = 1f;

    [Header("Shape")]
    public float radius = 2f;
    public float castDistance = 3f;
    public float lifetime = 3f;

    [Header("Pull")]
    public float pullSpeed = 2f;
    public float holdRefresh = 0.15f;

    /// <summary>Number of damage ticks over the lifetime, e.g. 3 ticks of 8 for a 3s vortex.</summary>
    public int TickCount => tickInterval > 0f ? Mathf.FloorToInt(lifetime / tickInterval + 0.0001f) : 0;
}
