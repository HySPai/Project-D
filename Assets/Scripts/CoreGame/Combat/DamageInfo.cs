using UnityEngine;

public readonly struct DamageInfo
{
    public readonly float points;
    public readonly CharacterControllerBase attacker;
    public readonly Vector3 sourcePosition;
    public readonly ElementType element;
    public readonly DamageSource source;
    public readonly bool unblockable;
    public readonly float staggerAmount;
    public readonly float knockbackScale;
    public readonly float stunScale;

    public DamageInfo(
        float points,
        CharacterControllerBase attacker,
        Vector3 sourcePosition,
        ElementType element,
        DamageSource source,
        bool unblockable,
        float staggerAmount,
        float knockbackScale = 1f,
        float stunScale = 1f)
    {
        this.knockbackScale = knockbackScale;
        this.stunScale = stunScale;
        this.points = points;
        this.attacker = attacker;
        this.sourcePosition = sourcePosition;
        this.element = element;
        this.source = source;
        this.unblockable = unblockable;
        this.staggerAmount = staggerAmount;
    }

    public DamageInfo WithPoints(float newPoints)
    {
        return new DamageInfo(
            newPoints, attacker, sourcePosition, element, source,
            unblockable, staggerAmount, knockbackScale, stunScale);
    }
}
