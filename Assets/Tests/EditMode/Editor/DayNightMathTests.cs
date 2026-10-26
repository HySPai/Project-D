using NUnit.Framework;

public class DayNightMathTests
{
    private const float Sunrise = 6f;
    private const float Sunset = 18f;
    private const float Wake = 6f;

    [TestCase(6f, 0f)]
    [TestCase(12f, 90f)]
    [TestCase(18f, 180f)]
    [TestCase(0f, 270f)]
    [TestCase(21f, 225f)]
    [TestCase(3f, 315f)]
    public void SunAngle_AtKeyHours_FollowsTheArc(float hour, float expected)
    {
        float angle = DayNightMath.SunAngle(hour, Sunrise, Sunset);

        Assert.AreEqual(expected, angle, 0.001f);
    }

    [Test]
    public void SunAngle_HourBeyondOneDay_WrapsAround()
    {
        float wrapped = DayNightMath.SunAngle(36f, Sunrise, Sunset);

        Assert.AreEqual(DayNightMath.SunAngle(12f, Sunrise, Sunset), wrapped, 0.001f);
    }

    [Test]
    public void SunAngle_NegativeHour_WrapsToPreviousDay()
    {
        float wrapped = DayNightMath.SunAngle(-6f, Sunrise, Sunset);

        Assert.AreEqual(DayNightMath.SunAngle(18f, Sunrise, Sunset), wrapped, 0.001f);
    }

    [Test]
    public void PlanRest_EveningStart_DarkensUntilMidnightThenBrightensUntilWake()
    {
        DayNightMath.PlanRest(20f, Wake, out float darken, out float brighten);

        Assert.AreEqual(4f, darken, 0.001f);
        Assert.AreEqual(6f, brighten, 0.001f);
    }

    [Test]
    public void PlanRest_NoonStart_SpendsTwelveHoursDarkening()
    {
        DayNightMath.PlanRest(12f, Wake, out float darken, out float brighten);

        Assert.AreEqual(12f, darken, 0.001f);
        Assert.AreEqual(6f, brighten, 0.001f);
    }

    [Test]
    public void PlanRest_AfterMidnight_SkipsDarkeningAndBrightensToWake()
    {
        DayNightMath.PlanRest(2f, Wake, out float darken, out float brighten);

        Assert.AreEqual(0f, darken, 0.001f);
        Assert.AreEqual(4f, brighten, 0.001f);
    }

    [Test]
    public void PlanRest_ExactlyAtWakeHour_SleepsThroughToNextWake()
    {
        DayNightMath.PlanRest(6f, Wake, out float darken, out float brighten);

        Assert.AreEqual(18f, darken, 0.001f);
        Assert.AreEqual(6f, brighten, 0.001f);
    }

    [Test]
    public void Smooth_EndsAreExactAndMiddleIsHalf()
    {
        Assert.AreEqual(0f, DayNightMath.Smooth(0f), 0.0001f);
        Assert.AreEqual(0.5f, DayNightMath.Smooth(0.5f), 0.0001f);
        Assert.AreEqual(1f, DayNightMath.Smooth(1f), 0.0001f);
    }

    [TestCase(6f, "06:00")]
    [TestCase(6.5f, "06:30")]
    [TestCase(0f, "00:00")]
    [TestCase(25.25f, "01:15")]
    public void Format_ShowsClockTime(float hour, string expected)
    {
        Assert.AreEqual(expected, DayNightMath.Format(hour));
    }
}
