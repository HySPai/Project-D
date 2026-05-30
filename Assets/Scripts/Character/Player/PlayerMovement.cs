using UnityEngine;

public class PlayerMovement : CharacterMovementBase
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform cameraTransform;

    private Vector2 input;
    private PlayerState state;

    public void Initialize(PlayerState state)
    {
        this.state = state;
    }

    public override void SetInput(Vector2 input)
    {
        this.input = input;
    }

    public override void Move()
    {
        if (state == null) return;
        if (state.IsDead) return;
        if (state.IsAttacking) return;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = camRight * input.x + camForward * input.y;

        float currentMoveSpeed = state.MoveSpeed;

        if (state.IsRunning)
        {
            currentMoveSpeed *= state.RunMultiplier;
        }

        Vector3 delta = move * currentMoveSpeed * Time.fixedDeltaTime;
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

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            Quaternion smoothRot = Quaternion.Slerp(transform.rotation, targetRot, state.RotateSpeed * Time.fixedDeltaTime);

            rb.MoveRotation(smoothRot);
        }
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