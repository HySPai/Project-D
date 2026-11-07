using UnityEngine;

[CreateAssetMenu(fileName = "SO_PlayerStats", menuName = "Game/Stats/Player Stats")]
public class SO_PlayerStats : CharacterStats
{
    [Header("Hearts")]
    public int maxHearts = 4;
    public float pointsPerHeart = 25f;
    public float invulnerabilityDuration = 1f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaRegenRate = 25f;
    public float staminaRegenDelay = 0.5f;

    [Header("Stamina Cost")]
    public float rollStaminaCost = 15f;
    public float backstepStaminaCost = 10f;
    public float runStaminaDrainRate = 10f;
    public float runResumeStamina = 20f;

    [Header("Locomotion")]
    [Range(0f, 0.5f)] public float moveInputDeadzone = 0f;
    public float moveAnimSpeedThreshold = 0.15f;

    [Header("Roll")]
    [Range(0f, 1f)] public float rollCancelNormalizedTime = 0.55f;
    public float rollCancelBlendTime = 0.2f;
    public float rollExitSpeedRatio = 1f;
    public float rollSteerSpeed = 180f;
}