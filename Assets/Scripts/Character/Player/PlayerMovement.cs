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

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = camRight * input.x + camForward * input.y;

        Vector3 delta = move * state.MoveSpeed * Time.fixedDeltaTime;
        Vector3 target = rb.position + delta;

        rb.MovePosition(new Vector3(target.x, rb.position.y, target.z));

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);

            Quaternion smoothRot = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                state.RotateSpeed * Time.fixedDeltaTime);

            rb.MoveRotation(smoothRot);
        }
    }
}