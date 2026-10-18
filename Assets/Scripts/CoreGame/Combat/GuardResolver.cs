using UnityEngine;

public enum GuardOutcome
{
    None,
    Blocked,
    Parried,
    GuardBroken
}

public readonly struct GuardInput
{
    public readonly float points;
    public readonly bool unblockable;
    public readonly float staggerAmount;
    public readonly bool guardActive;
    public readonly bool inFrontArc;
    public readonly bool parryWindowOpen;
    public readonly bool canParry;
    public readonly float stamina;
    public readonly float blockedDamageRatio;
    public readonly float staminaPerBlockedPoint;
    public readonly float guardStunThreshold;

    public GuardInput(
        float points, bool unblockable, float staggerAmount,
        bool guardActive, bool inFrontArc, bool parryWindowOpen, bool canParry,
        float stamina, float blockedDamageRatio, float staminaPerBlockedPoint,
        float guardStunThreshold)
    {
        this.points = points;
        this.unblockable = unblockable;
        this.staggerAmount = staggerAmount;
        this.guardActive = guardActive;
        this.inFrontArc = inFrontArc;
        this.parryWindowOpen = parryWindowOpen;
        this.canParry = canParry;
        this.stamina = stamina;
        this.blockedDamageRatio = blockedDamageRatio;
        this.staminaPerBlockedPoint = staminaPerBlockedPoint;
        this.guardStunThreshold = guardStunThreshold;
    }
}

public readonly struct GuardVerdict
{
    public readonly GuardOutcome outcome;
    public readonly float remainingPoints;
    public readonly float staminaSpent;
    public readonly bool guardStunned;

    public GuardVerdict(GuardOutcome outcome, float remainingPoints, float staminaSpent, bool guardStunned)
    {
        this.outcome = outcome;
        this.remainingPoints = remainingPoints;
        this.staminaSpent = staminaSpent;
        this.guardStunned = guardStunned;
    }
}

public static class GuardResolver
{
    public static GuardVerdict Resolve(in GuardInput input)
    {
        if (!input.guardActive || input.unblockable || !input.inFrontArc || input.points <= 0f)
            return new GuardVerdict(GuardOutcome.None, input.points, 0f, false);

        if (input.parryWindowOpen && input.canParry)
            return new GuardVerdict(GuardOutcome.Parried, 0f, 0f, false);

        float ratio = Mathf.Clamp01(input.blockedDamageRatio);
        float blockable = input.points * ratio;
        float perPoint = Mathf.Max(0.0001f, input.staminaPerBlockedPoint);
        float cost = blockable * perPoint;

        if (input.stamina >= cost)
        {
            bool stunned = input.guardStunThreshold > 0f && input.staggerAmount >= input.guardStunThreshold;
            return new GuardVerdict(GuardOutcome.Blocked, input.points - blockable, cost, stunned);
        }

        float stamina = Mathf.Max(0f, input.stamina);
        float absorbed = stamina / perPoint;
        return new GuardVerdict(GuardOutcome.GuardBroken, input.points - absorbed, stamina, false);
    }

    public static bool IsInFrontArc(Vector3 position, Vector3 forward, Vector3 sourcePosition, float arcDegrees)
    {
        Vector3 toSource = sourcePosition - position;
        toSource.y = 0f;
        forward.y = 0f;

        if (toSource.sqrMagnitude < 0.0001f || forward.sqrMagnitude < 0.0001f)
            return true;

        return Vector3.Angle(forward, toSource) <= arcDegrees * 0.5f;
    }
}
