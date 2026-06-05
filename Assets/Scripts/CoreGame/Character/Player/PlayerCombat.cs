using UnityEngine;
using UnityEngine.Playables;

public class PlayerCombat : CharacterCombatBase
{
    private CharacterControllerBase character;
    private PlayerCamera playerCamera;
    private AttackData currentAttack;
    private PlayerState playerState;

    public void Initialize(CharacterStateBase state, CharacterControllerBase character, PlayerCamera playerCamera)
    {
        base.Initialize(state);

        this.playerState = state as PlayerState;
        this.character = character;
        this.playerCamera = playerCamera;
    }

    public override void Attack()
    {
        if (state == null) return;
        if (state.IsDead) return;
        if (playerState != null && !playerState.HasStamina) return;

        if (canPerformRollingAttack)
        {
            DoAttack(currentWeapon.rollingAttack);
            currentAttackType = AttackType.RollingAttack01;
            DisableCanDoRollingAttack();
            return;
        }

        if (state.IsRunning)
        {
            DoAttack(currentWeapon.runningAttack);
            currentAttackType = AttackType.RunningAttack01;
            return;
        }

        if (state.IsAttacking)
        {
            if (!canDoCombo) return;

            DisableCanDoCombo();
            RotateTowardsInput();
            AdvanceCombo();
            DoAttack(GetCurrentComboAttack());
            return;
        }

        comboIndex = 0;
        DoAttack(GetCurrentComboAttack());
    }

    private void DoAttack(AttackData attack)
    {
        if (attack == null) return;

        currentAttack = attack;
        PerformAttack(attack, character);
    }

    public void DrainStaminaBasedOnAttack()
    {
        if (playerState == null || currentAttack == null) return;

        playerState.DrainStamina(currentAttack.stamina);
    }

    private void RotateTowardsInput()
    {
        Vector3 direction = character.GetMovement.GetMoveDirection();

        if (direction.sqrMagnitude < 0.01f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
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

        Collider[] hits = Physics.OverlapSphere(transform.position, BroadphaseRange, lockOnLayer);

        float nearestDistance = float.MaxValue;
        Transform nearestTarget = null;

        foreach (Collider hit in hits)
        {
            // Chỉ lock kẻ địch đang nằm trong tầm nhìn camera
            if (!IsInView(hit.transform.position)) continue;

            float sqrDistance = (hit.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance < nearestDistance)
            {
                nearestDistance = sqrDistance;
                nearestTarget = hit.transform;
            }
        }

        SetLockTarget(nearestTarget);
    }

    public void SwitchTarget(Vector2 inputDir)
    {
        if (lockOnTransform == null || inputDir.sqrMagnitude < 0.01f) return;

        Vector3 camRight = playerCamera.Right;
        Vector3 camForward = playerCamera.Forward;
        Vector3 playerPos = transform.position;

        Vector2 currentScreen = ToScreenPlane(lockOnTransform.position - playerPos, camRight, camForward);

        Collider[] hits = Physics.OverlapSphere(playerPos, BroadphaseRange, lockOnLayer);

        Transform best = null;
        float bestScore = float.MinValue;

        Vector2 desired2D = inputDir.normalized;

        foreach (Collider hit in hits)
        {
            Transform t = hit.transform;
            if (t == lockOnTransform) continue;

            // Chỉ xét kẻ địch trong tầm nhìn camera
            if (!IsInView(t.position)) continue;

            Vector2 candidateScreen = ToScreenPlane(t.position - playerPos, camRight, camForward);

            Vector2 delta = candidateScreen - currentScreen;
            if (delta.sqrMagnitude < 0.0001f) continue;

            Vector2 deltaDir = delta.normalized;

            float alignment = Vector2.Dot(deltaDir, desired2D);
            if (alignment <= 0f) continue;

            float score = alignment / delta.magnitude;

            if (score > bestScore)
            {
                bestScore = score;
                best = t;
            }
        }

        if (best != null)
            SetLockTarget(best);
    }
    
    private Vector2 ToScreenPlane(Vector3 worldDir, Vector3 camRight, Vector3 camForward)
    {
        float x = Vector3.Dot(worldDir, camRight);
        float y = Vector3.Dot(worldDir, camForward);
        return new Vector2(x, y);
    }
}