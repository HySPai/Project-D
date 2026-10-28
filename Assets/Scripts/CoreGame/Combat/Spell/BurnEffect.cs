using DG.Tweening;
using UnityEngine;

public class BurnEffect : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField] private SO_BurnData data;
    [SerializeField] private Renderer[] flashRenderers;
    [SerializeField] private ParticleSystem burnVfxPrefab;
    [SerializeField] private Transform vfxAnchor;

    private MaterialPropertyBlock block;
    private Color[] baseColors;
    private CharacterControllerBase owner;
    private ParticleSystem burnVfx;
    private Tween flashTween;
    private float remaining;
    private float tickTimer;

    public bool IsBurning => remaining > 0f;

    public void Initialize(CharacterControllerBase owner)
    {
        this.owner = owner;
        block = new MaterialPropertyBlock();

        baseColors = new Color[flashRenderers.Length];
        for (int i = 0; i < flashRenderers.Length; i++)
        {
            Material material = flashRenderers[i] != null ? flashRenderers[i].sharedMaterial : null;
            baseColors[i] = material != null && material.HasProperty(BaseColorId)
                ? material.GetColor(BaseColorId)
                : Color.white;
        }
    }

    /// <summary>Sets the target on fire, or refreshes the timer if it already burns; burning never stacks.</summary>
    public void Apply()
    {
        if (data == null || owner == null) return;

        CharacterStateBase state = owner.GetState;
        if (state == null || state.IsDead) return;

        bool wasBurning = IsBurning;
        remaining = data.duration;

        if (!wasBurning)
        {
            tickTimer = data.tickInterval;
            PlayVfx();
        }

        Flash();
    }

    /// <summary>Puts the fire out at once and restores the original colours.</summary>
    public void Stop()
    {
        remaining = 0f;
        flashTween?.Kill();
        SetFlash(0f);

        if (burnVfx != null)
            burnVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void Update()
    {
        if (!IsBurning) return;

        CharacterStateBase state = owner.GetState;
        if (state == null || state.IsDead)
        {
            Stop();
            return;
        }

        remaining -= Time.deltaTime;
        tickTimer -= Time.deltaTime;

        if (tickTimer <= 0f)
        {
            tickTimer += data.tickInterval;
            Tick(state);
        }

        if (remaining <= 0f)
            Stop();
    }

    private void Tick(CharacterStateBase state)
    {
        DamageInfo info = new DamageInfo(
            data.DamagePerTick, null, transform.position, ElementType.Fire, DamageSource.Status, true, 0f);
        state.ReceiveDamage(in info);

        if (!state.IsDead)
            Flash();
    }

    private void PlayVfx()
    {
        if (burnVfxPrefab == null) return;

        if (burnVfx == null)
        {
            Transform anchor = vfxAnchor != null ? vfxAnchor : transform;
            burnVfx = Instantiate(burnVfxPrefab, anchor.position, Quaternion.identity, anchor);
        }

        burnVfx.Play(true);
    }

    private void Flash()
    {
        flashTween?.Kill();
        flashTween = DOVirtual.Float(1f, 0f, data.flashDuration, SetFlash)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    private void SetFlash(float weight)
    {
        if (block == null) return;

        for (int i = 0; i < flashRenderers.Length; i++)
        {
            Renderer target = flashRenderers[i];
            if (target == null) continue;

            target.GetPropertyBlock(block);
            block.SetColor(BaseColorId, Color.Lerp(baseColors[i], data.flashColor, weight));
            target.SetPropertyBlock(block);
        }
    }
}
