using NUnit.Framework;
using UnityEngine;

public class CameraZoneMathTests
{
    private static readonly Vector3 BoxCenter = Vector3.zero;
    private static readonly Vector3 BoxSize = new Vector3(10f, 4f, 6f);

    [Test]
    public void SoftLimit_InsideKnee_ReturnsSameValue()
    {
        float result = CameraZoneMath.SoftLimit(1.5f, 5f, 3f);

        Assert.AreEqual(1.5f, result);
    }

    [Test]
    public void SoftLimit_FarOutside_NeverExceedsHalfExtent()
    {
        float result = CameraZoneMath.SoftLimit(100f, 5f, 3f);

        Assert.LessOrEqual(result, 5f);
        Assert.Greater(result, 4.99f);
    }

    [Test]
    public void SoftLimit_JustPastKnee_MovesAlmostOneToOne()
    {
        float atKnee = CameraZoneMath.SoftLimit(2f, 5f, 3f);
        float past = CameraZoneMath.SoftLimit(2.01f, 5f, 3f);

        Assert.AreEqual(0.01f, past - atKnee, 0.0005f);
    }

    [Test]
    public void SoftLimit_NegativeSide_MirrorsPositiveSide()
    {
        float positive = CameraZoneMath.SoftLimit(7f, 5f, 3f);
        float negative = CameraZoneMath.SoftLimit(-7f, 5f, 3f);

        Assert.AreEqual(-positive, negative);
    }

    [Test]
    public void SoftLimit_ZeroSoftness_HardClamps()
    {
        float result = CameraZoneMath.SoftLimit(7f, 5f, 0f);

        Assert.AreEqual(5f, result);
    }

    [Test]
    public void SoftClampXZ_PointOutside_KeepsHeightAndStaysInside()
    {
        Vector3 point = new Vector3(20f, 7f, -9f);

        Vector3 result = CameraZoneMath.SoftClampXZ(Matrix4x4.identity, Matrix4x4.identity, BoxCenter, BoxSize, 2f, point);

        Assert.AreEqual(7f, result.y);
        Assert.Less(result.x, 5f);
        Assert.Greater(result.z, -3f);
    }

    [Test]
    public void SoftClampXZ_RotatedBox_LimitsAlongLocalAxes()
    {
        Matrix4x4 localToWorld = Matrix4x4.TRS(new Vector3(100f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
        Matrix4x4 worldToLocal = localToWorld.inverse;
        Vector3 point = new Vector3(100f, 0f, 50f);

        Vector3 result = CameraZoneMath.SoftClampXZ(worldToLocal, localToWorld, BoxCenter, BoxSize, 0f, point);

        Assert.AreEqual(100f, result.x, 0.0001f);
        Assert.AreEqual(5f, result.z, 0.0001f);
    }

    [Test]
    public void Contains_PointAboveBoxHeight_ReturnsFalse()
    {
        Vector3 point = new Vector3(0f, 3f, 0f);

        bool inside = CameraZoneMath.Contains(Matrix4x4.identity, BoxCenter, BoxSize, point);

        Assert.IsFalse(inside);
    }

    [Test]
    public void Contains_PointInsideBox_ReturnsTrue()
    {
        Vector3 point = new Vector3(4.9f, 1.9f, -2.9f);

        bool inside = CameraZoneMath.Contains(Matrix4x4.identity, BoxCenter, BoxSize, point);

        Assert.IsTrue(inside);
    }

    [Test]
    public void RampWeight_AtStart_ReturnsZero()
    {
        float weight = CameraZoneMath.RampWeight(new Vector3(0f, 0f, 4f), new Vector3(0f, 0f, 4f), new Vector3(0f, 0f, -4f));

        Assert.AreEqual(0f, weight);
    }

    [Test]
    public void RampWeight_PastEnd_ReturnsOne()
    {
        float weight = CameraZoneMath.RampWeight(new Vector3(3f, 0f, -9f), new Vector3(0f, 0f, 4f), new Vector3(0f, 0f, -4f));

        Assert.AreEqual(1f, weight);
    }

    [Test]
    public void RampWeight_Midpoint_ReturnsHalf()
    {
        float weight = CameraZoneMath.RampWeight(Vector3.zero, new Vector3(0f, 0f, 4f), new Vector3(0f, 0f, -4f));

        Assert.AreEqual(0.5f, weight, 0.0001f);
    }

    [Test]
    public void RampWeight_CloserToEnd_GrowsMonotonically()
    {
        Vector3 start = new Vector3(0f, 0f, 4f);
        Vector3 end = new Vector3(0f, 0f, -4f);

        float near = CameraZoneMath.RampWeight(new Vector3(0f, 0f, 2f), start, end);
        float far = CameraZoneMath.RampWeight(new Vector3(0f, 0f, -2f), start, end);

        Assert.Less(near, far);
    }

    [Test]
    public void Damp_ZeroSharpness_SnapsToTarget()
    {
        float result = CameraZoneMath.Damp(0f, 10f, 0f, 0.016f);

        Assert.AreEqual(10f, result);
    }

    [Test]
    public void Damp_SameTotalTimeDifferentSteps_ReachesSameValue()
    {
        float oneStep = CameraZoneMath.Damp(0f, 10f, 4f, 0.5f);
        float twoSteps = CameraZoneMath.Damp(CameraZoneMath.Damp(0f, 10f, 4f, 0.25f), 10f, 4f, 0.25f);

        Assert.AreEqual(oneStep, twoSteps, 0.0001f);
    }
}
