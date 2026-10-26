using UnityEngine;

/// <summary>Pure clock and sun-arc rules for the day-night cycle, kept free of scene state so they can be tested.</summary>
public static class DayNightMath
{
    public const float HoursPerDay = 24f;

    public static float Wrap(float hour)
    {
        hour %= HoursPerDay;
        return hour < 0f ? hour + HoursPerDay : hour;
    }

    public static bool IsDay(float hour, float sunriseHour, float sunsetHour)
    {
        hour = Wrap(hour);
        return hour >= sunriseHour && hour < sunsetHour;
    }

    /// <summary>Sun elevation along its arc: 0 at sunrise, 90 at midday, 180 at sunset, 270 at midnight.</summary>
    public static float SunAngle(float hour, float sunriseHour, float sunsetHour)
    {
        hour = Wrap(hour);

        if (IsDay(hour, sunriseHour, sunsetHour))
        {
            float dayLength = sunsetHour - sunriseHour;
            return Mathf.Lerp(0f, 180f, (hour - sunriseHour) / dayLength);
        }

        float nightLength = HoursPerDay - sunsetHour + sunriseHour;
        float sinceSunset = hour >= sunsetHour ? hour - sunsetHour : HoursPerDay - sunsetHour + hour;
        return Mathf.Lerp(180f, 360f, sinceSunset / nightLength);
    }

    /// <summary>
    /// Splits a rest into the hours that darken the screen up to midnight and the hours that brighten it up to wakeHour.
    /// Resting after midnight skips the darkening part, because midnight is already behind.
    /// </summary>
    public static void PlanRest(float hour, float wakeHour, out float darkenHours, out float brightenHours)
    {
        hour = Wrap(hour);

        if (hour < wakeHour)
        {
            darkenHours = 0f;
            brightenHours = wakeHour - hour;
            return;
        }

        darkenHours = HoursPerDay - hour;
        brightenHours = wakeHour;
    }

    public static float Smooth(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }

    /// <summary>Hours shown as a clock string, e.g. 6.5 becomes 06:30.</summary>
    public static string Format(float hour)
    {
        hour = Wrap(hour);
        int h = Mathf.FloorToInt(hour);
        int m = Mathf.FloorToInt((hour - h) * 60f);
        return $"{h:00}:{m:00}";
    }
}
