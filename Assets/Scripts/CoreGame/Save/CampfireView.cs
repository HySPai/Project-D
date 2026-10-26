using DG.Tweening;
using UnityEngine;

public class CampfireView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform flameRoot;
    [SerializeField] private Light fireLight;
    [SerializeField] private ParticleSystem sparks;

    [Header("Smoldering")]
    [SerializeField] private float smolderFlameScale = 0.14f;
    [SerializeField] private float smolderLightIntensity = 0.7f;
    [SerializeField] private float smolderLightRange = 2.5f;

    [Header("Burning")]
    [SerializeField] private float burnFlameScale = 1f;
    [SerializeField] private float burnLightIntensity = 6f;
    [SerializeField] private float burnLightRange = 10f;
    [SerializeField] private float sparkRate = 14f;

    [Header("Ignite")]
    [SerializeField] private float igniteDuration = 0.7f;
    [SerializeField] private Ease igniteEase = Ease.OutBack;

    [Header("Flicker")]
    [SerializeField] private float flickerSpeed = 7f;
    [SerializeField] private float flickerAmount = 0.18f;
    [SerializeField] private float flameSwayDegrees = 6f;

    private Tween igniteTween;
    private float burn;
    private float noiseSeed;

    private void Awake() => noiseSeed = Random.value * 100f;

    private void OnDestroy() => igniteTween?.Kill();

    private void Update() => Apply();

    /// <summary>Makes the fire blaze up, or settle back to glowing embers; without animate it jumps straight to the state.</summary>
    public void SetBurning(bool burning, bool animate)
    {
        igniteTween?.Kill();
        float target = burning ? 1f : 0f;

        if (!animate)
        {
            burn = target;
            Apply();
            return;
        }

        igniteTween = DOTween.To(() => burn, value => burn = value, target, igniteDuration).SetEase(igniteEase);
    }

    private void Apply()
    {
        float flicker = 1f + (Mathf.PerlinNoise(noiseSeed, Time.time * flickerSpeed) - 0.5f) * 2f * flickerAmount * burn;

        float scale = Mathf.LerpUnclamped(smolderFlameScale, burnFlameScale, burn) * flicker;
        flameRoot.localScale = new Vector3(scale, scale * (0.9f + 0.2f * flicker), scale);
        flameRoot.localRotation = Quaternion.Euler(0f, 0f, (Mathf.PerlinNoise(noiseSeed + 7f, Time.time * flickerSpeed * 0.6f) - 0.5f) * 2f * flameSwayDegrees * burn);

        fireLight.intensity = Mathf.LerpUnclamped(smolderLightIntensity, burnLightIntensity, burn) * flicker;
        fireLight.range = Mathf.LerpUnclamped(smolderLightRange, burnLightRange, burn);

        ParticleSystem.EmissionModule emission = sparks.emission;
        emission.rateOverTime = Mathf.Max(0f, sparkRate * (burn - 0.3f) / 0.7f);
    }
}
