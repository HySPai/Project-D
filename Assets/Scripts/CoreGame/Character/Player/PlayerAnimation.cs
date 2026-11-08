using DG.Tweening;
using UnityEngine;

/// <summary>Player animator: combat locomotion blend, the equip, guard and action override layers.</summary>
public class PlayerAnimation : CharacterAnimationBase
{
    private const string GuardLayerName = "Guard";
    private const string ActionLayerName = "ActionOverride";
    private const string EquipLayerName = "Equip";

    private static readonly int CombatBlendHash = Animator.StringToHash("CombatBlend");
    private static readonly int ActionEmptyStateHash = Animator.StringToHash("Empty");

    private int guardLayerIndex = -1;
    private int actionLayerIndex = -1;
    private int equipLayerIndex = -1;

    private float combatBlend;
    private float equipWeight;
    private Tween combatBlendTween;
    private Sequence equipSequence;

    public void Initialize(PlayerState state)
    {
        base.Initialize(state);

        if (animator == null) return;

        guardLayerIndex = animator.GetLayerIndex(GuardLayerName);
        actionLayerIndex = animator.GetLayerIndex(ActionLayerName);
        equipLayerIndex = animator.GetLayerIndex(EquipLayerName);
    }

    /// <summary>Tweens the locomotion blend between unarmed (0) and one-handed weapon (1).</summary>
    public void SetCombatBlend(float target, float duration)
    {
        combatBlendTween?.Kill();

        if (duration <= 0f)
        {
            ApplyCombatBlend(target);
            return;
        }

        combatBlendTween = DOTween.To(() => combatBlend, ApplyCombatBlend, target, duration)
            .SetEase(Ease.InOutSine)
            .SetLink(gameObject);
    }

    /// <summary>Plays a draw or sheathe clip on the upper-body equip layer, then fades the layer back out.</summary>
    public void PlayEquip(string stateName, float normalizedStart, float fadeTime, float duration)
    {
        if (string.IsNullOrEmpty(stateName)) return;
        PlayEquip(Animator.StringToHash(stateName), normalizedStart, fadeTime, duration);
    }

    /// <summary>Hash overload of PlayEquip; stateHash is Animator.StringToHash of the equip state name.</summary>
    public void PlayEquip(int stateHash, float normalizedStart, float fadeTime, float duration)
    {
        if (animator == null || equipLayerIndex < 0) return;

        animator.Play(stateHash, equipLayerIndex, normalizedStart);

        equipSequence?.Kill();
        equipSequence = DOTween.Sequence()
            .Append(DOTween.To(() => equipWeight, ApplyEquipWeight, 1f, fadeTime))
            .AppendInterval(Mathf.Max(0f, duration - fadeTime * 2f))
            .Append(DOTween.To(() => equipWeight, ApplyEquipWeight, 0f, fadeTime))
            .SetLink(gameObject);
    }

    /// <summary>Kills the equip fade and hides the equip layer immediately.</summary>
    public void StopEquip()
    {
        equipSequence?.Kill();
        ApplyEquipWeight(0f);
    }

    private void ApplyCombatBlend(float value)
    {
        combatBlend = value;
        if (animator != null) animator.SetFloat(CombatBlendHash, value);
    }

    private void ApplyEquipWeight(float value)
    {
        equipWeight = value;
        if (animator != null && equipLayerIndex >= 0) animator.SetLayerWeight(equipLayerIndex, value);
    }

    /// <summary>Cross-fades the Guard layer into a state given by its Animator.StringToHash value.</summary>
    public void CrossFadeGuard(int stateHash, float fadeDuration)
    {
        if (animator == null || guardLayerIndex < 0) return;

        animator.CrossFadeInFixedTime(stateHash, fadeDuration, guardLayerIndex, 0f);
    }

    /// <summary>Normalized time of the given action state on the action layer, or -1 when that state is not playing.</summary>
    public float GetActionNormalizedTime(int stateHash)
    {
        if (animator == null || actionLayerIndex < 0) return -1f;

        if (animator.IsInTransition(actionLayerIndex))
        {
            AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(actionLayerIndex);
            return next.shortNameHash == stateHash ? next.normalizedTime : -1f;
        }

        AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(actionLayerIndex);
        return current.shortNameHash == stateHash ? current.normalizedTime : -1f;
    }

    /// <summary>Blends the action layer out to its empty state so locomotion shows through.</summary>
    public void CancelAction(float fadeDuration)
    {
        if (animator == null || actionLayerIndex < 0) return;

        animator.CrossFadeInFixedTime(ActionEmptyStateHash, fadeDuration, actionLayerIndex, 0f);
    }
}
