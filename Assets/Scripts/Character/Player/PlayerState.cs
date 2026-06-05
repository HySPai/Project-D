using UnityEngine;

public class PlayerState : CharacterStateBase
{
    #region Stamina
    private SO_PlayerStats playerStats;

    [Header("Stamina (runtime)")]
    [SerializeField] protected float currentStamina;
    protected float lastStaminaUseTime;

    public float Stamina => playerStats.maxStamina;
    public float CurrentStamina => currentStamina;
    public bool HasStamina => currentStamina > 0f;
    public bool HasEnoughStamina(float amount) => currentStamina >= amount;

    public float RollStaminaCost => playerStats.rollStaminaCost;
    public float RunStaminaDrainRate => playerStats.runStaminaDrainRate;
    private bool runExhausted;
    public bool CanRun
    {
        get
        {
            if (currentStamina <= 0f) runExhausted = true;
            else if (currentStamina >= playerStats.runResumeStamina) runExhausted = false;
            return !runExhausted;
        }
    }
    #endregion

    #region Ground Check
    [Header("Ground Check")]
    [SerializeField] protected float groundCheckDistance = 5f;
    [SerializeField] protected float edgeCheckForwardDistance = 0.5f;
    [SerializeField] protected float maxStepDownHeight = 1f;
    public float GroundCheckDistance => groundCheckDistance;
    public float EdgeCheckForwardDistance => edgeCheckForwardDistance;
    public float MaxStepDownHeight => maxStepDownHeight;
    #endregion

    #region Input
    [Header("Input")]
    [SerializeField] private float fullSpeedInputThreshold = 0.7f;
    [SerializeField] private Vector2 currentInput;
    [SerializeField] private Vector2 currentCameraInput;
    public float FullSpeedInputThreshold => fullSpeedInputThreshold;
    public Vector2 CurrentInput => currentInput;
    public Vector2 CurrentCameraInput => currentCameraInput;
    #endregion

    #region Animation
    public float AnimationMoveAmount
    {
        get
        {
            float moveAmount =
                Mathf.Clamp01(
                    CurrentInput.magnitude /
                    FullSpeedInputThreshold);
            if (moveAmount <= 0.01f)
            {
                return 0f;
            }
            if (IsRunning)
            {
                return 1.5f;
            }
            return moveAmount;
        }
    }
    #endregion

    protected override void InitializeStats()
    {
        base.InitializeStats();

        playerStats = stats as SO_PlayerStats;
        if (playerStats == null)
        {
            Debug.LogError($"{name}: stats phải là SO_PlayerStats", this);
            return;
        }
        currentStamina = playerStats.maxStamina;
    }

    protected virtual void Update()
    {
        RegenerateStamina();
    }

    #region Input Setters
    public void SetCurrentInput(Vector2 value)
    {
        currentInput = value;
    }
    public void SetCurrentCameraInput(Vector2 value)
    {
        currentCameraInput = value;
    }
    #endregion

    #region Stamina Logic
    public void DrainStamina(float amount)
    {
        if (amount <= 0f) return;
        currentStamina = Mathf.Max(0f, currentStamina - amount);
        lastStaminaUseTime = Time.time;
    }

    protected virtual void RegenerateStamina()
    {
        if (playerStats == null) return;
        if (currentStamina >= playerStats.maxStamina) return;
        if (Time.time - lastStaminaUseTime < playerStats.staminaRegenDelay) return;

        currentStamina = Mathf.Min(playerStats.maxStamina,
            currentStamina + playerStats.staminaRegenRate * Time.deltaTime);
    }
    #endregion

    protected override void Die()
    {
        base.Die();
        Debug.Log("Player Dead");
    }
}