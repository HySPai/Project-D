using System;
using UnityEngine;

[Serializable]
public class BloodBurstSettings
{
    [Header("Droplets (fall and leave decals)")]
    public Vector2Int dropletCountRange = new Vector2Int(2, 6);
    public Vector2 speedRange = new Vector2(2f, 5f);
    [Range(0f, 90f)] public float coneAngle = 30f;
    [Range(-1f, 1f)] public float upwardBias = 0.3f;
    public Vector2 dropletSizeRange = new Vector2(0.04f, 0.08f);
    public Vector2 dropletLifetimeRange = new Vector2(0.6f, 1.1f);

    [Header("Decals")]
    [Range(0f, 1f)] public float decalChance = 0.7f;
    public Vector2 decalSizeRange = new Vector2(0.06f, 0.18f);
}

[CreateAssetMenu(fileName = "HitBloodProfile", menuName = "Game/Combat/Hit Blood Profile")]
public class HitBloodProfile : ScriptableObject
{
    [Header("Bursts per Hit Tier")]
    [SerializeField] private BloodBurstSettings light = new BloodBurstSettings();
    [SerializeField] private BloodBurstSettings heavy = new BloodBurstSettings
    {
        decalSizeRange = new Vector2(0.08f, 0.24f)
    };
    [SerializeField] private BloodBurstSettings kill = new BloodBurstSettings
    {
        speedRange = new Vector2(2.5f, 6f),
        decalSizeRange = new Vector2(0.1f, 0.3f)
    };

    [Header("Droplets")]
    [SerializeField, ColorUsage(false)] private Color dropletColor = new Color(0.75f, 0.02f, 0.07f, 1f);
    [SerializeField, Range(0f, 3f)] private float gravityModifier = 1.4f;
    [SerializeField, Range(0f, 2f)] private float spawnHeight = 0.8f;
    [SerializeField, Range(0f, 1f)] private float spawnBackOffset = 0.2f;

    [Header("Decals")]
    [SerializeField] private Material decalMaterial;
    [SerializeField, Range(0f, 30f)] private float decalStayDuration = 6f;
    [SerializeField, Range(0.05f, 5f)] private float decalFadeDuration = 1.5f;
    [SerializeField, Range(0f, 1f)] private float decalOpacity = 1f;
    [SerializeField, Range(0.05f, 1f)] private float decalProjectionDepth = 0.4f;

    public Color DropletColor => dropletColor;
    public float GravityModifier => gravityModifier;
    public float SpawnHeight => spawnHeight;
    public float SpawnBackOffset => spawnBackOffset;

    public Material DecalMaterial => decalMaterial;
    public float DecalStayDuration => decalStayDuration;
    public float DecalFadeDuration => decalFadeDuration;
    public float DecalOpacity => decalOpacity;
    public float DecalProjectionDepth => decalProjectionDepth;

    /// <summary>Burst settings for a hit tier. Example: profile.GetBurst(HitFeelTier.Kill).dropletCountRange</summary>
    public BloodBurstSettings GetBurst(HitFeelTier tier)
    {
        switch (tier)
        {
            case HitFeelTier.Heavy: return heavy;
            case HitFeelTier.Kill: return kill;
            default: return light;
        }
    }
}
