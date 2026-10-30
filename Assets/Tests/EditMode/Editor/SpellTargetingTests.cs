using NUnit.Framework;
using UnityEngine;

public class SpellTargetingTests
{
    [Test]
    public void IsWithinArc_PointStraightAhead_IsInside()
    {
        Assert.IsTrue(SpellTargeting.IsWithinArc(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 3.5f), 90f));
    }

    [Test]
    public void IsWithinArc_PointAt40Degrees_IsInsideNinetyDegreeArc()
    {
        Vector3 point = Quaternion.Euler(0f, 40f, 0f) * Vector3.forward * 3f;

        Assert.IsTrue(SpellTargeting.IsWithinArc(Vector3.zero, Vector3.forward, point, 90f));
    }

    [Test]
    public void IsWithinArc_PointAt60Degrees_IsOutsideNinetyDegreeArc()
    {
        Vector3 point = Quaternion.Euler(0f, 60f, 0f) * Vector3.forward * 3f;

        Assert.IsFalse(SpellTargeting.IsWithinArc(Vector3.zero, Vector3.forward, point, 90f));
    }

    [Test]
    public void IsWithinArc_PointBehind_IsOutside()
    {
        Assert.IsFalse(SpellTargeting.IsWithinArc(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, -2f), 90f));
    }

    [Test]
    public void IsWithinArc_HeightDifference_IsIgnored()
    {
        Assert.IsTrue(SpellTargeting.IsWithinArc(Vector3.zero, Vector3.forward, new Vector3(0f, 5f, 1f), 90f));
    }

    [Test]
    public void TickCount_ThreeSecondsAtOnePerSecond_IsThree()
    {
        var data = ScriptableObject.CreateInstance<SO_WindVortexData>();
        data.lifetime = 3f;
        data.tickInterval = 1f;

        int ticks = data.TickCount;
        Object.DestroyImmediate(data);

        Assert.AreEqual(3, ticks);
    }
}
