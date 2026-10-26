using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Drifting cloud shadows. Feeds the cloud settings read by the ToonLit shader, and slides a cloud cookie across the sun
/// so surfaces with ordinary URP materials get the same shadows; the shader-side speed is set by DayNightCycle.
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

    [Header("Shader Clouds")]
    [SerializeField] private Texture2D cloudNoise;
    [SerializeField] private float cloudScale = 60f;
    [SerializeField] private float cloudWorldY = 20f;
    [SerializeField] private float cloudSpeed = 10f;
    [SerializeField] private float cloudContrast = 20f;
    [Range(-1f, 1f)]
    [SerializeField] private float cloudThreshold = 0.47f;
    [SerializeField] private Vector2 cloudDirection = new Vector2(1f, 0f);
    [Range(0f, 1f)]
    [SerializeField] private float cloudShadowMin = 0.05f;
    [Range(0f, 45f)]
    [SerializeField] private float cloudDivergeAngle = 10f;
    [Range(1f, 5f)]
    [SerializeField] private float cloudPower = 3f;

    [Header("Sun Cookie Clouds")]
    [SerializeField] private Light sunLight;
    [SerializeField] private UniversalAdditionalLightData sunLightData;
    [SerializeField] private Texture2D cloudCookie;
    [SerializeField] private Vector2 cookieSize = new Vector2(40f, 40f);
    [SerializeField] private float cookieDriftSpeed = 0.6f;

    private Vector2 cookieOffset;

    public float CloudSpeed => cloudSpeed;

    private void Awake()
    {
        if (sunLight == null || cloudCookie == null) return;

        sunLight.cookie = cloudCookie;
        sunLightData.lightCookieSize = cookieSize;
    }

    private void Update()
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

        if (sunLightData == null) return;

        cookieOffset += cloudDirection.normalized * (cookieDriftSpeed * Time.deltaTime);
        cookieOffset.x = Mathf.Repeat(cookieOffset.x, cookieSize.x);
        cookieOffset.y = Mathf.Repeat(cookieOffset.y, cookieSize.y);
        sunLightData.lightCookieOffset = cookieOffset;
    }
}
