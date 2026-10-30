using NUnit.Framework;

public class GameTimeTests
{
    [Test]
    public void Resolve_Paused_IsZeroEvenDuringSlowMotion()
    {
        Assert.AreEqual(0f, GameTime.Resolve(true, 0.3f));
    }

    [Test]
    public void Resolve_NotPaused_UsesSlowMotionScale()
    {
        Assert.AreEqual(0.3f, GameTime.Resolve(false, 0.3f), 1e-6f);
    }

    [Test]
    public void Resolve_ScaleAboveOne_IsClampedToNormalSpeed()
    {
        Assert.AreEqual(1f, GameTime.Resolve(false, 2f));
    }
}
