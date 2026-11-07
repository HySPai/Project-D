using NUnit.Framework;

public class WadeSpeedTests
{
    private const float FullSlowDepth = 0.6f;
    private const float MinMultiplier = 0.45f;

    [Test]
    public void DryGround_KeepsFullSpeed()
    {
        float result = PlayerState.GetWadeSpeedMultiplier(0f, FullSlowDepth, MinMultiplier);

        Assert.AreEqual(1f, result);
    }

    [Test]
    public void HalfFullSlowDepth_IsHalfwayToMinimum()
    {
        float result = PlayerState.GetWadeSpeedMultiplier(FullSlowDepth * 0.5f, FullSlowDepth, MinMultiplier);

        Assert.AreEqual((1f + MinMultiplier) * 0.5f, result, 0.0001f);
    }

    [Test]
    public void DeeperThanFullSlowDepth_ClampsToMinimum()
    {
        float result = PlayerState.GetWadeSpeedMultiplier(FullSlowDepth * 3f, FullSlowDepth, MinMultiplier);

        Assert.AreEqual(MinMultiplier, result, 0.0001f);
    }

    [Test]
    public void DeeperWater_IsNeverFaster()
    {
        float shallow = PlayerState.GetWadeSpeedMultiplier(0.15f, FullSlowDepth, MinMultiplier);
        float deep = PlayerState.GetWadeSpeedMultiplier(0.4f, FullSlowDepth, MinMultiplier);

        Assert.Less(deep, shallow);
    }

    [Test]
    public void ZeroFullSlowDepth_InWater_UsesMinimum()
    {
        float result = PlayerState.GetWadeSpeedMultiplier(0.1f, 0f, MinMultiplier);

        Assert.AreEqual(MinMultiplier, result, 0.0001f);
    }
}
