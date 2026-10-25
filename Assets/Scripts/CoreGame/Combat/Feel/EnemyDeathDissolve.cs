using DG.Tweening;
using UnityEngine;

/// <summary>
/// Disintegrate and dissolve on death: swaps the enemy materials to the dissolve shader,
/// burns them away with a glowing edge while a particle system sampled from the skinned mesh
/// drifts upward, then hides the body.
/// Example: deathDissolve.Prepare(profile) on spawn, deathDissolve.Play(profile) when the enemy dies.
/// </summary>
public class EnemyDeathDissolve : MonoBehaviour
{
    private const string EmissionKeyword = "_EMISSION";

    private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionMapId = Shader.PropertyToID("_EmissionMap");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int DissolveAmountId = Shader.PropertyToID("_DissolveAmount");
    private static readonly int EdgeWidthId = Shader.PropertyToID("_EdgeWidth");
    private static readonly int EdgeColorId = Shader.PropertyToID("_EdgeColor");
    private static readonly int NoiseScaleId = Shader.PropertyToID("_NoiseScale");

    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Shader dissolveShader;
    [SerializeField] private ParticleSystem dissolveParticles;
    [SerializeField] private GameObject hideOnComplete;

    private Material[][] dissolveMaterials;
    private Material[][] originalMaterials;
    private MaterialPropertyBlock particleBlock;
    private Sequence sequence;
    private bool prepared;

    /// <summary>Builds the dissolve materials up front so dying allocates nothing.</summary>
    public void Prepare(DeathDissolveProfile profile)
    {
        if (prepared || profile == null || dissolveShader == null || renderers == null) return;
        prepared = true;

        dissolveMaterials = new Material[renderers.Length][];
        originalMaterials = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] source = renderers[i] != null ? renderers[i].sharedMaterials : new Material[0];
            originalMaterials[i] = source;
            Material[] target = new Material[source.Length];
            for (int j = 0; j < source.Length; j++)
                target[j] = CreateDissolveMaterial(source[j]);
            dissolveMaterials[i] = target;
        }

        particleBlock = new MaterialPropertyBlock();
    }

    /// <summary>Waits for the death pose, dissolves the body, then hides it.</summary>
    public void Play(DeathDissolveProfile profile)
    {
        if (profile == null || dissolveShader == null) return;

        Prepare(profile);
        sequence?.Kill();

        sequence = DOTween.Sequence()
            .AppendInterval(profile.DelayAfterDeath)
            .AppendCallback(() => BeginDissolve(profile))
            .Append(DOVirtual.Float(0f, 1f, profile.DissolveDuration, SetDissolveAmount).SetEase(profile.DissolveEase))
            .AppendCallback(HideRenderers)
            .AppendInterval(profile.ParticleLifetimeRange.y)
            .OnComplete(Finish)
            .SetLink(gameObject);
    }

    /// <summary>Undoes the dissolve so a respawned enemy looks untouched.</summary>
    public void ResetVisual()
    {
        sequence?.Kill();
        sequence = null;

        if (dissolveParticles != null)
            dissolveParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (dissolveMaterials != null)
            SetDissolveAmount(0f);

        for (int i = 0; renderers != null && i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            renderers[i].enabled = true;
            if (originalMaterials != null)
                renderers[i].sharedMaterials = originalMaterials[i];
        }

        if (hideOnComplete != null)
            hideOnComplete.SetActive(true);
    }

    private void OnDestroy()
    {
        sequence?.Kill();

        if (dissolveMaterials == null) return;
        for (int i = 0; i < dissolveMaterials.Length; i++)
        {
            Material[] materials = dissolveMaterials[i];
            for (int j = 0; j < materials.Length; j++)
                if (materials[j] != null) Destroy(materials[j]);
        }
    }

    private Material CreateDissolveMaterial(Material source)
    {
        Material material = new Material(dissolveShader);
        if (source == null) return material;

        material.name = source.name + " (Dissolve)";

        if (source.HasProperty(BaseMapId))
        {
            material.SetTexture(BaseMapId, source.GetTexture(BaseMapId));
            material.SetTextureScale(BaseMapId, source.GetTextureScale(BaseMapId));
            material.SetTextureOffset(BaseMapId, source.GetTextureOffset(BaseMapId));
        }

        if (source.HasProperty(BaseColorId))
            material.SetColor(BaseColorId, source.GetColor(BaseColorId));

        bool emissive = source.IsKeywordEnabled(EmissionKeyword);
        if (emissive && source.HasProperty(EmissionMapId))
            material.SetTexture(EmissionMapId, source.GetTexture(EmissionMapId));
        material.SetColor(EmissionColorId,
            emissive && source.HasProperty(EmissionColorId) ? source.GetColor(EmissionColorId) : Color.black);

        material.SetFloat(DissolveAmountId, 0f);
        return material;
    }

    private void BeginDissolve(DeathDissolveProfile profile)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            Material[] materials = dissolveMaterials[i];
            for (int j = 0; j < materials.Length; j++)
            {
                materials[j].SetFloat(DissolveAmountId, 0f);
                materials[j].SetFloat(EdgeWidthId, profile.EdgeWidth);
                materials[j].SetColor(EdgeColorId, profile.EdgeColor);
                materials[j].SetFloat(NoiseScaleId, profile.NoiseScale);
            }

            renderers[i].sharedMaterials = materials;
        }

        StartParticles(profile);
    }

    private void SetDissolveAmount(float amount)
    {
        for (int i = 0; i < dissolveMaterials.Length; i++)
        {
            Material[] materials = dissolveMaterials[i];
            for (int j = 0; j < materials.Length; j++)
                materials[j].SetFloat(DissolveAmountId, amount);
        }
    }

    private void StartParticles(DeathDissolveProfile profile)
    {
        if (dissolveParticles == null || profile.ParticleCount <= 0) return;

        dissolveParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        float duration = Mathf.Max(0.05f, profile.DissolveDuration);
        Vector2 lifetime = profile.ParticleLifetimeRange;
        Vector2 size = profile.ParticleSizeRange;
        Vector2 rise = profile.ParticleRiseSpeedRange;

        ParticleSystem.MainModule main = dissolveParticles.main;
        main.duration = duration;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime.x, lifetime.y);
        main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
        main.startColor = profile.ParticleColor;

        ParticleSystem.EmissionModule emission = dissolveParticles.emission;
        emission.rateOverTime = profile.ParticleCount / duration;

        ParticleSystem.VelocityOverLifetimeModule velocity = dissolveParticles.velocityOverLifetime;
        velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.y = new ParticleSystem.MinMaxCurve(rise.x, rise.y);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        ParticleSystem.NoiseModule noise = dissolveParticles.noise;
        noise.strength = profile.ParticleTurbulence;

        ParticleSystemRenderer particleRenderer = dissolveParticles.GetComponent<ParticleSystemRenderer>();
        particleRenderer.GetPropertyBlock(particleBlock);
        particleBlock.SetColor(BaseColorId, Color.white * profile.ParticleGlow);
        particleRenderer.SetPropertyBlock(particleBlock);

        dissolveParticles.Play();
    }

    private void HideRenderers()
    {
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].enabled = false;
    }

    private void Finish()
    {
        if (hideOnComplete != null)
            hideOnComplete.SetActive(false);
    }
}
