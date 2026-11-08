using System.Collections.Generic;
using UnityEngine;

/// <summary>Water skill: a ring around the caster that damages, pushes back and slows everything inside.</summary>
public sealed class WaterRingLauncher : ISpellLauncher
{
    private readonly SO_WaterRingData data;
    private readonly WaterRingView viewPrefab;
    private readonly SpellCastContext context;
    private WaterRingView view;

    public WaterRingLauncher(SO_WaterRingData data, WaterRingView viewPrefab, SpellCastContext context)
    {
        this.data = data;
        this.viewPrefab = viewPrefab;
        this.context = context;
    }

    public ElementType Element => ElementType.Water;
    public bool IsBusy => false;

    public bool TryLaunch()
    {
        if (data == null) return false;

        Vector3 center = context.CasterTransform.position;
        PlayView(center);
        context.CollectTargets(center, data.radius);

        IReadOnlyList<CharacterControllerBase> targets = context.Targets;
        for (int i = 0; i < targets.Count; i++)
        {
            CharacterControllerBase target = targets[i];
            if (!SpellTargeting.DealSpellDamage(target, data.damage, context.Character, center, ElementType.Water)) continue;

            if (target is EnemyController enemy)
                enemy.PushAway(center, data.pushDistance);

            if (target.GetSlow != null)
                target.GetSlow.Apply();
        }

        GameLog.Info($"[Phép] Vòng Nước trúng {targets.Count} mục tiêu");
        return true;
    }

    private void PlayView(Vector3 center)
    {
        if (viewPrefab == null) return;

        if (view == null)
            view = Object.Instantiate(viewPrefab);

        view.Play(center, data);
    }
}
