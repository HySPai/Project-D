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
    [SerializeField] private float streakFadeTime = 0.6f;

    private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
    private readonly HashSet<CharacterStateBase> hitStates = new HashSet<CharacterStateBase>();

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
        int count = Physics.OverlapSphereNonAlloc(
            center, data.explosionRadius, hitBuffer, hitLayer, QueryTriggerInteraction.Ignore);

        hitStates.Clear();

        for (int i = 0; i < count; i++)
        {
            if (!CombatTargetRegistry.TryGet(hitBuffer[i], out CharacterControllerBase target)) continue;
            if (target == null || target == caster) continue;

            CharacterStateBase targetState = target.GetState;
            if (targetState == null || targetState.IsDead) continue;
            if (!hitStates.Add(targetState)) continue;

            DamageInfo info = new DamageInfo(
                data.damage, caster, center, ElementType.Fire, DamageSource.Spell, false, 0f);
            targetState.ReceiveDamage(in info);

            if (target.GetBurn != null)
                target.GetBurn.Apply();
        }

        if (explosionVfxPrefab != null)
            Instantiate(explosionVfxPrefab, center, Quaternion.identity);

        Debug.Log($"[Phép] Cầu Lửa nổ tại {center}, trúng {hitStates.Count} mục tiêu");

        data = null;
        DetachStreak();
        gameObject.SetActive(false);
    }

    private void StartStreak()
    {
        foreach (TrailRenderer trail in streakTrails)
        {
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

    private void DetachStreak()
    {
        if (streak == null) return;

        StopStreak(ParticleSystemStopBehavior.StopEmitting);
        streak.SetParent(null, true);
        streakDetached = true;
        ReattachStreakAsync(++streakVersion).Forget();
    }

    private void AttachStreak()
    {
        if (streak == null || !streakDetached) return;

        streakVersion++;
        streakDetached = false;
        streak.SetParent(transform, false);
        streak.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private async UniTaskVoid ReattachStreakAsync(int version)
    {
        bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(streakFadeTime),
                cancellationToken: this.GetCancellationTokenOnDestroy())
            .SuppressCancellationThrow();
        if (canceled || version != streakVersion) return;

        AttachStreak();
    }
}
