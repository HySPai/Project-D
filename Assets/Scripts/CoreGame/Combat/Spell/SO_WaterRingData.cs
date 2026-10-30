using UnityEngine;

[CreateAssetMenu(fileName = "SO_WaterRingData", menuName = "Game/Spell/Water Ring Data")]
public class SO_WaterRingData : ScriptableObject
{
    public float damage = 20f;
    public float radius = 3f;
    public float pushDistance = 2f;

    [Header("Visual Shape")]
    public float startRadius = 0.4f;
    [Range(0f, 1f)] public float innerRadiusRatio = 0.6f;
    public float crestHeight = 0.5f;
    [Range(0f, 1f)] public float innerCrestRatio = 0.6f;
    public float groundOffset = 0.1f;

    [Header("Visual Timing")]
    public float spreadTime = 0.4f;
    public float innerDelay = 0.12f;
    [Range(0f, 1f)] public float growRatio = 0.3f;
    public float shrinkTime = 0.15f;
}
