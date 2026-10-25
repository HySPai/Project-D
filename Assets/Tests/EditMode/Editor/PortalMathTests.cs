using NUnit.Framework;
using UnityEngine;

public class PortalMathTests
{
    private static readonly Vector3 SourcePos = new Vector3(-8f, 2f, 5f);
    private static readonly Quaternion SourceRot = Quaternion.Euler(0f, 270f, 0f);
    private static readonly Vector3 TargetPos = new Vector3(-47f, 2f, 5f);
    private static readonly Quaternion TargetRot = Quaternion.Euler(0f, 90f, 0f);

    [Test]
    public void IsPastThreshold_InsideRoomSide_ReturnsFalse()
    {
        Vector3 player = new Vector3(-7.6f, 1f, 5f);

        bool past = PortalMath.IsPastThreshold(SourcePos, SourceRot, player);

        Assert.IsFalse(past);
    }

    [Test]
    public void IsPastThreshold_BeyondMidline_ReturnsTrue()
    {
        Vector3 player = new Vector3(-8.2f, 1f, 5f);

        bool past = PortalMath.IsPastThreshold(SourcePos, SourceRot, player);

        Assert.IsTrue(past);
    }

    [Test]
    public void MapPosition_StepPastSource_LandsSameStepBeforeTarget()
    {
        Vector3 player = new Vector3(-8.4f, 1f, 5f);

        Vector3 mapped = PortalMath.MapPosition(SourcePos, SourceRot, TargetPos, TargetRot, player);

        AssertVector(new Vector3(-47.4f, 1f, 5f), mapped);
    }

    [Test]
    public void MapPosition_SideOffset_MirrorsAcrossDoorway()
    {
        Vector3 player = new Vector3(-8.1f, 1f, 6f);

        Vector3 mapped = PortalMath.MapPosition(SourcePos, SourceRot, TargetPos, TargetRot, player);

        AssertVector(new Vector3(-47.1f, 1f, 6f), mapped);
    }

    [Test]
    public void MapPosition_ArrivalPoint_IsNotPastTargetThreshold()
    {
        Vector3 player = new Vector3(-8.4f, 1f, 5f);

        Vector3 mapped = PortalMath.MapPosition(SourcePos, SourceRot, TargetPos, TargetRot, player);

        Assert.IsFalse(PortalMath.IsPastThreshold(TargetPos, TargetRot, mapped));
    }

    [Test]
    public void MapRotation_WalkingOutOfSource_KeepsWorldHeadingIntoTarget()
    {
        Quaternion facing = Quaternion.Euler(0f, 270f, 0f);

        Quaternion mapped = PortalMath.MapRotation(SourceRot, TargetRot, facing);

        AssertVector(Vector3.left, mapped * Vector3.forward);
    }

    private static void AssertVector(Vector3 expected, Vector3 actual)
    {
        Assert.AreEqual(expected.x, actual.x, 0.0001f);
        Assert.AreEqual(expected.y, actual.y, 0.0001f);
        Assert.AreEqual(expected.z, actual.z, 0.0001f);
    }
}
