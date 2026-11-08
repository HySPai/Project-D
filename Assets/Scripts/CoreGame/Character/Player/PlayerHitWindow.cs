using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Tuning values for PlayerHitWindow, taken from PlayerCombat's serialized fields.</summary>
public readonly struct HitWindowSettings
{
    public readonly LayerMask hitLayer;
    public readonly float originHeight;
    public readonly float heightTolerance;
    public readonly float pointBlankDistance;
    public readonly float pointBlankArc;
    public readonly float maxDuration;

    public HitWindowSettings(LayerMask hitLayer, float originHeight, float heightTolerance,
        float pointBlankDistance, float pointBlankArc, float maxDuration)
    {
        this.hitLayer = hitLayer;
        this.originHeight = originHeight;
        this.heightTolerance = heightTolerance;
        this.pointBlankDistance = pointBlankDistance;
        this.pointBlankArc = pointBlankArc;
        this.maxDuration = maxDuration;
    }
}

/// <summary>
/// The active part of a player swing. Between Begin and End it sweeps an arc every frame and damages each
/// target at most once. Pure C#, owned by PlayerCombat.
/// </summary>
public sealed class PlayerHitWindow
{
    private const float BroadphasePadding = 1.5f;
    private const float PointBlankRadius = 0.05f;
    private const int HitBufferSize = 24;

    private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
    private readonly HashSet<CharacterStateBase> hitTargets = new HashSet<CharacterStateBase>();
    private readonly Transform owner;
    private readonly CharacterControllerBase self;
    private readonly CharacterStateBase state;
    private readonly PlayerHitFeedback feedback;
    private readonly Action<float> onWeaponHit;

    private HitWindowSettings settings;
    private AttackData attack;
    private WeaponItem weapon;
    private bool open;
    private float expireTime;

    public bool IsOpen => open;

    public PlayerHitWindow(Transform owner, CharacterControllerBase self, CharacterStateBase state,
        PlayerHitFeedback feedback, Action<float> onWeaponHit, in HitWindowSettings settings)
    {
        this.owner = owner;
        this.self = self;
        this.state = state;
        this.feedback = feedback;
        this.onWeaponHit = onWeaponHit;
        this.settings = settings;
    }

    /// <summary>Replaces the tuning values, e.g. after an Inspector edit in play mode.</summary>
    public void Configure(in HitWindowSettings value) => settings = value;

    /// <summary>Opens the window for attack and sweeps once right away.</summary>
    public void Begin(AttackData attack, WeaponItem weapon)
    {
        if (attack == null || state == null || state.IsDead) return;

        this.attack = attack;
        this.weapon = weapon;
        open = true;
        expireTime = Time.time + settings.maxDuration;
        hitTargets.Clear();

        feedback.PlaySwing(FeelProfile, owner.position);

        Sweep();
    }

    public void End()
    {
        open = false;
        feedback.StopSwing();
    }

    /// <summary>Forgets who was struck, e.g. on a combat reset.</summary>
    public void ClearHitTargets() => hitTargets.Clear();

    /// <summary>Damages every new target inside the arc; closes the window once the attack ends or times out.</summary>
    public void Sweep()
    {
        if (!open) return;

        if (state.IsDead || !state.IsAttacking || Time.time > expireTime)
        {
            End();
            return;
        }

        Vector3 origin = owner.position;
        Vector3 forward = CombatTargeting.GetFlatForward(owner);

        int count = Physics.OverlapSphereNonAlloc(
            origin + Vector3.up * settings.originHeight,
            attack.range + BroadphasePadding,
            hitBuffer, settings.hitLayer, QueryTriggerInteraction.Ignore);

        WeaponFeelProfile feel = FeelProfile;
        float knockbackScale = feel != null ? feel.GetKnockbackScale(attack.weight) : 1f;
        float stunScale = feel != null ? feel.GetStunScale(attack.weight) : 1f;
        float counterMultiplier = weapon != null ? weapon.counterMultiplier : 1f;
        float poiseMultiplier = weapon != null ? weapon.poiseMultiplier : 1f;

        int hitCount = 0;
        bool killedAny = false;

        for (int i = 0; i < count; i++)
        {
            if (!CombatTargeting.TryResolveTarget(hitBuffer[i], self, out CharacterControllerBase target)) continue;

            CharacterStateBase targetState = target.GetState;
            if (targetState.IsInvulnerable) continue;
            if (hitTargets.Contains(targetState)) continue;

            Vector3 targetPosition = target.transform.position;
            if (!IsInsideHitArc(origin, forward, targetPosition, CombatTargeting.GetTargetRadius(target),
                    attack.range, attack.arc, in settings)) continue;

            hitTargets.Add(targetState);

            float points = attack.damage;
            if (targetState.IsOffBalance)
            {
                points *= counterMultiplier;
                targetState.ClearOffBalance();
            }

            DamageInfo info = new DamageInfo(
                points, self, origin,
                ElementType.None, DamageSource.Weapon, false, attack.damage * poiseMultiplier,
                knockbackScale, stunScale);
            DamageResult result = targetState.ReceiveDamage(in info);
            if (result.appliedPoints > 0f)
                onWeaponHit?.Invoke(result.appliedPoints);

            hitCount++;
            killedAny |= result.targetDied;

            feedback.PlayBlood(attack, feel, origin, targetPosition, targetState.HitBlood, result.targetDied);
        }

        if (hitCount > 0)
            feedback.PlayHit(attack, feel, owner.position, killedAny);
    }

    /// <summary>
    /// True when a target of the given radius sits inside the swing arc. Close targets get the wider point-blank
    /// arc, and the arc widens by the angle the target's radius covers.
    /// </summary>
    public static bool IsInsideHitArc(Vector3 origin, Vector3 forward, Vector3 targetPosition, float targetRadius,
        float range, float arc, in HitWindowSettings settings)
    {
        Vector3 toTarget = targetPosition - origin;
        if (Mathf.Abs(toTarget.y) > settings.heightTolerance) return false;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        if (distance < PointBlankRadius) return true;

        float edgeDistance = distance - targetRadius;
        if (edgeDistance > range) return false;

        float effectiveArc = edgeDistance <= settings.pointBlankDistance
            ? Mathf.Max(arc, settings.pointBlankArc)
            : arc;

        float angularPadding = Mathf.Asin(Mathf.Clamp01(targetRadius / distance)) * Mathf.Rad2Deg;
        float angle = Vector3.Angle(forward, toTarget);

        return angle <= effectiveArc * 0.5f + angularPadding;
    }

    private WeaponFeelProfile FeelProfile => weapon != null ? weapon.feelProfile : null;
}
