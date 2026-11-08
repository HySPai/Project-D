using DG.Tweening;
using UnityEngine;

/// <summary>Guard, block and parry for the player's current weapon, driving the Guard animator layer.</summary>
public class PlayerGuard : MonoBehaviour
{
    private const float GuardFade = 0.1f;
    private const float ReactionFade = 0.05f;
    private const float MinSlowScale = 0.05f;
    private const float AbsorbEpsilon = 0.0001f;

    private PlayerState state;
    private PlayerAnimation anim;
    private PlayerCombat combat;

    private bool guardIntent;
    private float parryWindowEnd = float.NegativeInfinity;
    private float nextParryTime;
    private float brokenUntil;
    private Tween slowMotionTween;

    private GuardData hashedData;
    private string hashedAnimationSet;
    private GuardStateHashes hashes;

    public bool IsGuarding => guardIntent;
    public bool IsBroken => Time.time < brokenUntil;
    public bool IsParryWindowOpen => Time.time <= parryWindowEnd;

    public bool IsGuardActive =>
        guardIntent
        && !IsBroken
        && state != null
        && !state.IsDead
        && !state.IsAttacking
        && !state.IsRolling;

    public float MoveSpeedMultiplier
    {
        get
        {
            GuardData data = Data;
            return IsGuardActive && data != null ? data.moveSpeedMultiplier : 1f;
        }
    }

    private GuardData Data
    {
        get
        {
            WeaponItem weapon = combat != null ? combat.CurrentWeapon : null;
            return weapon != null ? weapon.guard : null;
        }
    }

    public void Initialize(PlayerState state, PlayerAnimation animation, PlayerCombat combat)
    {
        this.state = state;
        this.anim = animation;
        this.combat = combat;
    }

    private void OnDisable()
    {
        KillSlowMotion();
    }

    public void ResetGuard()
    {
        KillSlowMotion();
        guardIntent = false;
        parryWindowEnd = float.NegativeInfinity;
        nextParryTime = 0f;
        brokenUntil = 0f;
    }

    public void Tick(bool held, bool pressed)
    {
        GuardData data = Data;

        bool allowed = data != null
                       && data.canGuard
                       && state != null
                       && !state.IsDead
                       && !state.IsRolling
                       && !IsBroken;

        bool wantGuard = held && allowed;

        if (wantGuard && !guardIntent)
            BeginGuard(data);
        else if (!wantGuard && guardIntent)
            EndGuard(data);

        if (pressed && wantGuard)
            TryOpenParry(data);
    }

    private void BeginGuard(GuardData data)
    {
        guardIntent = true;
        anim.CrossFadeGuard(HashesFor(data).up, GuardFade);
    }

    private void EndGuard(GuardData data)
    {
        guardIntent = false;
        parryWindowEnd = float.NegativeInfinity;

        if (data != null)
            anim.CrossFadeGuard(HashesFor(data).down, GuardFade);
    }

    private void TryOpenParry(GuardData data)
    {
        if (!data.canParry) return;
        if (Time.time < nextParryTime) return;
        if (!state.HasEnoughStamina(data.parryStaminaCost)) return;

        state.DrainStamina(data.parryStaminaCost);
        parryWindowEnd = Time.time + data.parryWindow;
        nextParryTime = Time.time + data.parryCooldown;
    }

    public bool TryAbsorb(ref DamageInfo info, out DamageResult absorbed)
    {
        absorbed = default;

        GuardData data = Data;
        if (data == null || !IsGuardActive) return false;

        bool inFront = GuardResolver.IsInFrontArc(
            transform.position, transform.forward, info.sourcePosition, data.blockArc);

        GuardInput input = new GuardInput(
            info.points, info.unblockable, info.staggerAmount,
            true, inFront, IsParryWindowOpen, data.canParry,
            state.CurrentStamina, data.blockedDamageRatio, data.staminaPerBlockedPoint,
            data.guardStunThreshold);

        GuardVerdict verdict = GuardResolver.Resolve(in input);

        switch (verdict.outcome)
        {
            case GuardOutcome.Parried:
                HandleParry(in info, data);
                absorbed = new DamageResult(0f, true, true, false);
                return true;

            case GuardOutcome.Blocked:
                state.DrainStamina(verdict.staminaSpent);
                HandleBlock(in info, data, verdict);
                return AbsorbBlocked(verdict, out absorbed);

            case GuardOutcome.GuardBroken:
                state.DrainStamina(verdict.staminaSpent);
                BreakGuard(data.guardBreakStun, HashesFor(data).breakAction);
                info = info.WithPoints(verdict.remainingPoints);

                if (verdict.remainingPoints > AbsorbEpsilon) return false;

                absorbed = new DamageResult(0f, true, false, false);
                return true;

            default:
                return false;
        }
    }

    private bool AbsorbBlocked(GuardVerdict verdict, out DamageResult absorbed)
    {
        if (verdict.remainingPoints <= AbsorbEpsilon)
        {
            absorbed = new DamageResult(0f, true, false, false);
            return true;
        }

        bool died = state.ApplyChipDamage(verdict.remainingPoints);
        absorbed = new DamageResult(verdict.remainingPoints, true, false, died);
        return true;
    }

    private void HandleParry(in DamageInfo info, GuardData data)
    {
        parryWindowEnd = float.NegativeInfinity;
        anim.CrossFadeGuard(HashesFor(data).parry, ReactionFade);

        CharacterStateBase attackerState = info.attacker != null ? info.attacker.GetState : null;
        if (attackerState != null)
            attackerState.OnParried(data.parryStaggerDuration);

        combat.PlayDefenseFeedback(HitFeelTier.Heavy, transform.position);
        state.GrantInvulnerability(data.blockInvulnerability, true);
        PlaySlowMotion(data);
    }

    private void HandleBlock(in DamageInfo info, GuardData data, GuardVerdict verdict)
    {
        GuardStateHashes guardSet = HashesFor(data);
        bool heavy = info.points >= data.heavyBlockPoints;
        int blockState = heavy ? guardSet.blockHeavy
            : info.points >= data.mediumBlockPoints ? guardSet.blockMedium
            : guardSet.blockLight;

        anim.CrossFadeGuard(blockState, ReactionFade);

        HitFeelTier feel = heavy ? HitFeelTier.Heavy : HitFeelTier.Light;
        combat.PlayDefenseFeedback(feel, transform.position);
        state.GrantInvulnerability(data.blockInvulnerability, true);

        if (verdict.guardStunned)
            BreakGuard(data.guardStunDuration, guardSet.stunAction);
    }

    private void BreakGuard(float duration, in AnimationAction reaction)
    {
        brokenUntil = Time.time + duration;
        parryWindowEnd = float.NegativeInfinity;

        combat.DisableAllDamageColliders();

        anim.Play(reaction);
    }

    private void PlaySlowMotion(GuardData data)
    {
        if (data.parrySlowDuration <= 0f) return;

        KillSlowMotion();
        GameTime.SetSlowMotion(Mathf.Max(data.parrySlowScale, MinSlowScale));
        slowMotionTween = DOVirtual.DelayedCall(data.parrySlowDuration, RestoreTimeScale)
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    private void RestoreTimeScale()
    {
        GameTime.ClearSlowMotion();
        slowMotionTween = null;
    }

    private void KillSlowMotion()
    {
        if (slowMotionTween == null) return;

        slowMotionTween.Kill();
        slowMotionTween = null;
        GameTime.ClearSlowMotion();
    }

    private GuardStateHashes HashesFor(GuardData data)
    {
        if (!ReferenceEquals(data, hashedData) || !string.Equals(data.animationSet, hashedAnimationSet))
        {
            hashedData = data;
            hashedAnimationSet = data.animationSet;
            hashes = new GuardStateHashes(data.animationSet);
        }
        return hashes;
    }
}

/// <summary>Animator state hashes for one guard animation set, named Guard_{set}_{suffix}.</summary>
public readonly struct GuardStateHashes
{
    public readonly int up;
    public readonly int down;
    public readonly int parry;
    public readonly int blockLight;
    public readonly int blockMedium;
    public readonly int blockHeavy;
    public readonly AnimationAction breakAction;
    public readonly AnimationAction stunAction;

    public GuardStateHashes(string animationSet)
    {
        up = Animator.StringToHash(StateName(animationSet, "Up"));
        down = Animator.StringToHash(StateName(animationSet, "Down"));
        parry = Animator.StringToHash(StateName(animationSet, "Parry"));
        blockLight = Animator.StringToHash(StateName(animationSet, "BlockLight"));
        blockMedium = Animator.StringToHash(StateName(animationSet, "BlockMedium"));
        blockHeavy = Animator.StringToHash(StateName(animationSet, "BlockHeavy"));
        breakAction = new AnimationAction(StateName(animationSet, "Break"), true, true, false, false);
        stunAction = new AnimationAction(StateName(animationSet, "Stun"), true, true, false, false);
    }

    /// <summary>Animator state name for a guard set and suffix, e.g. Guard_Off_Up.</summary>
    public static string StateName(string animationSet, string suffix) => $"Guard_{animationSet}_{suffix}";
}
