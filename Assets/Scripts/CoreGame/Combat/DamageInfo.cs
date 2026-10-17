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

    public DamageInfo(
        float points,
        CharacterControllerBase attacker,
        Vector3 sourcePosition,
        ElementType element,
        DamageSource source,
        bool unblockable,
        float staggerAmount)
    {
        this.points = points;
        this.attacker = attacker;
        this.sourcePosition = sourcePosition;
        this.element = element;
        this.source = source;
        this.unblockable = unblockable;
        this.staggerAmount = staggerAmount;
    }
}
