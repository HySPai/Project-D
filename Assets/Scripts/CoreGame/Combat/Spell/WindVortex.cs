using System.Collections.Generic;
using UnityEngine;

public class WindVortex : MonoBehaviour
{
    private const int HitBufferSize = 16;

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

        foreach (CharacterControllerBase target in targets)
        {
            if (target is EnemyController enemy)
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
        if (distance < 0.05f) return Vector3.zero;

        return toCenter / distance * Mathf.Min(distance, data.pullSpeed * dt);
    }

    private void DealTick(Vector3 center)
    {
        foreach (CharacterControllerBase target in targets)
        {
            CharacterStateBase state = target.GetState;
            if (state == null || state.IsDead) continue;

            DamageInfo info = new DamageInfo(
                data.damagePerTick, caster, center, ElementType.Wind, DamageSource.Spell, false, 0f);
            state.ReceiveDamage(in info);
        }

        Debug.Log($"[Phép] Lốc Xoáy nhát {ticksDone}/{data.TickCount}, trúng {targets.Count} mục tiêu");
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

        Gizmos.color = new Color(0.9f, 0.95f, 0.85f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, data.radius);
    }
}
