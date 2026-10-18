using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "DeathDissolveProfile", menuName = "Game/Combat/Death Dissolve Profile")]
public class DeathDissolveProfile : ScriptableObject
{
    [Header("Timing")]
    [SerializeField, Range(0f, 5f)] private float delayAfterDeath = 1.1f;
    [SerializeField, Range(0.1f, 6f)] private float dissolveDuration = 1.4f;
    [SerializeField] private Ease dissolveEase = Ease.InQuad;

    [Header("Dissolve Look")]
    [SerializeField, Range(0f, 0.3f)] private float edgeWidth = 0.08f;
    [SerializeField, ColorUsage(false, true)] private Color edgeColor = new Color(3f, 0.6f, 0.25f, 1f);
    [SerializeField, Range(0.5f, 20f)] private float noiseScale = 4f;

    [Header("Particles")]
    [SerializeField, Range(0, 600)] private int particleCount = 140;
    [SerializeField] private Vector2 particleLifetimeRange = new Vector2(0.6f, 1.3f);
    [SerializeField] private Vector2 particleSizeRange = new Vector2(0.03f, 0.08f);
    [SerializeField] private Vector2 particleRiseSpeedRange = new Vector2(0.6f, 1.6f);
    [SerializeField, Range(0f, 3f)] private float particleTurbulence = 0.6f;
    [SerializeField, ColorUsage(false)] private Color particleColor = new Color(1f, 0.55f, 0.3f, 1f);
    [SerializeField, Range(0f, 10f)] private float particleGlow = 3f;

    public float DelayAfterDeath => delayAfterDeath;
    public float DissolveDuration => dissolveDuration;
    public Ease DissolveEase => dissolveEase;

    public float EdgeWidth => edgeWidth;
    public Color EdgeColor => edgeColor;
    public float NoiseScale => noiseScale;

    public int ParticleCount => particleCount;
    public Vector2 ParticleLifetimeRange => particleLifetimeRange;
    public Vector2 ParticleSizeRange => particleSizeRange;
    public Vector2 ParticleRiseSpeedRange => particleRiseSpeedRange;
    public float ParticleTurbulence => particleTurbulence;
    public Color ParticleColor => particleColor;
    public float ParticleGlow => particleGlow;
}
