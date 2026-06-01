using UnityEngine;

public class PlayerController : CharacterControllerBase
{
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private PlayerState state;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAnimation anim;
    [SerializeField] private PlayerCombat combat;

    public override CharacterStateBase GetState => state;
    public override CharacterMovementBase GetMovement => movement;
    public override CharacterCombatBase GetCombat => combat;
    public override CharacterAnimationBase GetAnimation => anim;

    private void Awake()
    {
        movement.Initialize(state, anim, combat);
        combat.Initialize(state, this);
        anim.Initialize(state);
        input.Initialize(state);
    }

    private void Update()
    {
        bool canRun = input.IsRunning() && state.CurrentInput.sqrMagnitude > 0.01f;

        state.SetRunning(!state.IsRolling && canRun);

        movement.SetInput(state.CurrentInput);

        anim.UpdateAnimation(state.AnimationMoveAmount);

        if (input.IsLockTarget())
        {
            combat.LockTarget();
        }

        if (input.IsFire())
        {
            combat.Attack();
        }

        if (input.IsRolling())
        {
            movement.Roll();
        }
    }

    private void FixedUpdate()
    {
        movement.Move();
    }
}