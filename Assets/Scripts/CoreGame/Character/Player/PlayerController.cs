using UIGame;
using UnityEngine;

public class PlayerController : CharacterControllerBase
{
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private PlayerState state;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAnimation anim;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private PlayerCamera playerCamera;

    private GamePlayView gamePlayView;

    public override CharacterStateBase GetState => state;
    public override CharacterMovementBase GetMovement => movement;
    public override CharacterCombatBase GetCombat => combat;
    public override CharacterAnimationBase GetAnimation => anim;

    private void Awake()
    {
        state.Initialize(this);
        movement.Initialize(state, anim, combat, playerCamera);
        combat.Initialize(state, this, playerCamera);
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
        // 1) Cập nhật grounded + gravity trước mọi di chuyển.
        state.HandleGroundCheckAndGravity();

        // 2) Quyết định chạy/đi.
        bool canRun = input.IsRunning()
                      && state.CurrentInput.sqrMagnitude > 0.01f
                      && state.CanRun;
        state.SetRunning(!state.IsRolling && canRun);

        movement.SetInput(state.CurrentInput);

        // 3) Lock-on / switch target.
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

        // 4) Action.
        if (input.IsFire()) combat.Attack();
        if (input.IsRolling()) movement.Roll();

        // 5) Di chuyển (CharacterController) + cập nhật animation.
        movement.Move();
        anim.UpdateAnimation(state.AnimationMoveAmount);
    }

    private void OnDestroy()
    {
        if (gamePlayView == null) return;

        state.OnHeartsChanged -= gamePlayView.UpdateHearts;
        state.OnStaminaChanged -= gamePlayView.UpdateStamina;
    }
}