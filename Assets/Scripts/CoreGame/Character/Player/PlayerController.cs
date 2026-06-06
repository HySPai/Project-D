using UIGame;
using UnityEngine;

public class PlayerController : CharacterControllerBase
{
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private PlayerState state;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAnimation anim;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private PlayerCamera camera;

    private GamePlayView gamePlayView;

    public override CharacterStateBase GetState => state;
    public override CharacterMovementBase GetMovement => movement;
    public override CharacterCombatBase GetCombat => combat;
    public override CharacterAnimationBase GetAnimation => anim;

    private void Awake()
    {
        state.Initialize(this);

        movement.Initialize(state, anim, combat, camera);
        combat.Initialize(state, this, camera);
        anim.Initialize(state);
        input.Initialize(state);
    }

    private void Start()
    {
        gamePlayView = GUIViewManager.Instance.GetGamePlayView;
        if (gamePlayView == null) return;

        state.OnHeartsChanged += gamePlayView.UpdateHearts;
        state.OnStaminaChanged += gamePlayView.UpdateStamina;

        gamePlayView.BuildHearts(state.MaxHearts);
        gamePlayView.UpdateHearts(state.CurrentHearts, state.MaxHearts);
        gamePlayView.UpdateStamina(state.CurrentStamina, state.Stamina);
    }

    private void Update()
    {
        if (input.IsFire()) combat.Attack();
        if (input.IsRolling()) movement.Roll();

        bool canRun = input.IsRunning()
                      && state.CurrentInput.sqrMagnitude > 0.01f
                      && state.CanRun;
        state.SetRunning(!state.IsRolling && canRun);
        movement.SetInput(state.CurrentInput);

        if (input.IsLockTarget())
        {
            combat.LockTarget();
        }
        else if (combat.LockOnTransform != null)
        {
            Vector2 switchDir = input.GetLockSwitchInput();
            if (switchDir != Vector2.zero)
                combat.SwitchTarget(switchDir);
        }

        float moveAmount = state.AnimationMoveAmount;
        float horizontal;
        float vertical;

        if (combat.LockOnTransform != null)
        {
            Vector3 worldMove = camera.GetMoveDirection(state.CurrentInput);
            Vector3 localMove = transform.InverseTransformDirection(worldMove.normalized);
            horizontal = localMove.x * moveAmount;
            vertical = localMove.z * moveAmount;
        }
        else
        {
            horizontal = 0f;
            vertical = moveAmount;
        }

        anim.UpdateAnimation(moveAmount);
        anim.UpdateAnimatorMovementParameters(horizontal, vertical, state.IsRunning);
    }

    private void FixedUpdate()
    {
        movement.Move();
    }

    private void OnDestroy()
    {
        if (gamePlayView == null) return;
        state.OnHeartsChanged -= gamePlayView.UpdateHearts;
        state.OnStaminaChanged -= gamePlayView.UpdateStamina;
    }
}