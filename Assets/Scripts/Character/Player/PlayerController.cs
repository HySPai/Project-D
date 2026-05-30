using UnityEngine;

public class PlayerController : CharacterControllerBase
{
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private PlayerState state;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAnimation animationController;
    [SerializeField] private PlayerCombat combat;

    public override CharacterStateBase State => state;
    public override CharacterMovementBase Movement => movement;
    public override CharacterCombatBase Combat => combat;
    public override CharacterAnimationBase Animation => animationController;

    private void Awake()
    {
        movement.Initialize(state);
        combat.Initialize(state);
        animationController.Initialize(state);
    }

    private void Update()
    {
        Vector2 moveInput = input.GetMoveInput();
        float moveAmount = input.MoveAmount;

        bool isAnalogWalk = moveAmount == 0.5f;
        bool isWalkButton = input.IsWalking();
        bool isRunning = input.IsRunning();

        if (isWalkButton)
        {
            state.SetRunning(isRunning);
            state.SetWalking(!isRunning);
        }
        else
        {
            state.SetWalking(isAnalogWalk);
            state.SetRunning(isRunning && !isAnalogWalk);
        }

        movement.SetInput(moveInput);

        animationController.UpdateAnimation(moveAmount);

        if (input.IsFire())
        {
            combat.Attack();
        }

        if (input.IsDash())
        {
            Debug.Log("Dash");
        }
    }

    private void FixedUpdate()
    {
        movement.Move();
    }
}