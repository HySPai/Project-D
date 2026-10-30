using NUnit.Framework;

public class ElementCycleTests
{
    [Test]
    public void NextCastable_FromStoneForward_WrapsToWindAndSkipsNone()
    {
        Assert.AreEqual(ElementType.Wind, PlayerElementCaster.NextCastable(ElementType.Stone, 1));
    }

    [Test]
    public void NextCastable_FromWindBackward_WrapsToStone()
    {
        Assert.AreEqual(ElementType.Stone, PlayerElementCaster.NextCastable(ElementType.Wind, -1));
    }

    [Test]
    public void NextCastable_FromFireForward_IsWater()
    {
        Assert.AreEqual(ElementType.Water, PlayerElementCaster.NextCastable(ElementType.Fire, 1));
    }

    [Test]
    public void NextCastable_FromNoneForward_IsWind()
    {
        Assert.AreEqual(ElementType.Wind, PlayerElementCaster.NextCastable(ElementType.None, 1));
    }
}
