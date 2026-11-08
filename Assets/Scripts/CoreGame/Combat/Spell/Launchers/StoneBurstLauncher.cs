using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>Stone skill: after a short windup, spikes burst out in a forward arc, damaging and stunning what they hit.</summary>
public sealed class StoneBurstLauncher : ISpellLauncher
{
    private readonly SO_StoneBurstData data;
    private readonly StoneBurstView viewPrefab;
    private readonly SpellCastContext context;
    private StoneBurstView view;
    private bool windingUp;

    public StoneBurstLauncher(SO_StoneBurstData data, StoneBurstView viewPrefab, SpellCastContext context)
    {
        this.data = data;
        this.viewPrefab = viewPrefab;
        this.context = context;
    }

    public ElementType Element => ElementType.Stone;
    public bool IsBusy => windingUp;

    public bool TryLaunch()
    {
        if (data == null) return false;

        Vector3 direction = context.FaceCastDirection();
        WindupAsync(direction).Forget();
        return true;
    }

    private async UniTaskVoid WindupAsync(Vector3 direction)
    {
        windingUp = true;

        bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(data.windup), cancellationToken: context.LifetimeToken)
            .SuppressCancellationThrow();

        windingUp = false;
        if (canceled) return;

        if (!context.CasterCanAct)
        {
            GameLog.Info("[Phép] Đá Văng bị hủy giữa lúc khởi động");
            return;
        }

        Release(direction);
    }

    private void Release(Vector3 direction)
    {
        Vector3 origin = context.CasterTransform.position;
        PlayView(origin, direction);
        context.CollectTargets(origin, data.range);

        int hits = 0;
        IReadOnlyList<CharacterControllerBase> targets = context.Targets;
        for (int i = 0; i < targets.Count; i++)
        {
            CharacterControllerBase target = targets[i];
            if (!SpellTargeting.IsWithinArc(origin, direction, target.transform.position, data.arc)) continue;

            bool alive = SpellTargeting.DealSpellDamage(target, data.damage, context.Character, origin, ElementType.Stone);
            hits++;

            if (alive && target is EnemyController enemy)
                enemy.ApplyStun(data.stunDuration);
        }

        GameLog.Info($"[Phép] Đá Văng trúng {hits} mục tiêu");
    }

    private void PlayView(Vector3 origin, Vector3 direction)
    {
        if (viewPrefab == null) return;

        if (view == null)
            view = UnityEngine.Object.Instantiate(viewPrefab);

        view.Play(origin, direction, data);
    }
}
