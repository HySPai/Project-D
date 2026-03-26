using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public InputActionReference moveAction;
    public InputActionReference fire;

    private Vector2 moveInput;
    private bool isFire;

    private void OnEnable()
    {
        moveAction.action.Enable();
        fire.action.Enable();

        fire.action.performed += ctx => isFire = true;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        fire.action.Disable();
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
}