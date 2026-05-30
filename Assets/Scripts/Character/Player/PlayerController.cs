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

        state.SetRunning(input.IsRunning());

        movement.SetInput(moveInput);

        animationController.UpdateAnimation(moveInput);

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