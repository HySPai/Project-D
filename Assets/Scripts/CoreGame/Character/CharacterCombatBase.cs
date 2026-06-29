using UnityEngine;

public abstract class CharacterCombatBase : MonoBehaviour
{
    protected CharacterStateBase state;

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

    public virtual void Initialize(CharacterStateBase state)
    {
        this.state = state;

        // Khởi tạo TẤT CẢ damage collider (gán state để biết ai là chủ đòn đánh).
        if (damageColliders != null)
        {
            for (int i = 0; i < damageColliders.Length; i++)
                if (damageColliders[i] != null)
                    damageColliders[i].Initialize(state);
        }
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

    public abstract void Attack();

    public void EnableCanDoRollingAttack() => canPerformRollingAttack = true;
    public void DisableCanDoRollingAttack() => canPerformRollingAttack = false;

    public virtual void EnableCanDoCombo() => canDoCombo = true;
    public virtual void DisableCanDoCombo() => canDoCombo = false;

    public virtual void EnableCanRotate() => state?.SetCanRotate(true);
    public virtual void DisableCanRotate() => state?.SetCanRotate(false);

    #region Damage Colliders
    // Lấy collider theo chỉ số (null nếu chỉ số không hợp lệ).
    protected DamageCollider GetDamageCollider(int index)
    {
        if (damageColliders == null || index < 0 || index >= damageColliders.Length)
            return null;
        return damageColliders[index];
    }

    // Đặt sát thương cho 1 collider cụ thể trước khi mở (dùng cho enemy nhiều đòn).
    public void SetDamage(int index, float damage)
    {
        DamageCollider dc = GetDamageCollider(index);
        if (dc != null) dc.SetDamage(damage);
    }

    // ---- Animation event (giữ nguyên cho đòn đánh đơn: collider [0]) ----
    public virtual void OpenDamageCollider() => OpenDamageColliderAt(0);
    public virtual void CloseDamageCollider() => CloseDamageColliderAt(0);

    // ---- Animation event có chỉ số (cho đòn đánh nhiều collider) ----
    public virtual void OpenDamageColliderAt(int index)
    {
        DamageCollider dc = GetDamageCollider(index);
        if (dc != null && dc.GetCollider != null)
            dc.GetCollider.enabled = true;
    }

    public virtual void CloseDamageColliderAt(int index)
    {
        DamageCollider dc = GetDamageCollider(index);
        if (dc != null && dc.GetCollider != null)
            dc.GetCollider.enabled = false;
    }

    // Tắt hết collider — gọi khi chết, hoặc khi cần dập đòn khẩn cấp.
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

        // Đòn đánh đơn -> sát thương đặt vào collider mặc định [0].
        SetDamage(0, attack.damage);
        PlayAttackAnimation(attack.animationName, character);
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