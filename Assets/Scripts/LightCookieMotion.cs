using UnityEngine;

/// <summary>
/// Scrolls the two textures of the dappled light cookie along authored curves. At runtime it animates a private copy
/// of the material, handed to the cookie render texture when one is assigned, so the material asset is never dirtied.
/// </summary>
public class LightCookieMotion : MonoBehaviour
{
    private static readonly int Tex1StId = Shader.PropertyToID("_Tex1_ST");
    private static readonly int Tex2StId = Shader.PropertyToID("_Tex2_ST");

    [SerializeField] Material m_lightCookieMaterial;
    [SerializeField] CustomRenderTexture m_cookieTexture;

    [Header("Texture 1")]
    [SerializeField] Vector2 m_cycleDuration1UV = new Vector2(20f, 20f);
    [SerializeField] AnimationCurve m_movementPath1U;
    [SerializeField] AnimationCurve m_movementPath1V;
    [SerializeField] Vector2 m_movementMagnitude1UV = new Vector2(0.1f, 0.1f);
    [SerializeField] Vector2 m_movementTimeOffset1UV = new Vector2();
    [SerializeField] Vector2 m_tex1TilingUV = new Vector2(1f, 1f);
    [SerializeField] Vector2 m_tex1OffsetUV = new Vector2();

    [Header("Texture 2")]
    [SerializeField] Vector2 m_cycleDuration2UV = new Vector2(20f, 20f);
    [SerializeField] AnimationCurve m_movementPath2U;
    [SerializeField] AnimationCurve m_movementPath2V;
    [SerializeField] Vector2 m_movementMagnitude2UV = new Vector2(0.1f, 0.1f);
    [SerializeField] Vector2 m_movementTimeOffset2UV = new Vector2();
    [SerializeField] Vector2 m_tex2TilingUV = new Vector2(2f, 2f);
    [SerializeField] Vector2 m_tex2OffsetUV = new Vector2();

    private Material runtimeMaterial;

    private Material Target => runtimeMaterial != null ? runtimeMaterial : m_lightCookieMaterial;

    private void Awake()
    {
        if (m_lightCookieMaterial == null || m_cookieTexture == null) return;

        runtimeMaterial = new Material(m_lightCookieMaterial) { name = m_lightCookieMaterial.name + " (Runtime)" };
        m_cookieTexture.material = runtimeMaterial;
    }

    private void OnDestroy()
    {
        if (runtimeMaterial == null) return;

        if (m_cookieTexture != null && m_cookieTexture.material == runtimeMaterial)
            m_cookieTexture.material = m_lightCookieMaterial;

        Destroy(runtimeMaterial);
        runtimeMaterial = null;
    }

    private void Update() => UpdateMaterial(Time.time);

    private void UpdateMaterial(float time)
    {
        Material target = Target;
        if (target == null) return;

        target.SetVector(Tex1StId, Evaluate(time, m_cycleDuration1UV, m_movementPath1U, m_movementPath1V,
            m_movementMagnitude1UV, m_movementTimeOffset1UV, m_tex1TilingUV, m_tex1OffsetUV));

        target.SetVector(Tex2StId, Evaluate(time, m_cycleDuration2UV, m_movementPath2U, m_movementPath2V,
            m_movementMagnitude2UV, m_movementTimeOffset2UV, m_tex2TilingUV, m_tex2OffsetUV));
    }

    private static Vector4 Evaluate(
        float time, Vector2 cycleDuration, AnimationCurve pathU, AnimationCurve pathV,
        Vector2 magnitude, Vector2 timeOffset, Vector2 tiling, Vector2 offset)
    {
        float u = time % cycleDuration.x / cycleDuration.x;
        float v = time % cycleDuration.y / cycleDuration.y;

        float newU = pathU.Evaluate(u + timeOffset.x) * magnitude.x;
        float newV = pathV.Evaluate(v + timeOffset.y) * magnitude.y;

        return new Vector4(tiling.x, tiling.y, newU + offset.x, newV + offset.y);
    }

    private void OnValidate()
    {
        if (m_movementPath1U == null || m_movementPath1V == null || m_movementPath2U == null || m_movementPath2V == null) return;

        UpdateMaterial(Application.isPlaying ? Time.time : 0f);
    }
}
