using System;
using UnityEngine;

/// <summary>Shared combat state: damage colliders, combo index, attack flags and the lock-on target.</summary>
public abstract class CharacterCombatBase : MonoBehaviour
{
    protected CharacterStateBase state;

    public event Action<CharacterStateBase, DamageResult> OnHitLanded;

    [Header("Damage Colliders")]
    [SerializeField] protected DamageCollider[] damageColliders;

    [Header("Attack Type")]
    public AttackType currentAttackType;
    [Header("Last Attack Animation Performed")]
    public string lastAttackAnimationPerformed;
    [Header("Lock On Transform")]
    public Transform lockOnTransform;
    [Header("Attack Flags")]
    public bool canPerformRollingAttack = false;
    public bool canPerformBackstepAttack = false;
    public bool canBlock = true;
    [Header("Combo Flags")]
    [SerializeField] protected bool canDoCombo;
    [SerializeField] protected float comboResetDelay = 2f;
    protected float lastAttackTime;
    [Header("Current Weapon")]
    [SerializeField] protected WeaponItem currentWeapon;
    [SerializeField] protected int comboIndex;
    [Header("Lock On")]
    [SerializeField] protected LayerMask lockOnLayer;

    /// <summary>Physics radius used only to gather lock-on candidates; the real lock limit is the camera view.</summary>
    protected const float BroadphaseRange = 50f;

    private CharacterStateBase lockedTargetState;

    public Transform LockOnTransform => lockOnTransform;
    protected CharacterStateBase LockedTargetState => lockedTargetState;
    public bool CanDoCombo => canDoCombo;

    public virtual void Initialize(CharacterStateBase state)
    {
        this.state = state;

        if (damageColliders == null) return;

        for (int i = 0; i < damageColliders.Length; i++)
            if (damageColliders[i] != null)
                damageColliders[i].Initialize(state);
    }

    public abstract void Attack();

    public void EnableCanDoRollingAttack() => canPerformRollingAttack = true;
    public void DisableCanDoRollingAttack() => canPerformRollingAttack = false;

    public void EnableCanDoBackstepAttack() => canPerformBackstepAttack = true;
    public void DisableCanDoBackstepAttack() => canPerformBackstepAttack = false;

    public virtual void EnableCanDoCombo() => canDoCombo = true;
    public virtual void DisableCanDoCombo() => canDoCombo = false;

    public virtual void EnableCanRotate() => state?.SetCanRotate(true);
    public virtual void DisableCanRotate() => state?.SetCanRotate(false);

    #region Damage Colliders
    protected DamageCollider GetDamageCollider(int index)
    {
        if (damageColliders == null || index < 0 || index >= damageColliders.Length)
            return null;
        return damageColliders[index];
    }

    public void SetDamage(
        int index,
        float points,
        DamageSource source,
        ElementType element = ElementType.None,
        bool unblockable = false,
        float staggerAmount = 0f)
    {
        DamageCollider dc = GetDamageCollider(index);
        if (dc != null) dc.SetDamage(points, source, element, unblockable, staggerAmount);
    }

    public void RaiseHitLanded(CharacterStateBase target, DamageResult result)
    {
        OnHitLanded?.Invoke(target, result);
    }

    /// <summary>Animation event for single-collider attacks: opens collider 0.</summary>
    public virtual void OpenDamageCollider() => OpenDamageColliderAt(0);

    /// <summary>Animation event for single-collider attacks: closes collider 0.</summary>
    public virtual void CloseDamageCollider() => CloseDamageColliderAt(0);

    /// <summary>Animation event for multi-collider attacks.</summary>
    public virtual void OpenDamageColliderAt(int index)
    {
        DamageCollider dc = GetDamageCollider(index);
        if (dc != null && dc.GetCollider != null)
            dc.GetCollider.enabled = true;
    }

    /// <summary>Animation event for multi-collider attacks.</summary>
    public virtual void CloseDamageColliderAt(int index)
    {
        DamageCollider dc = GetDamageCollider(index);
        if (dc != null && dc.GetCollider != null)
            dc.GetCollider.enabled = false;
    }

    /// <summary>Closes every damage collider, e.g. on death or when an attack is interrupted.</summary>
    public virtual void DisableAllDamageColliders()
    {
        if (damageColliders == null) return;
        for (int i = 0; i < damageColliders.Length; i++)
        {
            DamageCollider dc = damageColliders[i];
            if (dc != null && dc.GetCollider != null)
                dc.GetCollider.enabled = false;
        }
    }
    #endregion

    public virtual void AdvanceCombo()
    {
        if (currentWeapon == null || currentWeapon.lightAttacks == null) return;
        comboIndex++;
        comboIndex = Mathf.Clamp(comboIndex, 0, currentWeapon.lightAttacks.Length - 1);
    }

    protected virtual AttackData GetCurrentComboAttack()
    {
        if (currentWeapon == null || currentWeapon.lightAttacks == null || currentWeapon.lightAttacks.Length == 0)
            return null;
        comboIndex = Mathf.Clamp(comboIndex, 0, currentWeapon.lightAttacks.Length - 1);
        return currentWeapon.lightAttacks[comboIndex];
    }

    protected void PerformAttack(AttackData attack, CharacterControllerBase character)
    {
        if (attack == null || string.IsNullOrEmpty(attack.animationName)) return;

        SetDamage(0, attack.damage, DamageSource.Weapon);
        PlayAttackAnimation(attack.animationName, attack.AnimationHash, character);
    }

    public virtual void ResetCombo() => comboIndex = 0;

    /// <summary>Drops every in-flight attack, combo window and lock-on.</summary>
    public virtual void ResetCombat()
    {
        DisableAllDamageColliders();
        canDoCombo = false;
        canPerformRollingAttack = false;
        canPerformBackstepAttack = false;
        comboIndex = 0;
        ClearLockTarget();
    }

    protected void PlayAttackAnimation(string animationName, CharacterControllerBase character)
    {
        PlayAttackAnimation(animationName, Animator.StringToHash(animationName), character);
    }

    protected void PlayAttackAnimation(string animationName, int animationHash, CharacterControllerBase character)
    {
        state.SetAttacking(true);
        state.SetCanMove(false);
        state.SetCanRotate(false);
        state.SetApplyRootMotion(true);
        lastAttackAnimationPerformed = animationName;
        character.GetAnimation.PlayTargetAnimation(animationHash, true);
    }

    /// <summary>Locks onto target; targetState lets the lock drop itself when that character dies.</summary>
    public virtual void SetLockTarget(Transform target, CharacterStateBase targetState)
    {
        lockOnTransform = target;
        lockedTargetState = target != null ? targetState : null;
    }

    public virtual void ClearLockTarget()
    {
        lockOnTransform = null;
        lockedTargetState = null;
    }
}
