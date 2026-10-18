using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class PlayerGuard : MonoBehaviour
{
    private const float GuardFade = 0.1f;
    private const float ReactionFade = 0.05f;

    private PlayerState state;
    private PlayerAnimation anim;
    private PlayerCombat combat;

    private bool guardIntent;
    private float parryWindowEnd = float.NegativeInfinity;
    private float nextParryTime;
    private float brokenUntil;
    private Tween slowMotionTween;

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
        anim.CrossFadeGuard(StateName(data, "Up"), GuardFade);
    }

    private void EndGuard(GuardData data)
    {
        guardIntent = false;
        parryWindowEnd = float.NegativeInfinity;

        if (data != null)
            anim.CrossFadeGuard(StateName(data, "Down"), GuardFade);
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
                BreakGuard(data, data.guardBreakStun, "Break");
                info = info.WithPoints(verdict.remainingPoints);

                if (verdict.remainingPoints > 0.0001f) return false;

                absorbed = new DamageResult(0f, true, false, false);
                return true;

            default:
                return false;
        }
    }

    private bool AbsorbBlocked(GuardVerdict verdict, out DamageResult absorbed)
    {
        if (verdict.remainingPoints <= 0.0001f)
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
        anim.CrossFadeGuard(StateName(data, "Parry"), ReactionFade);

        CharacterStateBase attackerState = info.attacker != null ? info.attacker.GetState : null;
        if (attackerState != null)
            attackerState.OnParried(data.parryStaggerDuration);

        combat.PlayDefenseFeedback(HitFeelTier.Heavy, transform.position);
        GrantShortInvulnerability(data.blockInvulnerability).Forget();
        PlaySlowMotion(data);
    }

    private void HandleBlock(in DamageInfo info, GuardData data, GuardVerdict verdict)
    {
        string tier = info.points >= data.heavyBlockPoints ? "BlockHeavy"
            : info.points >= data.mediumBlockPoints ? "BlockMedium"
            : "BlockLight";

        anim.CrossFadeGuard(StateName(data, tier), ReactionFade);

        HitFeelTier feel = tier == "BlockHeavy" ? HitFeelTier.Heavy : HitFeelTier.Light;
        combat.PlayDefenseFeedback(feel, transform.position);
        GrantShortInvulnerability(data.blockInvulnerability).Forget();

        if (verdict.guardStunned)
            BreakGuard(data, data.guardStunDuration, "Stun");
    }

    private void BreakGuard(GuardData data, float duration, string stateSuffix)
    {
        brokenUntil = Time.time + duration;
        parryWindowEnd = float.NegativeInfinity;

        combat.CancelCharge();
        combat.DisableAllDamageColliders();

        anim.Play(new AnimationAction(StateName(data, stateSuffix), true, true, false, false));
    }

    private async UniTaskVoid GrantShortInvulnerability(float duration)
    {
        if (duration <= 0f || state == null) return;

        state.EnableIsInvulnerable();

        bool cancelled = await UniTask
            .Delay(TimeSpan.FromSeconds(duration), true, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy())
            .SuppressCancellationThrow();

        if (cancelled) return;
        state.DisableIsInvulnerable();
    }

    private void PlaySlowMotion(GuardData data)
    {
        if (data.parrySlowDuration <= 0f) return;

        KillSlowMotion();
        Time.timeScale = Mathf.Clamp(data.parrySlowScale, 0.05f, 1f);
        slowMotionTween = DOVirtual.DelayedCall(data.parrySlowDuration, RestoreTimeScale).SetUpdate(true);
    }

    private void RestoreTimeScale()
    {
        Time.timeScale = 1f;
        slowMotionTween = null;
    }

    private void KillSlowMotion()
    {
        if (slowMotionTween == null) return;

        slowMotionTween.Kill();
        slowMotionTween = null;
        Time.timeScale = 1f;
    }

    private static string StateName(GuardData data, string suffix) => $"Guard_{data.animationSet}_{suffix}";
}
