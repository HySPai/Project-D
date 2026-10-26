using Sirenix.OdinInspector;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    private static readonly int GlobalAmbientColorId = Shader.PropertyToID("_GlobalAmbientColor");
    private static readonly int CloudLightDirectionId = Shader.PropertyToID("_CloudLightDirection");
    private static readonly int CloudSpeedId = Shader.PropertyToID("_CloudSpeed");

    [Header("Clock")]
    [SerializeField] private float startHour = 9f;
    [SerializeField] private float dayDurationSeconds = 840f;
    [SerializeField] private float sunriseHour = 6f;
    [SerializeField] private float sunsetHour = 18f;

    [Header("Sun and Moon")]
    [SerializeField] private Light sunLight;
    [SerializeField] private Light moonLight;
    [SerializeField] private float orbitAxisY = 43.9f;
    [Range(0f, 80f)]
    [SerializeField] private float orbitTilt;
    [SerializeField] private float maxSunIntensity = 1.2f;
    [SerializeField] private float maxMoonIntensity = 0.3f;
    [SerializeField] private AnimationCurve lightIntensityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField] private Gradient sunColor;
    [SerializeField] private Gradient moonColor;
    [SerializeField] private Gradient ambientColor;

    [Header("Clouds")]
    [SerializeField] private CloudShadows cloudShadows;
    [SerializeField] private Vector3 cloudShadowDirection = new Vector3(0.2f, -1f, 0.2f);

    private float hour;
    private bool paused;

    public float Hour => hour;
    public float SunriseHour => sunriseHour;
    public float SunsetHour => sunsetHour;
    public bool IsPaused => paused;

    /// <summary>0 in full daylight, 1 once the sun has set; follows the sun intensity curve so dusk fades in smoothly.</summary>
    public float NightFactor { get; private set; }

    private void Awake() => SetHour(startHour);

    private void Update()
    {
        if (paused || dayDurationSeconds <= 0f) return;

        SetHour(hour + Time.deltaTime * DayNightMath.HoursPerDay / dayDurationSeconds);
    }

    public void SetPaused(bool value)
    {
        paused = value;
        ApplyCloudSpeed();
    }

    [Button]
    public void SetHour(float value)
    {
        hour = DayNightMath.Wrap(value);
        Apply();
    }

    private void Apply()
    {
        float sunAngle = DayNightMath.SunAngle(hour, sunriseHour, sunsetHour);
        Quaternion orbit = Quaternion.Euler(0f, orbitAxisY, orbitTilt);

        if (sunLight != null) sunLight.transform.rotation = orbit * Quaternion.AngleAxis(sunAngle, Vector3.right);
        if (moonLight != null) moonLight.transform.rotation = orbit * Quaternion.AngleAxis(sunAngle + 180f, Vector3.right);

        float sunDot = sunLight != null ? Mathf.Clamp01(Vector3.Dot(sunLight.transform.forward, Vector3.down)) : 0f;
        float intensity = lightIntensityCurve.Evaluate(sunDot);
        NightFactor = 1f - Mathf.Clamp01(intensity);

        if (sunLight != null)
        {
            sunLight.intensity = Mathf.Lerp(0f, maxSunIntensity, intensity);
            sunLight.color = sunColor.Evaluate(sunDot);
            sunLight.shadows = sunLight.intensity < 0.01f ? LightShadows.None : LightShadows.Soft;
        }

        if (moonLight != null)
        {
            float moonDot = Mathf.Clamp01(Vector3.Dot(moonLight.transform.forward, Vector3.down));
            moonLight.intensity = Mathf.Lerp(maxMoonIntensity, 0f, intensity);
            moonLight.color = moonColor.Evaluate(moonDot);
            moonLight.shadows = moonLight.intensity < 0.01f ? LightShadows.None : LightShadows.Soft;
        }

        Color ambient = ambientColor.Evaluate(sunDot);
        RenderSettings.ambientLight = ambient;
        Shader.SetGlobalColor(GlobalAmbientColorId, ambient);
        Shader.SetGlobalVector(CloudLightDirectionId, cloudShadowDirection.normalized);
        ApplyCloudSpeed();
    }

    private void ApplyCloudSpeed()
    {
        if (cloudShadows == null || dayDurationSeconds <= 0f) return;

        Shader.SetGlobalFloat(CloudSpeedId, paused ? 0f : cloudShadows.CloudSpeed / dayDurationSeconds);
    }
}
