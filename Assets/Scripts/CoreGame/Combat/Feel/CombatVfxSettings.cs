using UnityEngine;

[CreateAssetMenu(fileName = "CombatVfxSettings", menuName = "Game/Combat/Combat Vfx Settings")]
public class CombatVfxSettings : ScriptableObject
{
    [Header("Pools")]
    [SerializeField, Range(8, 256)] private int decalPoolSize = 64;
    [SerializeField, Range(50, 2000)] private int maxDroplets = 600;

    [Header("Droplet Collision")]
    [SerializeField] private LayerMask dropletCollisionLayers = ~0;
    [SerializeField, Range(0f, 0.2f)] private float decalSurfaceOffset = 0.02f;
    [SerializeField, Range(-1f, 1f)] private float minDecalNormalY = 0.6f;

    [Header("Streak Pool")]
    [SerializeField, Range(50, 3000)] private int maxStreaks = 800;
    [SerializeField, Range(8, 400)] private int maxSplashes = 120;
    [SerializeField, Range(0f, 6f)] private float splashCameraOffset = 2.5f;

    public int DecalPoolSize => decalPoolSize;
    public int MaxDroplets => maxDroplets;
    public LayerMask DropletCollisionLayers => dropletCollisionLayers;
    public float DecalSurfaceOffset => decalSurfaceOffset;
    public float MinDecalNormalY => minDecalNormalY;
    public int MaxStreaks => maxStreaks;
    public int MaxSplashes => maxSplashes;
    public float SplashCameraOffset => splashCameraOffset;
}
