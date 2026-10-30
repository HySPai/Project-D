using NUnit.Framework;

public class PoiseMeterTests
{
    [Test]
    public void Hit_BelowMax_DoesNotBreakAndLowersPoise()
    {
        var poise = new PoiseMeter(50f, 2f, 30f);

        bool broke = poise.Hit(25f, 0f);

        Assert.IsFalse(broke);
        Assert.AreEqual(25f, poise.GetCurrent(0f), 0.001f);
    }

    [Test]
    public void Hit_ReachingZero_BreaksAndRefills()
    {
        var poise = new PoiseMeter(50f, 2f, 30f);
        poise.Hit(25f, 0f);

        bool broke = poise.Hit(25f, 0.5f);

        Assert.IsTrue(broke);
        Assert.AreEqual(50f, poise.GetCurrent(0.5f), 0.001f);
    }

    [Test]
    public void GetCurrent_BeforeRecoverDelay_StaysLow()
    {
        var poise = new PoiseMeter(50f, 2f, 30f);
        poise.Hit(40f, 0f);

        Assert.AreEqual(10f, poise.GetCurrent(1.9f), 0.001f);
    }

    [Test]
    public void GetCurrent_AfterRecoverDelay_RefillsAtRate()
    {
        var poise = new PoiseMeter(50f, 2f, 30f);
        poise.Hit(40f, 0f);

        Assert.AreEqual(25f, poise.GetCurrent(2.5f), 0.001f);
    }

    [Test]
    public void GetCurrent_LongAfterHit_CapsAtMax()
    {
        var poise = new PoiseMeter(50f, 2f, 30f);
        poise.Hit(40f, 0f);

        Assert.AreEqual(50f, poise.GetCurrent(100f), 0.001f);
    }

    [Test]
    public void Hit_AfterPartialRecovery_UsesRecoveredValue()
    {
        var poise = new PoiseMeter(50f, 2f, 30f);
        poise.Hit(40f, 0f);

        bool broke = poise.Hit(20f, 2.5f);

        Assert.IsFalse(broke);
        Assert.AreEqual(5f, poise.GetCurrent(2.5f), 0.001f);
    }

    [Test]
    public void Hit_WithZeroMax_NeverBreaks()
    {
        var poise = new PoiseMeter(0f, 2f, 30f);

        bool broke = poise.Hit(1000f, 0f);

        Assert.IsFalse(poise.Enabled);
        Assert.IsFalse(broke);
    }

    [Test]
    public void Reset_AfterHit_RefillsToMax()
    {
        var poise = new PoiseMeter(50f, 2f, 30f);
        poise.Hit(40f, 0f);

        poise.Reset();

        Assert.AreEqual(50f, poise.GetCurrent(0f), 0.001f);
    }
}
