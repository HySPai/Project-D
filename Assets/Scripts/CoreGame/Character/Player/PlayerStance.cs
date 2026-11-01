using System;
using UnityEngine;

public enum PlayerStanceMode
{
    Explore,
    Combat
}

/// <summary>Switches the player between exploring (weapons stowed) and combat (sword and shield drawn).</summary>
public class PlayerStance : MonoBehaviour
{
    [SerializeField] private SO_PlayerStanceConfig config;
    [SerializeField] private WeaponRevealer[] weapons;

    private PlayerState state;
    private PlayerAnimation anim;
    private PlayerCombat combat;
    private PlayerGuard guard;

    private PlayerStanceMode mode = PlayerStanceMode.Explore;
    private float lastThreatTime = float.NegativeInfinity;
    private float weaponReadyTime;
    private bool attackQueued;

    public event Action<PlayerStanceMode> OnStanceChanged;

    public PlayerStanceMode Mode => mode;
    public bool IsCombat => mode == PlayerStanceMode.Combat;
    public bool IsWeaponReady => IsCombat && Time.time >= weaponReadyTime;

    public void Initialize(PlayerState state, PlayerAnimation animation, PlayerCombat combat, PlayerGuard guard)
    {
        this.state = state;
        this.anim = animation;
        this.combat = combat;
        this.guard = guard;

        for (int i = 0; i < weapons.Length; i++)
            weapons[i].Initialize(config);

        ResetToExplore();
    }

    public void Tick()
    {
        if (state == null || state.IsDead) return;

        bool threat = HasThreat();

        if (mode == PlayerStanceMode.Explore)
        {
            if (threat) RequestCombat();
            return;
        }

        if (threat || state.IsAttacking || (guard != null && guard.IsGuardActive))
            lastThreatTime = Time.time;

        if (Time.time - lastThreatTime >= config.combatExitDelay)
            EnterExplore();
    }

    /// <summary>Draws the weapons right away, e.g. when the player presses attack or guard while exploring.</summary>
    public void RequestCombat()
    {
        lastThreatTime = Time.time;
        if (mode == PlayerStanceMode.Explore)
            EnterCombat();
    }

    public void QueueAttack()
    {
        RequestCombat();
        attackQueued = true;
    }

    /// <summary>Returns true once for an attack queued during the draw, as soon as the weapon is in hand.</summary>
    public bool ConsumeReadyAttack()
    {
        if (!attackQueued || !IsWeaponReady) return false;

        attackQueued = false;
        return true;
    }

    /// <summary>Snaps back to exploring with the weapons hidden, without the sheathe animation.</summary>
    public void ResetToExplore()
    {
        mode = PlayerStanceMode.Explore;
        lastThreatTime = float.NegativeInfinity;
        attackQueued = false;

        anim.SetCombatBlend(0f, 0f);
        anim.StopEquip();

        for (int i = 0; i < weapons.Length; i++)
            weapons[i].SetProgress(0f);

        state.SetCombatStance(false);
    }

    private bool HasThreat()
    {
        if (combat.LockOnTransform != null) return true;
        return EnemyAwarenessRegistry.HasAwareEnemy;
    }

    private void EnterCombat()
    {
        mode = PlayerStanceMode.Combat;
        weaponReadyTime = Time.time + config.weaponReadyTime;

        anim.SetCombatBlend(1f, config.locomotionBlendTime);
        anim.PlayEquip(config.drawStateName, 0f, config.equipLayerFadeTime, config.drawDuration);

        for (int i = 0; i < weapons.Length; i++)
            weapons[i].Reveal(config.drawRevealDelay, config.drawRevealDuration);

        state.SetCombatStance(true);
        OnStanceChanged?.Invoke(mode);
    }

    private void EnterExplore()
    {
        mode = PlayerStanceMode.Explore;
        attackQueued = false;

        anim.SetCombatBlend(0f, config.locomotionBlendTime);
        anim.PlayEquip(config.sheatheStateName, config.sheatheStartNormalizedTime, config.equipLayerFadeTime, config.sheatheDuration);

        for (int i = 0; i < weapons.Length; i++)
            weapons[i].Dissolve(config.sheatheDissolveDelay, config.sheatheDissolveDuration);

        state.SetCombatStance(false);
        OnStanceChanged?.Invoke(mode);
    }
}
