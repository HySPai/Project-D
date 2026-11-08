using System.Collections.Generic;
using UnityEngine;

/// <summary>A wind funnel that pulls, holds and periodically damages whatever stands inside it, then returns to its pool.</summary>
public class WindVortex : MonoBehaviour
{
    private const int HitBufferSize = 16;
    private const float MinPullDistance = 0.05f;
    private static readonly Color GizmoColor = new Color(0.9f, 0.95f, 0.85f, 0.8f);

    [SerializeField] private LayerMask hitLayer = 1 << 8;
    [SerializeField] private WindVortexView view;

    private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
    private readonly List<CharacterControllerBase> targets = new List<CharacterControllerBase>();

    private SO_WindVortexData data;
    private CharacterControllerBase caster;
    private float elapsed;
    private int ticksDone;

    /// <summary>Opens the vortex at position for its lifetime; it pulls, holds and damages whatever stands inside.</summary>
    public void Launch(SO_WindVortexData data, CharacterControllerBase caster, Vector3 position)
    {
        this.data = data;
        this.caster = caster;
        elapsed = 0f;
        ticksDone = 0;

        transform.SetPositionAndRotation(position, Quaternion.identity);
        gameObject.SetActive(true);

        if (view != null)
            view.Play(data);
    }

    private void Update()
    {
        if (data == null) return;

        float dt = Time.deltaTime;
        elapsed += dt;

        Vector3 center = transform.position;
        SpellTargeting.CollectTargets(center, data.radius, hitLayer, caster, hitBuffer, targets);

        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] is EnemyController enemy)
                enemy.HoldInVortex(data.holdRefresh, ResolvePull(center, enemy.transform.position, dt));
        }

        while (ticksDone < data.TickCount && elapsed >= (ticksDone + 1) * data.tickInterval)
        {
            ticksDone++;
            DealTick(center);
        }

        if (elapsed >= data.lifetime)
            Close();
    }

    private Vector3 ResolvePull(Vector3 center, Vector3 position, float dt)
    {
        Vector3 toCenter = center - position;
        toCenter.y = 0f;

        float distance = toCenter.magnitude;
        if (distance < MinPullDistance) return Vector3.zero;

        return toCenter / distance * Mathf.Min(distance, data.pullSpeed * dt);
    }

    private void DealTick(Vector3 center)
    {
        for (int i = 0; i < targets.Count; i++)
            SpellTargeting.DealSpellDamage(targets[i], data.damagePerTick, caster, center, ElementType.Wind);

        GameLog.Info($"[Phép] Lốc Xoáy nhát {ticksDone}/{data.TickCount}, trúng {targets.Count} mục tiêu");
    }

    private void Close()
    {
        data = null;
        targets.Clear();
        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        if (data == null) return;

        Gizmos.color = GizmoColor;
        Gizmos.DrawWireSphere(transform.position, data.radius);
    }
}
