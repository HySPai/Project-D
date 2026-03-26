using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    public Transform cameraTransform;

    public float moveSpeed = 5f;
    public float rotateSpeed = 12f;

    private Vector2 input;

    private PlayerCombat combat;

    private void Awake()
    {
        combat = GetComponent<PlayerCombat>();
    }

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void SetInput(Vector2 input)
    {
        this.input = input;
    }

    public void Move()
    {
        // ❌ Không cho di chuyển khi đang attack
        if (combat != null && combat.IsAttacking()) return;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = camRight * input.x + camForward * input.y;

        Vector3 delta = move * moveSpeed * Time.fixedDeltaTime;
        Vector3 target = rb.position + delta;

        rb.MovePosition(new Vector3(target.x, rb.position.y, target.z));

        if (move.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            Quaternion smoothRot = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(smoothRot);
        }
    }
}