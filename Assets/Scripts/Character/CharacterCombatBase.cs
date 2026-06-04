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
    [SerializeField] protected LayerMask lockOnLayer;

    // Broadphase nội bộ chỉ để lấy candidate từ physics — KHÔNG phải lock/unlock radius.
    // Điều kiện lock/unlock thực sự là viewport (IsInView).
    protected const float BroadphaseRange = 50f;

    private CharacterStateBase _lockedTargetState;
    protected Camera viewCamera;

    public Transform LockOnTransform => lockOnTransform;
    public bool CanDoCombo => canDoCombo;

    protected virtual void Awake()
    {
        viewCamera = Camera.main;
    }

    protected virtual void Update()
    {
        CheckAutoUnlock();
    }

    private void CheckAutoUnlock()
    {
        if (lockOnTransform == null) return;

        if (_lockedTargetState != null && _lockedTargetState.IsDead)
        {
            ClearLockTarget();
            return;
        }

        if (!IsInView(lockOnTransform.position))
        {
            ClearLockTarget();
        }
    }

    // Kiểm tra một điểm world có nằm trong khung nhìn camera không
    protected bool IsInView(Vector3 worldPos)
    {
        if (viewCamera == null)
        {
            viewCamera = Camera.main;
            if (viewCamera == null) return false;
        }

        Vector3 vp = viewCamera.WorldToViewportPoint(worldPos);

        return vp.z > 0f
            && vp.x >= 0f && vp.x <= 1f
            && vp.y >= 0f && vp.y <= 1f;
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