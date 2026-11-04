using UnityEngine;
using UnityEngine.Rendering;

public class TimeOfDayGrade : MonoBehaviour
{
    [SerializeField] private DayNightCycle dayNight;

    [Header("Grade")]
    [SerializeField] private Volume morningVolume;
    [SerializeField] private Volume afternoonVolume;
    [SerializeField] private float afternoonBlendStartHour = 11f;
    [SerializeField] private float afternoonBlendEndHour = 15f;

    [Header("Fog")]
    [SerializeField] private Gradient fogColor;
    [SerializeField] private float fogDensity = 0.006f;

    private void Start()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = fogDensity;
    }

    private void LateUpdate()
    {
        float day = 1f - dayNight.NightFactor;
        float afternoon = DayNightMath.Smooth(Mathf.InverseLerp(afternoonBlendStartHour, afternoonBlendEndHour, dayNight.Hour));

        morningVolume.weight = day * (1f - afternoon);
        afternoonVolume.weight = day * afternoon;

        RenderSettings.fogColor = fogColor.Evaluate(dayNight.Hour / DayNightMath.HoursPerDay);
    }
}
