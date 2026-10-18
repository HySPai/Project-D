using NUnit.Framework;
using UnityEngine;

public class GuardResolverTests
{
    private static GuardInput Input(
        float points = 25f,
        bool unblockable = false,
        float staggerAmount = 0f,
        bool guardActive = true,
        bool inFrontArc = true,
        bool parryWindowOpen = false,
        bool canParry = true,
        float stamina = 100f,
        float ratio = 1f,
        float perPoint = 0.8f,
        float stunThreshold = 1f)
    {
        return new GuardInput(
            points, unblockable, staggerAmount, guardActive, inFrontArc,
            parryWindowOpen, canParry, stamina, ratio, perPoint, stunThreshold);
    }

    [Test]
    public void Block_FullStamina_AbsorbsAllDamageAndCostsZeroPointEightPerPoint()
    {
        GuardInput input = Input(points: 25f, stamina: 100f);

        GuardVerdict verdict = GuardResolver.Resolve(in input);

        Assert.AreEqual(GuardOutcome.Blocked, verdict.outcome);
        Assert.AreEqual(0f, verdict.remainingPoints, 0.001f);
        Assert.AreEqual(20f, verdict.staminaSpent, 0.001f);
    }

    [Test]
    public void Block_StaminaTooLow_BreaksGuardAndLetsHalfDamagePass()
    {
        GuardInput input = Input(points: 25f, stamina: 10f);

        GuardVerdict verdict = GuardResolver.Resolve(in input);

        Assert.AreEqual(GuardOutcome.GuardBroken, verdict.outcome);
        Assert.AreEqual(12.5f, verdict.remainingPoints, 0.001f);
        Assert.AreEqual(10f, verdict.staminaSpent, 0.001f);
    }

    [Test]
    public void Block_ZeroStamina_BreaksGuardAndPassesFullDamage()
    {
        GuardInput input = Input(points: 25f, stamina: 0f);

        GuardVerdict verdict = GuardResolver.Resolve(in input);

        Assert.AreEqual(GuardOutcome.GuardBroken, verdict.outcome);
        Assert.AreEqual(25f, verdict.remainingPoints, 0.001f);
        Assert.AreEqual(0f, verdict.staminaSpent, 0.001f);
    }

    [Test]
    public void Block_AttackFromBehind_IsIgnored()
    {
        GuardInput input = Input(inFrontArc: false);

        GuardVerdict verdict = GuardResolver.Resolve(in input);

        Assert.AreEqual(GuardOutcome.None, verdict.outcome);
        Assert.AreEqual(25f, verdict.remainingPoints, 0.001f);
    }

    [Test]
    public void Block_UnblockableAttack_PiercesGuardAndParry()
    {
        GuardInput input = Input(unblockable: true, parryWindowOpen: true);

        GuardVerdict verdict = GuardResolver.Resolve(in input);

        Assert.AreEqual(GuardOutcome.None, verdict.outcome);
        Assert.AreEqual(25f, verdict.remainingPoints, 0.001f);
    }

    [Test]
    public void Block_GuardNotActive_IsIgnored()
    {
        GuardInput input = Input(guardActive: false);

        GuardVerdict verdict = GuardResolver.Resolve(in input);

        Assert.AreEqual(GuardOutcome.None, verdict.outcome);
    }

    [Test]
    public void Parry_InsideWindow_TakesNoDamageAndSpendsNoStamina()
    {
        GuardInput input = Input(parryWindowOpen: true);

        GuardVerdict verdict = GuardResolver.Resolve(in input);

        Assert.AreEqual(GuardOutcome.Parried, verdict.outcome);
        Assert.AreEqual(0f, verdict.remainingPoints, 0.001f);
        Assert.AreEqual(0f, verdict.staminaSpent, 0.001f);
    }

    [Test]
    public void Parry_WeaponCannotParry_FallsBackToBlock()
    {
        GuardInput input = Input(parryWindowOpen: true, canParry: false);

        GuardVerdict verdict = GuardResolver.Resolve(in input);

        Assert.AreEqual(GuardOutcome.Blocked, verdict.outcome);
    }

    [Test]
    public void Block_HammerStyleHalfRatio_PassesHalfDamageAndCostsOnePerPoint()
    {
        GuardInput input = Input(points: 40f, stamina: 100f, ratio: 0.5f, perPoint: 1f);

        GuardVerdict verdict = GuardResolver.Resolve(in input);

        Assert.AreEqual(GuardOutcome.Blocked, verdict.outcome);
        Assert.AreEqual(20f, verdict.remainingPoints, 0.001f);
        Assert.AreEqual(20f, verdict.staminaSpent, 0.001f);
    }

    [Test]
    public void Block_StaggeringHit_StunsGuard()
    {
        GuardInput input = Input(staggerAmount: 2f, stunThreshold: 1f);

        GuardVerdict verdict = GuardResolver.Resolve(in input);

        Assert.AreEqual(GuardOutcome.Blocked, verdict.outcome);
        Assert.IsTrue(verdict.guardStunned);
    }

    [Test]
    public void Block_NormalHit_DoesNotStunGuard()
    {
        GuardInput input = Input(staggerAmount: 0f, stunThreshold: 1f);

        GuardVerdict verdict = GuardResolver.Resolve(in input);

        Assert.IsFalse(verdict.guardStunned);
    }

    [Test]
    public void FrontArc_SourceInFront_IsInside()
    {
        bool inside = GuardResolver.IsInFrontArc(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 2f), 120f);

        Assert.IsTrue(inside);
    }

    [Test]
    public void FrontArc_SourceAtSixtyFiveDegrees_IsOutsideOneTwentyArc()
    {
        Vector3 source = Quaternion.Euler(0f, 65f, 0f) * Vector3.forward * 2f;

        bool inside = GuardResolver.IsInFrontArc(Vector3.zero, Vector3.forward, source, 120f);

        Assert.IsFalse(inside);
    }

    [Test]
    public void FrontArc_SourceBehind_IsOutside()
    {
        bool inside = GuardResolver.IsInFrontArc(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, -2f), 120f);

        Assert.IsFalse(inside);
    }
}
