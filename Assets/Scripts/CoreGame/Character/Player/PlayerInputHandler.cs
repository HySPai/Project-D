using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Reads Input System actions into polled values and one-shot requests that PlayerController consumes each frame.</summary>
public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference cameraAction;
    public InputActionReference fire;
    public InputActionReference run_Roll;
    public InputActionReference walk;
    public InputActionReference lockTarget;
    public InputActionReference guard;
    public InputActionReference interact;
    public InputActionReference cast;
    public InputActionReference elementPrev;
    public InputActionReference elementNext;

    [Header("Cast")]
    [SerializeField] private float castHoldThreshold = 0.35f;

    private Vector2 movementInput;
    private Vector2 cameraInput;
    private float horizontalInput;
    private float verticalInput;
    private float moveAmount;

    private bool isFire;
    private bool isGuardHeld;
    private bool isGuardPressed;
    private bool isInteractPressed;
    private bool isWalkPressed;
    private bool isRunHeld;
    private bool rollRequested;
    private bool isLockTargetRequested;

    private bool castTapRequested;
    private bool castHoldRequested;
    private int elementSwitchStep;
    private float castStartTime;
    private bool castHeld;

    [Header("Lock Switch")]
    [SerializeField] private float lockSwitchThreshold = 0.5f;
    [SerializeField] private float lockSwitchReleaseThreshold = 0.2f;
    private bool lockSwitchReady = true;

    private PlayerState state;

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
        run_Roll.action.Enable();
        walk.action.Enable();
        lockTarget.action.Enable();

        if (guard != null)
        {
            guard.action.Enable();
            guard.action.started += OnGuardStarted;
            guard.action.canceled += OnGuardCanceled;
        }

        if (interact != null)
        {
            interact.action.Enable();
            interact.action.started += OnInteractStarted;
        }

        if (cast != null)
        {
            cast.action.Enable();
            cast.action.started += OnCastStarted;
            cast.action.canceled += OnCastCanceled;
        }

        if (elementPrev != null)
        {
            elementPrev.action.Enable();
            elementPrev.action.started += OnElementPrevStarted;
        }

        if (elementNext != null)
        {
            elementNext.action.Enable();
            elementNext.action.started += OnElementNextStarted;
        }

        moveAction.action.performed += OnMovePerformed;
        moveAction.action.canceled += OnMoveCanceled;

        cameraAction.action.performed += OnCameraPerformed;
        cameraAction.action.canceled += OnCameraCanceled;

        fire.action.performed += OnFirePerformed;

        run_Roll.action.started += OnRunStarted;
        run_Roll.action.canceled += OnRunCanceled;

        walk.action.started += OnWalkStarted;
        walk.action.canceled += OnWalkCanceled;

        lockTarget.action.started += OnLockTargetStarted;
    }

    private void OnDisable()
    {
        moveAction.action.performed -= OnMovePerformed;
        moveAction.action.canceled -= OnMoveCanceled;

        cameraAction.action.performed -= OnCameraPerformed;
        cameraAction.action.canceled -= OnCameraCanceled;

        fire.action.performed -= OnFirePerformed;

        if (guard != null)
        {
            guard.action.started -= OnGuardStarted;
            guard.action.canceled -= OnGuardCanceled;
            guard.action.Disable();
        }

        if (interact != null)
        {
            interact.action.started -= OnInteractStarted;
            interact.action.Disable();
        }

        if (cast != null)
        {
            cast.action.started -= OnCastStarted;
            cast.action.canceled -= OnCastCanceled;
            cast.action.Disable();
        }

        if (elementPrev != null)
        {
            elementPrev.action.started -= OnElementPrevStarted;
            elementPrev.action.Disable();
        }

        if (elementNext != null)
        {
            elementNext.action.started -= OnElementNextStarted;
            elementNext.action.Disable();
        }

        run_Roll.action.started -= OnRunStarted;
        run_Roll.action.canceled -= OnRunCanceled;

        walk.action.started -= OnWalkStarted;
        walk.action.canceled -= OnWalkCanceled;

        lockTarget.action.started -= OnLockTargetStarted;

        lockTarget.action.Disable();
        moveAction.action.Disable();
        cameraAction.action.Disable();
        fire.action.Disable();
        run_Roll.action.Disable();
        walk.action.Disable();
    }

    private void Update()
    {
        HandleMovementInput();
        HandleCameraInput();
    }

    private void HandleMovementInput()
    {
        state.SetCurrentInput(movementInput);

        verticalInput = state.CurrentInput.y;
        horizontalInput = state.CurrentInput.x;

        moveAmount = Mathf.Clamp01(state.CurrentInput.magnitude);
    }

    private void HandleCameraInput()
    {
        state.SetCurrentCameraInput(Vector2.MoveTowards(state.CurrentCameraInput, cameraInput, state.MoveSmoothSpeed * Time.deltaTime));
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

    private void OnGuardStarted(InputAction.CallbackContext ctx)
    {
        isGuardHeld = true;
        isGuardPressed = true;
    }

    private void OnGuardCanceled(InputAction.CallbackContext ctx)
    {
        isGuardHeld = false;
    }

    private void OnInteractStarted(InputAction.CallbackContext ctx)
    {
        isInteractPressed = true;
    }

    private void OnCastStarted(InputAction.CallbackContext ctx)
    {
        castHeld = true;
        castStartTime = Time.time;
    }

    private void OnCastCanceled(InputAction.CallbackContext ctx)
    {
        if (!castHeld) return;

        castHeld = false;
        if (Time.time - castStartTime >= castHoldThreshold)
            castHoldRequested = true;
        else
            castTapRequested = true;
    }

    private void OnElementPrevStarted(InputAction.CallbackContext ctx)
    {
        elementSwitchStep = -1;
    }

    private void OnElementNextStarted(InputAction.CallbackContext ctx)
    {
        elementSwitchStep = 1;
    }

    public bool ConsumeCastTap()
    {
        if (!castTapRequested)
            return false;

        castTapRequested = false;
        return true;
    }

    public bool ConsumeCastHold()
    {
        if (!castHoldRequested)
            return false;

        castHoldRequested = false;
        return true;
    }

    public int ConsumeElementSwitch()
    {
        int step = elementSwitchStep;
        elementSwitchStep = 0;
        return step;
    }

    public void DiscardBufferedActions()
    {
        castHeld = false;
        castTapRequested = false;
        castHoldRequested = false;
        elementSwitchStep = 0;
        isFire = false;
        isGuardPressed = false;
        isInteractPressed = false;
        rollRequested = false;
        isLockTargetRequested = false;
    }

    public bool ConsumeInteractPressed()
    {
        if (!isInteractPressed)
            return false;

        isInteractPressed = false;
        return true;
    }

    private void OnRunStarted(InputAction.CallbackContext ctx)
    {
        isRunHeld = true;
        rollRequested = true;
    }

    private void OnRunCanceled(InputAction.CallbackContext ctx)
    {
        isRunHeld = false;
    }

    private void OnWalkStarted(InputAction.CallbackContext ctx)
    {
        isWalkPressed = true;
    }

    private void OnWalkCanceled(InputAction.CallbackContext ctx)
    {
        isWalkPressed = false;
    }
    private void OnLockTargetStarted(InputAction.CallbackContext ctx)
    {
        isLockTargetRequested = true;
    }
    public bool IsRunning()
    {
        return isRunHeld;
    }

    public bool IsWalking()
    {
        return isWalkPressed;
    }

    public bool ConsumeFirePressed()
    {
        if (!isFire)
            return false;

        isFire = false;
        return true;
    }

    public bool IsGuardHeld()
    {
        return isGuardHeld;
    }

    public bool ConsumeGuardPressed()
    {
        if (!isGuardPressed)
            return false;

        isGuardPressed = false;
        return true;
    }

    /// <summary>Consumes a pending roll request from the run/roll button press.</summary>
    public bool IsRolling()
    {
        if (!rollRequested)
            return false;

        rollRequested = false;
        return true;
    }
    /// <summary>Consumes a pending lock-on toggle request.</summary>
    public bool IsLockTarget()
    {
        if (!isLockTargetRequested)
            return false;

        isLockTargetRequested = false;
        return true;
    }
    /// <summary>Normalized raw camera stick once per flick past lockSwitchThreshold; zero until it returns under lockSwitchReleaseThreshold.</summary>
    public Vector2 GetLockSwitchInput()
    {
        Vector2 raw = cameraInput;
        float magnitude = raw.magnitude;

        if (magnitude < lockSwitchReleaseThreshold)
            lockSwitchReady = true;

        if (lockSwitchReady && magnitude >= lockSwitchThreshold)
        {
            lockSwitchReady = false;
            return raw / magnitude;
        }

        return Vector2.zero;
    }
}