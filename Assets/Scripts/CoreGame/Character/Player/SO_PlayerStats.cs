using UnityEngine;

[CreateAssetMenu(fileName = "SO_PlayerStats", menuName = "Game/Stats/Player Stats")]
public class SO_PlayerStats : CharacterStats
{
    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaRegenRate = 25f;
    public float staminaRegenDelay = 0.5f;

    [Header("Stamina Cost")]
    public float rollStaminaCost = 15f;
    public float runStaminaDrainRate = 10f;
    public float runResumeStamina = 20f;
}