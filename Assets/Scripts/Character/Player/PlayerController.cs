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
        input.Initialize(state);
    }

    private void Update()
    {
        state.SetRunning(input.IsRunning());

        movement.SetInput(state.CurrentInput);

        animationController.UpdateAnimation(state.AnimationMoveAmount);

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