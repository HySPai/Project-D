using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Rigidbody rb;
    public Transform cameraTransform;
    public Animator animator;
    public float moveSpeed = 5f;
    public float rotateSpeed = 12f;
    public float animSmooth = 8f;

    public InputActionReference moveAction;
    public InputActionReference fire;

    Vector2 inputCache;
    float animMoveValue;

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

    void Update()
    {
        inputCache = moveAction.action.ReadValue<Vector2>();

        float inputMagnitude = Mathf.Clamp01(inputCache.magnitude);

        float deadZone = 0.05f;
        float midThreshold = 0.2f;

        float target;
        if (inputMagnitude <= deadZone)
        {
            target = 0f;
        }
        else if (inputMagnitude < midThreshold)
        {
            target = Mathf.Lerp(0f, 0.5f, (inputMagnitude - deadZone) / (midThreshold - deadZone));
        }
        else
        {
            target = Mathf.Lerp(0.5f, 1f, (inputMagnitude - midThreshold) / (1f - midThreshold));
        }

        animMoveValue = Mathf.Lerp(animMoveValue, target, animSmooth * Time.deltaTime);
        animator.SetFloat("Move", animMoveValue);
    }

    void FixedUpdate()
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = camRight * inputCache.x + camForward * inputCache.y;

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

    void OnFire(InputAction.CallbackContext ctx)
    {
        Debug.Log("Pew Pew!");
    }
}
