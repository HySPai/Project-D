using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public InputActionReference moveAction;
    public InputActionReference fire;
    public InputActionReference run_Dash;

    private Vector2 moveInput;
    private bool isFire;
    private bool isRunPressed;
    private bool isDash;
    private float runPressedTime;

    private void OnEnable()
    {
        moveAction.action.Enable();
        fire.action.Enable();
        run_Dash.action.Enable();

        fire.action.performed += ctx => isFire = true;

        run_Dash.action.started += OnRunStarted;
        run_Dash.action.canceled += OnRunCanceled;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        fire.action.Disable();
        run_Dash.action.Disable();

        run_Dash.action.started -= OnRunStarted;
        run_Dash.action.canceled -= OnRunCanceled;
    }

    private void Update()
    {
        moveInput = moveAction.action.ReadValue<Vector2>();
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    public bool IsFire()
    {
        if (isFire)
        {
            isFire = false;
            return true;
        }
        return false;
    }
    private void OnRunStarted(InputAction.CallbackContext ctx)
    {
        runPressedTime = Time.time;
        isRunPressed = true;
    }

    private void OnRunCanceled(InputAction.CallbackContext ctx)
    {
        float holdTime = Time.time - runPressedTime;

        if (holdTime < 0.2f)
        {
            isDash = true;
        }

        isRunPressed = false;
    }
    public bool IsRunning()
    {
        return isRunPressed;
    }

    public bool IsDash()
    {
        if (isDash)
        {
            isDash = false;
            return true;
        }

        return false;
    }
}