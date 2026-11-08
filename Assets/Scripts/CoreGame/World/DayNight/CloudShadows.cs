using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Drifting cloud shadows at a fixed world speed that does not depend on the day length. Feeds the cloud settings read
/// by the ToonLit and GrassBlade shaders and slides a cloud cookie across the sun so ordinary URP materials match.
/// Clouds follow game time, so they stop while the game is paused and never jump when the clock is paused or changed.
/// </summary>
public class CloudShadows : MonoBehaviour
{
    private static readonly int CloudNoiseId = Shader.PropertyToID("_CloudNoise");
    private static readonly int CloudScaleId = Shader.PropertyToID("_CloudScale");
    private static readonly int CloudWorldYId = Shader.PropertyToID("_CloudWorldY");
    private static readonly int CloudContrastId = Shader.PropertyToID("_CloudContrast");
    private static readonly int CloudThresholdId = Shader.PropertyToID("_CloudThreshold");
    private static readonly int CloudDirectionId = Shader.PropertyToID("_CloudDirection");
    private static readonly int CloudShadowMinId = Shader.PropertyToID("_CloudShadowMin");
    private static readonly int CloudDivergeAngleId = Shader.PropertyToID("_CloudDivergeAngle");
    private static readonly int CloudPowerId = Shader.PropertyToID("_CloudPower");
    private static readonly int CloudLightDirectionId = Shader.PropertyToID("_CloudLightDirection");
    private static readonly int CloudTravelId = Shader.PropertyToID("_CloudTravel");

    [Header("Drift")]
    [Min(0f)]
    [SerializeField] private float driftSpeed = 0.7f;
    [SerializeField] private Vector2 cloudDirection = new Vector2(1f, 0f);

    [Header("Shader Clouds")]
    [SerializeField] private Texture2D cloudNoise;
    [Min(0.01f)]
    [SerializeField] private float cloudScale = 60f;
    [SerializeField] private float cloudWorldY = 20f;
    [SerializeField] private float cloudContrast = 20f;
    [Range(-1f, 1f)]
    [SerializeField] private float cloudThreshold = 0.47f;
    [Range(0f, 1f)]
    [SerializeField] private float cloudShadowMin = 0.05f;
    [Range(0f, 45f)]
    [SerializeField] private float cloudDivergeAngle = 10f;
    [Range(1f, 5f)]
    [SerializeField] private float cloudPower = 3f;
    [SerializeField] private Vector3 cloudLightDirection = new Vector3(0.2f, -1f, 0.2f);

    [Header("Sun Cookie Clouds")]
    [SerializeField] private Light sunLight;
    [SerializeField] private UniversalAdditionalLightData sunLightData;
    [SerializeField] private Texture2D cloudCookie;
    [SerializeField] private Vector2 cookieSize = new Vector2(40f, 40f);

    private float travel;
    private Vector2 cookieOffset;

    /// <summary>World distance the clouds cover per second of game time.</summary>
    public float DriftSpeed => driftSpeed;

    /// <summary>Shader-space offset reached after moving a world distance at the given cloud scale.</summary>
    public static float WorldToShaderTravel(float worldDistance, float scale) => worldDistance / Mathf.Max(scale, 0.01f);

    private void Awake()
    {
        if (sunLight == null || sunLightData == null || cloudCookie == null) return;

        sunLight.cookie = cloudCookie;
        sunLightData.lightCookieSize = cookieSize;
    }

    private void OnEnable() => ApplySettings();

    private void OnValidate() => ApplySettings();

    private void Update()
    {
        float step = driftSpeed * Time.deltaTime;
        if (step <= 0f) return;

        travel += WorldToShaderTravel(step, cloudScale);
        Shader.SetGlobalFloat(CloudTravelId, travel);

        if (sunLightData == null) return;

        cookieOffset += cloudDirection.normalized * step;
        cookieOffset.x = Mathf.Repeat(cookieOffset.x, cookieSize.x);
        cookieOffset.y = Mathf.Repeat(cookieOffset.y, cookieSize.y);
        sunLightData.lightCookieOffset = cookieOffset;
    }

    private void ApplySettings()
    {
        Shader.SetGlobalTexture(CloudNoiseId, cloudNoise);
        Shader.SetGlobalFloat(CloudScaleId, cloudScale);
        Shader.SetGlobalFloat(CloudWorldYId, cloudWorldY);
        Shader.SetGlobalFloat(CloudContrastId, cloudContrast);
        Shader.SetGlobalFloat(CloudThresholdId, cloudThreshold);
        Shader.SetGlobalFloat(CloudShadowMinId, cloudShadowMin);
        Shader.SetGlobalFloat(CloudDivergeAngleId, cloudDivergeAngle);
        Shader.SetGlobalFloat(CloudPowerId, cloudPower);
        Shader.SetGlobalVector(CloudDirectionId, new Vector4(cloudDirection.x, cloudDirection.y, 0f, 0f));
        Shader.SetGlobalVector(CloudLightDirectionId, cloudLightDirection.normalized);
        Shader.SetGlobalFloat(CloudTravelId, travel);
    }
}
