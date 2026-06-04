using UnityEngine;

public class PlayerMovement : CharacterMovementBase
{
    private Rigidbody rb;
    private Vector2 input;
    private PlayerState state;
    private PlayerAnimation anim;
    private PlayerCombat combat;
    private PlayerCamera playerCamera;

    [SerializeField] private string rollAction = "Roll_Forward_01";

    public void Initialize(PlayerState state, PlayerAnimation animation, PlayerCombat combat, PlayerCamera playerCamera)
    {
        this.state = state;
        this.anim = animation;
        this.combat = combat;
        this.playerCamera = playerCamera;
        rb = state.Owner.Rigidbody;
    }

    public override void SetInput(Vector2 input)
    {
        this.input = input;
    }

    public override void Move()
    {
        if (state == null) return;
        if (state.IsDead) return;
        if (!state.CanMove) return;

        Vector3 move = playerCamera.GetMoveDirection(input);

        float moveAmount = Mathf.Clamp01(input.magnitude / state.FullSpeedInputThreshold);

        if (move.sqrMagnitude > 0.001f)
            move.Normalize();

        UpdateMoveSpeed(moveAmount);

        Vector3 delta = move * state.CurrentMoveSpeed * Time.fixedDeltaTime;

        Vector3 currentPos = rb.position;

        Vector3 xTarget = currentPos + new Vector3(delta.x, 0f, 0f);
        Vector3 zTarget = currentPos + new Vector3(0f, 0f, delta.z);

        float finalX = currentPos.x;
        float finalZ = currentPos.z;

        if (IsGroundValid(xTarget))
        {
            finalX = xTarget.x;
        }

        if (IsGroundValid(zTarget))
        {
            finalZ = zTarget.z;
        }

        Vector3 finalPosition = new Vector3(finalX, currentPos.y, finalZ);

        rb.MovePosition(finalPosition);

        if (state.CanRotate)
        {
            if (combat.LockOnTransform != null)
            {
                Vector3 direction = combat.LockOnTransform.position - transform.position;

                direction.y = 0f;

                if (direction.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(direction);

                    Quaternion smoothRot = Quaternion.Slerp(transform.rotation, targetRot, state.RotateSpeed * Time.fixedDeltaTime);

                    rb.MoveRotation(smoothRot);
                }
            }
            else if (move.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(move);

                Quaternion smoothRot = Quaternion.Slerp(transform.rotation, targetRot, state.RotateSpeed * Time.fixedDeltaTime);

                rb.MoveRotation(smoothRot);
            }
        }
    }
    private void UpdateMoveSpeed(float moveAmount)
    {
        float targetSpeed = 0f;

        if (moveAmount > 0f)
        {
            if (state.IsRunning)
            {
                targetSpeed = state.RunSpeed;
            }
            else
            {
                targetSpeed = state.MoveSpeed * moveAmount;
            }
        }

        state.SetTargetMoveSpeed(targetSpeed);

        float smoothSpeed = state.MoveSmoothSpeed * state.MoveSpeed;

        if (moveAmount <= 0f)
        {
            state.SetCurrentMoveSpeed(0f);
        }
        else
        {
            state.SetCurrentMoveSpeed(Mathf.MoveTowards(state.CurrentMoveSpeed,state.TargetMoveSpeed,smoothSpeed * Time.fixedDeltaTime));
        }
    }
    public override void Roll()
    {
        if (state == null)
            return;

        if (state.IsDead)
            return;

        if (state.IsRolling)
            return;

        if (state.IsAttacking)
            return;

        Vector3 moveDirection = playerCamera.GetMoveDirection(input);

        if (moveDirection.sqrMagnitude <= 0.01f)
            moveDirection = transform.forward;

        moveDirection.Normalize();

        if (moveDirection.sqrMagnitude <= 0.01f)
        {
            moveDirection = transform.forward;
        }

        moveDirection.Normalize();

        transform.rotation = Quaternion.LookRotation(moveDirection);

        state.SetRolling(true);

        anim.PlayTargetAnimation(rollAction, true, true, false, false);
    }
    public override Vector3 GetMoveDirection()
    {
        return playerCamera.GetMoveDirection(input).normalized;
    }
    private bool IsGroundValid(Vector3 position)
    {
        Vector3 center = position + Vector3.up * 0.2f;

        if (!Physics.Raycast(
            center,
            Vector3.down,
            out RaycastHit hit,
            state.GroundCheckDistance,
            state.GroundLayer))
        {
            return false;
        }

        float heightDiff = rb.position.y - hit.point.y;
        return heightDiff <= state.MaxStepDownHeight;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (rb == null || state == null)
            return;

        Vector3 center = rb.position + Vector3.up * 0.2f;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center, center + Vector3.down * state.GroundCheckDistance);
    }
#endif
}