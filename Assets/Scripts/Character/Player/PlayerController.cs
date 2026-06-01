using UnityEngine;

public class PlayerController : CharacterControllerBase
{
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private PlayerState state;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAnimation animationController;
    [SerializeField] private PlayerCombat combat;

    public override CharacterStateBase GetState => state;
    public override CharacterMovementBase GetMovement => movement;
    public override CharacterCombatBase GetCombat => combat;
    public override CharacterAnimationBase GetAnimation => animationController;

    private void Awake()
    {
        movement.Initialize(state);
        combat.Initialize(state, this);
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