using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class PlayerState : CharacterStateBase
{
    private SO_PlayerStats playerStats;

    #region Hearts
    public event Action<int, int> OnHeartsChanged;

    private int currentHearts;
    public int CurrentHearts => currentHearts;
    public int MaxHearts => playerStats.maxHearts;

    private void RaiseHeartsChanged() => OnHeartsChanged?.Invoke(currentHearts, playerStats.maxHearts);

    // Damage dạng float (từ DamageCollider) → quy đổi sang số tim, tối thiểu 1.
    public override void TakeDamage(float damage, Vector3 sourcePosition)
    {
        if (damage <= 0f) return;
        int hearts = Mathf.Max(1, Mathf.CeilToInt(damage));
        TakeDamage(hearts, sourcePosition);
    }

    public override void TakeDamage(float damage) =>
        TakeDamage(damage, transform.position + transform.forward);

    [Button]
    public void TakeDamage(int amount) =>
        TakeDamage(amount, transform.position + transform.forward);

    public void TakeDamage(int amount, Vector3 sourcePosition)
    {
        if (isDead) return;
        if (isInvulnerable) return;
        if (amount <= 0) return;

        currentHearts = Mathf.Max(0, currentHearts - amount);
        currentHp = currentHearts;
        RaiseHeartsChanged();
        RaiseHpChanged();

        InterruptAttack();
        PlayDamageFeedback();

        if (currentHearts <= 0)
        {
            Die();
            return;
        }

        PlayHitReaction(sourcePosition);
        EnableIsInvulnerable();
        InvulnerabilityAsync().Forget();
    }

    private void InterruptAttack()
    {
        var combat = owner != null ? owner.GetCombat : null;
        if (combat != null)
            combat.DisableAllDamageColliders();
    }

    [Button]
    public int Heal(int amount)
    {
        if (amount <= 0 || isDead) return 0;

        int before = currentHearts;
        currentHearts = Mathf.Min(playerStats.maxHearts, currentHearts + amount);
        int healed = currentHearts - before;

        if (healed > 0)
        {
            currentHp = currentHearts;
            RaiseHeartsChanged();
            RaiseHpChanged();
        }

        return healed;
    }

    private async UniTaskVoid InvulnerabilityAsync()
    {
        var token = this.GetCancellationTokenOnDestroy();
        await UniTask.Delay(
            TimeSpan.FromSeconds(playerStats.invulnerabilityDuration),
            cancellationToken: token);
        DisableIsInvulnerable();
    }
    #endregion

    #region Stamina
    [Header("Stamina (runtime)")]
    [SerializeField] protected float currentStamina;
    protected float lastStaminaUseTime;
    private bool runExhausted;

    public event Action<float, float> OnStaminaChanged;

    public float Stamina => playerStats.maxStamina;
    public float CurrentStamina => currentStamina;
    public bool HasStamina => currentStamina > 0f;
    public bool HasEnoughStamina(float amount) => currentStamina >= amount;

    public float RollStaminaCost => playerStats.rollStaminaCost;
    public float RunStaminaDrainRate => playerStats.runStaminaDrainRate;

    public bool CanRun
    {
        get
        {
            if (currentStamina <= 0f) runExhausted = true;
            else if (currentStamina >= playerStats.runResumeStamina) runExhausted = false;
            return !runExhausted;
        }
    }

    private void RaiseStaminaChanged() => OnStaminaChanged?.Invoke(currentStamina, playerStats.maxStamina);

    public void DrainStamina(float amount)
    {
        if (amount <= 0f) return;
        currentStamina = Mathf.Max(0f, currentStamina - amount);
        lastStaminaUseTime = Time.time;
        RaiseStaminaChanged();
    }

    protected virtual void RegenerateStamina()
    {
        if (playerStats == null) return;
        if (currentStamina >= playerStats.maxStamina) return;
        if (Time.time - lastStaminaUseTime < playerStats.staminaRegenDelay) return;

        currentStamina = Mathf.Min(playerStats.maxStamina,
            currentStamina + playerStats.staminaRegenRate * Time.deltaTime);
        RaiseStaminaChanged();
    }
    #endregion

    #region Ground Check Extra
    [Header("Ground Check")]
    [SerializeField] protected float groundCheckDistance = 5f;
    [SerializeField] protected float edgeCheckForwardDistance = 0.5f;

    public float GroundCheckDistance => groundCheckDistance;
    public float EdgeCheckForwardDistance => edgeCheckForwardDistance;
    #endregion

    #region Input
    [Header("Input")]
    [SerializeField] private float fullSpeedInputThreshold = 0.7f;
    [SerializeField] private Vector2 currentInput;
    [SerializeField] private Vector2 currentCameraInput;

    public float FullSpeedInputThreshold => fullSpeedInputThreshold;
    public Vector2 CurrentInput => currentInput;
    public Vector2 CurrentCameraInput => currentCameraInput;

    public void SetCurrentInput(Vector2 value) => currentInput = value;
    public void SetCurrentCameraInput(Vector2 value) => currentCameraInput = value;
    #endregion

    #region Animation Move Amount
    public float AnimationMoveAmount
    {
        get
        {
            float moveAmount = Mathf.Clamp01(CurrentInput.magnitude / FullSpeedInputThreshold);
            if (moveAmount <= 0.01f) return 0f;
            if (IsRunning) return 2f;
            return moveAmount;
        }
    }
    #endregion

    #region Lifecycle
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
        currentHearts = playerStats.maxHearts;
        maxHp = playerStats.maxHearts;
    }

    protected virtual void Update()
    {
        RegenerateStamina();
    }

    protected override void Die()
    {
        base.Die();
        Debug.Log("Player Dead");
    }
    #endregion

    #region Animation Test Buttons
    private void PlayAction(in AnimationAction action)
    {
        var animation = owner != null ? owner.GetAnimation : null;
        if (animation == null)
        {
            Debug.LogWarning($"{name}: thiếu Animation để play action", this);
            return;
        }
        animation.Play(action);
    }
    #endregion
}