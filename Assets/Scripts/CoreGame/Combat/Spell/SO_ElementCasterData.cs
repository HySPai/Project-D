using UnityEngine;

[CreateAssetMenu(fileName = "SO_ElementCasterData", menuName = "Game/Spell/Element Caster Data")]
public class SO_ElementCasterData : ScriptableObject
{
    [Header("Mana")]
    public float maxMana = 100f;
    public float chargePerDamage = 0.24f;
    public float minChargePerHit = 1f;
    public float skillCost = 33f;

    [Header("Cooldown")]
    public float windCooldown = 2f;
    public float fireCooldown = 1f;
    public float waterCooldown = 1.5f;
    public float stoneCooldown = 1.5f;

    /// <summary>Mana gained from one weapon hit, rounded down with a floor of minChargePerHit.</summary>
    public float GetCharge(float damage)
    {
        if (damage <= 0f) return 0f;
        return Mathf.Max(minChargePerHit, Mathf.Floor(damage * chargePerDamage));
    }

    /// <summary>Seconds a skill of this element waits before it can be cast again.</summary>
    public float GetCooldown(ElementType element)
    {
        switch (element)
        {
            case ElementType.Wind: return windCooldown;
            case ElementType.Fire: return fireCooldown;
            case ElementType.Water: return waterCooldown;
            case ElementType.Stone: return stoneCooldown;
            default: return 0f;
        }
    }
}
