using UnityEngine;

[CreateAssetMenu(fileName = "SO_BurnData", menuName = "Game/Spell/Burn Data")]
public class SO_BurnData : ScriptableObject
{
    [Header("Damage")]
    public float duration = 4f;
    public float damagePerSecond = 5f;
    public float tickInterval = 1f;

    [Header("Flash")]
    [ColorUsage(false, true)] public Color flashColor = new Color(3f, 1.2f, 0.25f, 1f);
    public float flashDuration = 0.25f;

    public float DamagePerTick => damagePerSecond * tickInterval;
}
