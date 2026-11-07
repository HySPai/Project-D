using UIGame;
using UnityEngine;

public class PlayerController : CharacterControllerBase
{
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private PlayerState state;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAnimation anim;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private PlayerGuard guard;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private GamepadRumble rumble;
    [SerializeField] private PlayerElementCaster caster;
    [SerializeField] private PlayerStance stance;

    private GamePlayView gamePlayView;
    private bool controlLocked;

    /// <summary>While locked the character stands still and ignores every action input, e.g. during a rest.</summary>
    public void SetControlLocked(bool locked) => controlLocked = locked;

    public override CharacterStateBase GetState => state;
    public override CharacterMovementBase GetMovement => movement;
    public override CharacterCombatBase GetCombat => combat;
    public override CharacterAnimationBase GetAnimation => anim;
    public PlayerGuard GetGuard => guard;
    public GamepadRumble GetRumble => rumble;
    public PlayerState GetPlayerState => state;
    public PlayerInputHandler GetInput => input;
    public PlayerCombat GetPlayerCombat => combat;
    public PlayerElementCaster GetCaster => caster;
    public PlayerStance GetStance => stance;

    protected override void Awake()
    {
        base.Awake();
        state.Initialize(this);
        movement.Initialize(state, anim, combat, playerCamera, guard);
        combat.Initialize(state, this, playerCamera);
        anim.Initialize(state);
        guard.Initialize(state, anim, combat);
        input.Initialize(state);
        if (caster != null) caster.Initialize(state, this, combat);
        stance.Initialize(state, anim, combat, guard);
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

        if (caster == null) return;

        caster.OnManaChanged += gamePlayView.UpdateMana;
        caster.OnElementChanged += gamePlayView.UpdateElement;
        caster.OnCooldownChanged += gamePlayView.UpdateSpellCooldown;

        gamePlayView.UpdateMana(caster.CurrentMana, caster.MaxMana);
        gamePlayView.UpdateElement(caster.CurrentElement);
        gamePlayView.UpdateSpellCooldown(caster.GetCurrentCooldown01());
    }

    private void Update()
    {
        // 1) Cập nhật grounded + gravity trước mọi di chuyển.
        state.HandleGroundCheckAndGravity();
        if (state.IsDead) return;

        if (controlLocked)
        {
            input.DiscardBufferedActions();
            movement.SetInput(Vector2.zero);
            movement.Move();
            anim.UpdateAnimation(0f);
            return;
        }

        bool guardHeld = input.IsGuardHeld();
        bool guardPressed = input.ConsumeGuardPressed();
        if (guardHeld || guardPressed) stance.RequestCombat();
        bool weaponReady = stance.IsWeaponReady;
        guard.Tick(guardHeld && weaponReady, guardPressed && weaponReady);

        // 2) Quyết định chạy/đi.
        bool canRun = input.IsRunning()
                      && state.CurrentInput.sqrMagnitude > 0.01f
                      && state.CanRun
                      && !state.IsInWater
                      && !guard.IsGuardActive;
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
        if (input.ConsumeFirePressed())
        {
            if (stance.IsWeaponReady) combat.Attack();
            else stance.QueueAttack();
        }
        if (stance.ConsumeReadyAttack()) combat.Attack();
        if (input.IsRolling()) movement.Roll();

        if (caster != null)
        {
            int elementStep = input.ConsumeElementSwitch();
            if (elementStep != 0) caster.CycleElement(elementStep);
            if (input.ConsumeCastTap()) caster.CastSkill();
            if (input.ConsumeCastHold()) caster.Imbue();
        }

        // 5) Di chuyển (CharacterController) + cập nhật animation.
        stance.Tick();

        movement.Move();
        anim.UpdateAnimation(state.AnimationMoveAmount);
    }

    protected override void OnReset()
    {
        guard.ResetGuard();
        stance.ResetToExplore();
        if (caster != null) caster.RestoreMana();
        movement.SetInput(Vector2.zero);
        if (rumble != null) rumble.Stop();
    }

    private void OnDestroy()
    {
        if (gamePlayView == null) return;

        state.OnHeartsChanged -= gamePlayView.UpdateHearts;
        state.OnStaminaChanged -= gamePlayView.UpdateStamina;

        if (caster == null) return;

        caster.OnManaChanged -= gamePlayView.UpdateMana;
        caster.OnElementChanged -= gamePlayView.UpdateElement;
        caster.OnCooldownChanged -= gamePlayView.UpdateSpellCooldown;
    }
}