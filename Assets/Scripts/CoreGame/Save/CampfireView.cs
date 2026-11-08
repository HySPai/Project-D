using DG.Tweening;
using UnityEngine;

/// <summary>Rest point fire: smoulders while unlit and blazes with flicker, sway and sparks once lit.</summary>
public class CampfireView : MonoBehaviour
{
    private const float SeedRange = 100f;
    private const float SwayNoiseOffset = 7f;

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
    [SerializeField, Range(0f, 0.99f)] private float sparkStartBurn = 0.3f;

    [Header("Ignite")]
    [SerializeField] private float igniteDuration = 0.7f;
    [SerializeField] private Ease igniteEase = Ease.OutBack;

    [Header("Flicker")]
    [SerializeField] private float flickerSpeed = 7f;
    [SerializeField] private float flickerAmount = 0.18f;
    [SerializeField] private float flameSwayDegrees = 6f;
    [SerializeField] private float swaySpeedRatio = 0.6f;
    [SerializeField] private float flameStretchBase = 0.9f;
    [SerializeField] private float flameStretchFlicker = 0.2f;

    private Tween igniteTween;
    private float burn;
    private float noiseSeed;
    private bool restingApplied;

    private void Awake() => noiseSeed = Random.value * SeedRange;

    private void Start() => ApplyBurnLevel();

    private void OnDestroy() => igniteTween?.Kill();

    private void Update()
    {
        if (burn == 0f)
        {
            if (restingApplied) return;
            restingApplied = true;
        }
        else
        {
            restingApplied = false;
        }

        ApplyFlicker();
    }

    /// <summary>Makes the fire blaze up, or settle back to glowing embers; without animate it jumps straight to the state.</summary>
    public void SetBurning(bool burning, bool animate)
    {
        igniteTween?.Kill();
        float target = burning ? 1f : 0f;

        if (!animate)
        {
            SetBurn(target);
            ApplyFlicker();
            return;
        }

        igniteTween = DOTween.To(() => burn, SetBurn, target, igniteDuration)
            .SetEase(igniteEase)
            .SetLink(gameObject);
    }

    private void SetBurn(float value)
    {
        burn = value;
        restingApplied = false;
        ApplyBurnLevel();
    }

    private void ApplyBurnLevel()
    {
        fireLight.range = Mathf.LerpUnclamped(smolderLightRange, burnLightRange, burn);

        ParticleSystem.EmissionModule emission = sparks.emission;
        emission.rateOverTime = Mathf.Max(0f, sparkRate * (burn - sparkStartBurn) / (1f - sparkStartBurn));
    }

    private void ApplyFlicker()
    {
        float time = Time.time;
        float flicker = 1f + (Mathf.PerlinNoise(noiseSeed, time * flickerSpeed) - 0.5f) * 2f * flickerAmount * burn;

        float scale = Mathf.LerpUnclamped(smolderFlameScale, burnFlameScale, burn) * flicker;
        flameRoot.localScale = new Vector3(scale, scale * (flameStretchBase + flameStretchFlicker * flicker), scale);

        float sway = (Mathf.PerlinNoise(noiseSeed + SwayNoiseOffset, time * flickerSpeed * swaySpeedRatio) - 0.5f) * 2f * flameSwayDegrees * burn;
        flameRoot.localRotation = Quaternion.Euler(0f, 0f, sway);

        fireLight.intensity = Mathf.LerpUnclamped(smolderLightIntensity, burnLightIntensity, burn) * flicker;
    }
}
