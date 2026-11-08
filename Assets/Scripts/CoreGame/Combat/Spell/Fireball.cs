using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    private const int HitBufferSize = 16;

    [SerializeField] private LayerMask hitLayer = 1 << 8;
    [SerializeField] private LayerMask blockLayer = 1;
    [SerializeField] private ParticleSystem explosionVfxPrefab;

    [Header("Streak")]
    [SerializeField] private Transform streak;
    [SerializeField] private TrailRenderer[] streakTrails;
    [SerializeField] private ParticleSystem[] streakParticles;

    private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
    private readonly List<CharacterControllerBase> targets = new List<CharacterControllerBase>();

    private SO_FireballData data;
    private CharacterControllerBase caster;
    private Vector3 direction;
    private float travelled;
    private bool streakDetached;
    private int streakVersion;

    /// <summary>Places the fireball at the cast point and sends it flying in a straight line.</summary>
    public void Launch(SO_FireballData data, CharacterControllerBase caster, Vector3 position, Vector3 direction)
    {
        this.data = data;
        this.caster = caster;
        this.direction = direction;
        travelled = 0f;

        AttachStreak();
        StopStreak(ParticleSystemStopBehavior.StopEmittingAndClear);

        transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
        gameObject.SetActive(true);

        StartStreak();
    }

    private void Update()
    {
        if (data == null) return;

        float step = Mathf.Min(data.speed * Time.deltaTime, data.maxRange - travelled);
        Vector3 origin = transform.position;

        if (Physics.SphereCast(origin, data.projectileRadius, direction, out RaycastHit hit, step,
                hitLayer | blockLayer, QueryTriggerInteraction.Ignore)
            && !IsCaster(hit.collider))
        {
            transform.position = origin + direction * hit.distance;
            Explode();
            return;
        }

        transform.position = origin + direction * step;
        travelled += step;

        if (travelled >= data.maxRange)
            Explode();
    }

    private void OnDestroy()
    {
        if (streakDetached && streak != null)
            Destroy(streak.gameObject);
    }

    private bool IsCaster(Collider other)
    {
        return CombatTargetRegistry.TryGet(other, out CharacterControllerBase target) && target == caster;
    }

    private void Explode()
    {
        Vector3 center = transform.position;
        SpellTargeting.CollectTargets(center, data.explosionRadius, hitLayer, caster, hitBuffer, targets);

        for (int i = 0; i < targets.Count; i++)
        {
            CharacterControllerBase target = targets[i];
            SpellTargeting.DealSpellDamage(target, data.damage, caster, center, ElementType.Fire);

            if (target.GetBurn != null)
                target.GetBurn.Apply();
        }

        PlayExplosionVfx(center);
        GameLog.Info($"[Phép] Cầu Lửa nổ tại {center}, trúng {targets.Count} mục tiêu");

        targets.Clear();
        DetachStreak(data.streakFadeTime);
        data = null;
        gameObject.SetActive(false);
    }

    private void PlayExplosionVfx(Vector3 center)
    {
        if (explosionVfxPrefab == null) return;

        ParticleSystem vfx = ObjectPool.Get(explosionVfxPrefab);
        vfx.transform.SetPositionAndRotation(center, Quaternion.identity);
        vfx.Clear(true);
        vfx.Play(true);
    }

    private void StartStreak()
    {
        foreach (TrailRenderer trail in streakTrails)
        {
            trail.time = data.trailTime;
            trail.Clear();
            trail.emitting = true;
        }

        foreach (ParticleSystem system in streakParticles)
        {
            system.Clear(false);
            system.Play(false);
        }
    }

    private void StopStreak(ParticleSystemStopBehavior behavior)
    {
        foreach (TrailRenderer trail in streakTrails)
        {
            trail.emitting = false;
            if (behavior == ParticleSystemStopBehavior.StopEmittingAndClear)
                trail.Clear();
        }

        foreach (ParticleSystem system in streakParticles)
            system.Stop(false, behavior);
    }

    private void DetachStreak(float fadeTime)
    {
        if (streak == null) return;

        StopStreak(ParticleSystemStopBehavior.StopEmitting);
        streak.SetParent(null, true);
        streakDetached = true;
        ReattachStreakAsync(++streakVersion, fadeTime).Forget();
    }

    private void AttachStreak()
    {
        if (streak == null || !streakDetached) return;

        streakVersion++;
        streakDetached = false;
        streak.SetParent(transform, false);
        streak.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private async UniTaskVoid ReattachStreakAsync(int version, float fadeTime)
    {
        bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(fadeTime),
                cancellationToken: destroyCancellationToken)
            .SuppressCancellationThrow();
        if (canceled || version != streakVersion) return;

        AttachStreak();
    }
}
