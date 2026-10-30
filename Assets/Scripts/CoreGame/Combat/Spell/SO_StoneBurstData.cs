using UnityEngine;

[CreateAssetMenu(fileName = "SO_StoneBurstData", menuName = "Game/Spell/Stone Burst Data")]
public class SO_StoneBurstData : ScriptableObject
{
    public float damage = 35f;
    public float range = 4f;
    [Range(0f, 360f)] public float arc = 90f;
    public float windup = 0.35f;
    public float stunDuration = 1f;
    public bool breaksArmor = true;

    [Header("Visual Layout")]
    public int[] rowCounts = { 3, 5, 7 };
    public float[] rowDistances = { 0.35f, 0.68f, 1f };
    public float outwardTilt = 18f;
    [Range(0f, 1f)] public float exposedRatio = 0.7f;

    [Header("Visual Timing")]
    public float rowDelay = 0.06f;
    public float riseTime = 0.12f;
    public float holdTime = 0.5f;
    public float sinkTime = 0.3f;

    [Header("Visual Terrain")]
    public LayerMask groundLayer = 1 << 7;
    public float maxRise = 1f;
    public float maxDrop = 1.5f;
    public float probeRadius = 0.05f;
    public float clearanceRadius = 0.15f;
    public float clearanceHeight = 0.35f;
}
