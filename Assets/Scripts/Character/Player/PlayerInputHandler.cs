using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference cameraAction;
    public InputActionReference fire;
    public InputActionReference run_Dash;
    public InputActionReference walk;

    [Header("Movement Input")]
    [SerializeField] private Vector2 movementInput;
    [SerializeField] private Vector2 cameraInput;

    [SerializeField] private float horizontalInput;
    [SerializeField] private float verticalInput;
    [SerializeField] private float moveAmount;

    [Header("Action Input")]
    [SerializeField] private bool isFire;
    [SerializeField] private bool isRunPressed;
    [SerializeField] private bool isWalkPressed;
    [SerializeField] private bool isDash;

    private PlayerState state;
    private float runPressedTime;

    public float HorizontalInput => horizontalInput;
    public float VerticalInput => verticalInput;
    public float MoveAmount => moveAmount;

    public void Initialize(PlayerState state)
    {
        this.state = state;
    }

    public Vector2 GetMoveInput()
    {
        return movementInput;
    }

    public Vector2 GetCameraInput()
    {
        return cameraInput;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        cameraAction.action.Enable();
        fire.action.Enable();
        run_Dash.action.Enable();
        walk.action.Enable();

        moveAction.action.performed += OnMovePerformed;
        moveAction.action.canceled += OnMoveCanceled;

        cameraAction.action.performed += OnCameraPerformed;
        cameraAction.action.canceled += OnCameraCanceled;

        fire.action.performed += OnFirePerformed;

        run_Dash.action.started += OnRunStarted;
        run_Dash.action.canceled += OnRunCanceled;

        walk.action.started += OnWalkStarted;
        walk.action.canceled += OnWalkCanceled;
    }

    private void OnDisable()
    {
        moveAction.action.performed -= OnMovePerformed;
        moveAction.action.canceled -= OnMoveCanceled;

        cameraAction.action.performed -= OnCameraPerformed;
        cameraAction.action.canceled -= OnCameraCanceled;

        fire.action.performed -= OnFirePerformed;

        run_Dash.action.started -= OnRunStarted;
        run_Dash.action.canceled -= OnRunCanceled;

        walk.action.started -= OnWalkStarted;
        walk.action.canceled -= OnWalkCanceled;

        moveAction.action.Disable();
        cameraAction.action.Disable();
        fire.action.Disable();
        run_Dash.action.Disable();
        walk.action.Disable();
    }

    private void Update()
    {
        HandleMovementInput();
        HandleCameraInput();
    }

    private void HandleMovementInput()
    {
        state.SetCurrentInput(
            Vector2.MoveTowards(
                state.CurrentInput,
                movementInput,
                state.MoveSmoothSpeed * Time.deltaTime));

        verticalInput = state.CurrentInput.y;
        horizontalInput = state.CurrentInput.x;

        moveAmount = Mathf.Clamp01(state.CurrentInput.magnitude);
    }

    private void HandleCameraInput()
    {
        state.SetCurrentCameraInput(
            Vector2.MoveTowards(
                state.CurrentCameraInput,
                cameraInput,
                state.MoveSmoothSpeed * Time.deltaTime));
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        movementInput = Vector2.zero;
    }

    private void OnCameraPerformed(InputAction.CallbackContext ctx)
    {
        cameraInput = ctx.ReadValue<Vector2>();
    }

    private void OnCameraCanceled(InputAction.CallbackContext ctx)
    {
        cameraInput = Vector2.zero;
    }

    private void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        isFire = true;
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

    private void OnWalkStarted(InputAction.CallbackContext ctx)
    {
        isWalkPressed = true;
    }

    private void OnWalkCanceled(InputAction.CallbackContext ctx)
    {
        isWalkPressed = false;
    }

    public bool IsRunning()
    {
        return isRunPressed;
    }

    public bool IsWalking()
    {
        return isWalkPressed;
    }

    public bool IsFire()
    {
        if (!isFire)
            return false;

        isFire = false;
        return true;
    }

    public bool IsDash()
    {
        if (!isDash)
            return false;

        isDash = false;
        return true;
    }
}