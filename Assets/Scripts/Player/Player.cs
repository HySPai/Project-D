using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Rigidbody rb;
    public Transform cameraTransform;
    public float moveSpeed = 5f;

    public InputActionReference moveAction;
    public InputActionReference fire;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        fire.action.Enable();
        fire.action.performed += OnFire;
    }

    void OnDisable()
    {
        fire.action.performed -= OnFire;
        fire.action.Disable();
        moveAction.action.Disable();
    }

    void FixedUpdate()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

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
    }

    void OnFire(InputAction.CallbackContext ctx)
    {
        Debug.Log("Pew Pew!");
    }
}
