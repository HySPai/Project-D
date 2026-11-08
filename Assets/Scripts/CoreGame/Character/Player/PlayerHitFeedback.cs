using UnityEngine;

/// <summary>Swing, hit, blood and defense feedback for the player's weapon. Pure C#; every reference may be null.</summary>
public sealed class PlayerHitFeedback
{
    private const float DirectionEpsilon = 0.0001f;

    private readonly CombatFeelPlayer combatFeel;
    private readonly CombatVfxController combatVfx;
    private readonly EffectController effectController;
    private readonly TrailRenderer swingTrail;

    public PlayerHitFeedback(CombatFeelPlayer combatFeel, CombatVfxController combatVfx,
        EffectController effectController, TrailRenderer swingTrail)
    {
        this.combatFeel = combatFeel;
        this.combatVfx = combatVfx;
        this.effectController = effectController;
        this.swingTrail = swingTrail;

        if (combatFeel != null)
            combatFeel.Initialize();

        SetTrailEmitting(false);
    }

    /// <summary>Starts the swing trail and plays the swing feel.</summary>
    public void PlaySwing(WeaponFeelProfile feel, Vector3 position)
    {
        if (swingTrail != null)
        {
            swingTrail.Clear();
            swingTrail.emitting = true;
        }

        if (combatFeel != null)
            combatFeel.PlaySwing(feel, position);
    }

    public void StopSwing() => SetTrailEmitting(false);

    /// <summary>Blood spray and blood slash on one struck target.</summary>
    public void PlayBlood(AttackData attack, WeaponFeelProfile feel, Vector3 origin, Vector3 targetPosition,
        HitBloodProfile blood, bool killed)
    {
        if (combatVfx == null) return;

        HitFeelTier tier = CombatFeelPlayer.ResolveTier(attack, killed);
        if (feel != null)
            combatVfx.PlayBlood(targetPosition, targetPosition - origin, tier, blood, feel.GetBloodDroplets(tier));
        else
            combatVfx.PlayBlood(targetPosition, targetPosition - origin, tier, blood);

        PlayBloodSlash(origin, targetPosition, blood, feel, tier);
    }

    /// <summary>Hit stop, shake and sound once per sweep that struck anything.</summary>
    public void PlayHit(AttackData attack, WeaponFeelProfile feel, Vector3 position, bool killedAny)
    {
        if (combatFeel == null) return;

        HitFeelTier tier = CombatFeelPlayer.ResolveTier(attack, killedAny);
        combatFeel.PlayHit(feel, tier, attack.impact, position, killedAny);
    }

    /// <summary>Feedback for a block or parry at full impact.</summary>
    public void PlayDefense(WeaponFeelProfile feel, HitFeelTier tier, Vector3 position)
    {
        if (combatFeel == null) return;

        combatFeel.PlayHit(feel, tier, 1f, position);
    }

    private void PlayBloodSlash(Vector3 origin, Vector3 targetPosition, HitBloodProfile blood, WeaponFeelProfile feel, HitFeelTier tier)
    {
        if (effectController == null || blood == null || feel == null) return;

        Vector3 direction = targetPosition - origin;
        direction.y = 0f;
        if (direction.sqrMagnitude < DirectionEpsilon) return;

        Vector3 spawn = targetPosition + Vector3.up * blood.SpawnHeight;
        effectController.SpawnFx(feel.GetBloodSlashVfx(tier), spawn, Quaternion.LookRotation(direction), feel.BloodSlashLifetime);
    }

    private void SetTrailEmitting(bool emitting)
    {
        if (swingTrail != null)
            swingTrail.emitting = emitting;
    }
}
