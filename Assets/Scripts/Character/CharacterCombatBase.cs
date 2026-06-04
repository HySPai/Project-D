using UnityEngine;

public abstract class CharacterCombatBase : MonoBehaviour
{
    protected CharacterStateBase state;
    [SerializeField] protected Collider colAttack;

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
    [SerializeField] protected float lockOnRadius = 10f;
    [SerializeField] protected float unlockRadius = 15f;
    [SerializeField] protected LayerMask lockOnLayer;

    // Cache state của target để check IsDead
    private CharacterStateBase _lockedTargetState;

    // sqrMagnitude để tránh Sqrt mỗi frame
    private float _unlockRadiusSqr;

    public Transform LockOnTransform => lockOnTransform;
    public bool CanDoCombo => canDoCombo;

    protected virtual void Awake()
    {
        _unlockRadiusSqr = unlockRadius * unlockRadius;
    }

    protected virtual void Update()
    {
        CheckAutoUnlock();
    }

    private void CheckAutoUnlock()
    {
        if (lockOnTransform == null) return;

        // 1. Kẻ địch chết
        if (_lockedTargetState != null && _lockedTargetState.IsDead)
        {
            ClearLockTarget();
            return;
        }

        // 2. Vượt quá unlockRadius
        float sqrDist = (lockOnTransform.position - transform.position).sqrMagnitude;
        if (sqrDist > _unlockRadiusSqr)
        {
            ClearLockTarget();
        }
    }

    public virtual void Initialize(CharacterStateBase state)
    {
        this.state = state;
    }

    public abstract void Attack();

    public void EnableCanDoRollingAttack() => canPerformRollingAttack = true;
    public void DisableCanDoRollingAttack() => canPerformRollingAttack = false;

    public virtual void EnableCanDoCombo() => canDoCombo = true;
    public virtual void DisableCanDoCombo() => canDoCombo = false;

    public virtual void EnableCanRotate() => state?.SetCanRotate(true);
    public virtual void DisableCanRotate() => state?.SetCanRotate(false);

    public virtual void OpenDamageCollider() => colAttack.enabled = true;
    public virtual void CloseDamageCollider() => colAttack.enabled = false;

    public virtual void AdvanceCombo()
    {
        if (currentWeapon == null) return;
        comboIndex++;
        comboIndex = Mathf.Clamp(comboIndex, 0, currentWeapon.lightAttackAnimations.Length - 1);
    }

    protected virtual string GetCurrentComboAnimation()
    {
        if (currentWeapon == null || currentWeapon.lightAttackAnimations == null)
            return string.Empty;
        comboIndex = Mathf.Clamp(comboIndex, 0, currentWeapon.lightAttackAnimations.Length - 1);
        return currentWeapon.lightAttackAnimations[comboIndex];
    }

    public virtual void ResetCombo() => comboIndex = 0;

    protected void PlayAttackAnimation(string animationName, CharacterControllerBase character)
    {
        state.SetAttacking(true);
        state.SetCanMove(false);
        state.SetCanRotate(false);
        state.SetApplyRootMotion(true);
        lastAttackAnimationPerformed = animationName;
        character.GetAnimation.PlayTargetAnimation(animationName, true);
    }

    public virtual void SetLockTarget(Transform target)
    {
        lockOnTransform = target;

        _lockedTargetState = target != null
            ? target.GetComponentInParent<CharacterStateBase>()
            : null;
    }

    public virtual void ClearLockTarget()
    {
        lockOnTransform = null;
        _lockedTargetState = null;
    }
}