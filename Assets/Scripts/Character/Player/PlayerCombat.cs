using UnityEngine;

public class PlayerCombat : CharacterCombatBase
{
    private CharacterControllerBase character;

    public void Initialize(CharacterStateBase state, CharacterControllerBase character)
    {
        base.Initialize(state);

        this.character = character;
    }

    public override void Attack()
    {
        if (state == null) return;
        if (state.IsDead) return;

        if (canPerformRollingAttack)
        {
            PlayAttackAnimation(currentWeapon.rollingAttackAnimation, character);
            currentAttackType = AttackType.RollingAttack01;
            DisableCanDoRollingAttack();

            return;
        }

        if (state.IsRunning)
        {
            PlayAttackAnimation(currentWeapon.runningAttackAnimation, character);
            currentAttackType = AttackType.RunningAttack01;

            return;
        }

        if (state.IsAttacking)
        {
            if (!canDoCombo)
                return;

            DisableCanDoCombo();
            RotateTowardsInput();
            AdvanceCombo();
            PlayAttackAnimation(GetCurrentComboAnimation(), character);

            return;
        }

        comboIndex = 0;

        PlayAttackAnimation(GetCurrentComboAnimation(), character);
    }
    private void RotateTowardsInput()
    {
        Vector3 direction = character.GetMovement.GetMoveDirection();

        if (direction.sqrMagnitude < 0.01f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }
    public virtual void DrainStaminaBasedOnAttack()
    {

    }

    public override void EnableCanDoCombo()
    {
        canDoCombo = true;
    }

    public override void DisableCanDoCombo()
    {
        canDoCombo = false;
    }

    public void LockTarget()
    {
        if (lockOnTransform != null)
        {
            ClearLockTarget();
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, lockOnRadius, lockOnLayer);

        float nearestDistance = float.MaxValue;
        Transform nearestTarget = null;

        foreach (Collider hit in hits)
        {
            float sqrDistance = (hit.transform.position - transform.position).sqrMagnitude;

            if (sqrDistance < nearestDistance)
            {
                nearestDistance = sqrDistance;
                nearestTarget = hit.transform;
            }
        }

        SetLockTarget(nearestTarget);
    }
}