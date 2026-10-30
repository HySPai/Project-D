using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerElementCaster : MonoBehaviour
{
    private const ElementType FirstCastable = ElementType.Wind;
    private static readonly int ElementCount = Enum.GetValues(typeof(ElementType)).Length;
    private static readonly int CastableCount = ElementCount - (int)FirstCastable;

    [Header("Elements")]
    [SerializeField] private ElementType currentElement = ElementType.Fire;

    [Header("Data")]
    [SerializeField] private SO_ElementCasterData casterData;

    [Header("Fireball")]
    [SerializeField] private SO_FireballData fireballData;
    [SerializeField] private Fireball fireballPrefab;

    private readonly List<Fireball> fireballPool = new List<Fireball>();
    private readonly float[] cooldownRemaining = new float[ElementCount];

    private CharacterStateBase state;
    private CharacterControllerBase character;
    private PlayerCombat combat;
    private float currentMana;

    public ElementType CurrentElement => currentElement;
    public float CurrentMana => currentMana;
    public float MaxMana => casterData != null ? casterData.maxMana : 0f;

    public event Action<ElementType> OnElementChanged;
    public event Action<float, float> OnManaChanged;
    public event Action<float> OnCooldownChanged;

    public void Initialize(CharacterStateBase state, CharacterControllerBase character, PlayerCombat combat)
    {
        this.state = state;
        this.character = character;
        this.combat = combat;

        currentMana = MaxMana;

        if (combat != null)
            combat.OnWeaponHit += ChargeFromHit;
    }

    private void OnDestroy()
    {
        if (combat != null)
            combat.OnWeaponHit -= ChargeFromHit;
    }

    private void Update()
    {
        bool currentWasCooling = cooldownRemaining[(int)currentElement] > 0f;

        for (int i = 0; i < ElementCount; i++)
        {
            if (cooldownRemaining[i] > 0f)
                cooldownRemaining[i] = Mathf.Max(0f, cooldownRemaining[i] - Time.deltaTime);
        }

        if (currentWasCooling)
            RaiseCooldownChanged();
    }

    /// <summary>Moves to the next (step 1) or previous (step -1) castable element in ElementType order, skipping None and wrapping at both ends.</summary>
    public void CycleElement(int step)
    {
        if (step == 0) return;

        currentElement = NextCastable(currentElement, step);
        Debug.Log($"[Phép] Đổi nguyên tố: {currentElement}");
        OnElementChanged?.Invoke(currentElement);
        RaiseCooldownChanged();
    }

    /// <summary>The castable element step places away from current, never None; a None input counts as the slot before the first castable.</summary>
    public static ElementType NextCastable(ElementType current, int step)
    {
        int index = Mathf.Max(-1, (int)current - (int)FirstCastable);
        int next = ((index + step) % CastableCount + CastableCount) % CastableCount;
        return (ElementType)(next + (int)FirstCastable);
    }

    /// <summary>Tap of the cast button: fires the combat skill of the equipped element if mana and cooldown allow.</summary>
    public void CastSkill()
    {
        if (!CanCast() || casterData == null) return;

        if (cooldownRemaining[(int)currentElement] > 0f)
        {
            Debug.Log($"[Phép] {currentElement} đang hồi: còn {cooldownRemaining[(int)currentElement]:0.0}s");
            return;
        }

        if (currentMana < casterData.skillCost)
        {
            Debug.Log($"[Phép] Thiếu mana: {currentMana:0}/{casterData.skillCost:0}");
            return;
        }

        if (!LaunchSkill(currentElement)) return;

        SetMana(currentMana - casterData.skillCost);
        cooldownRemaining[(int)currentElement] = casterData.GetCooldown(currentElement);
        RaiseCooldownChanged();
    }

    /// <summary>Hold of the cast button: imbues the main weapon, which needs and spends a full mana bar.</summary>
    public void Imbue()
    {
        if (!CanCast() || casterData == null) return;

        if (currentMana < casterData.maxMana)
        {
            Debug.Log($"[Phép] Yểm cần đầy mana: {currentMana:0}/{casterData.maxMana:0}");
            return;
        }

        SetMana(0f);
        Debug.Log($"[Phép] Yểm {currentElement} lên vũ khí (chưa làm)");
    }

    /// <summary>Fills mana and clears every cooldown, as resting at a rest point or respawning does.</summary>
    public void RestoreMana()
    {
        for (int i = 0; i < ElementCount; i++)
            cooldownRemaining[i] = 0f;

        SetMana(MaxMana);
        RaiseCooldownChanged();
    }

    /// <summary>Remaining cooldown of the equipped element, 1 just cast and 0 ready.</summary>
    public float GetCurrentCooldown01()
    {
        if (casterData == null) return 0f;

        float total = casterData.GetCooldown(currentElement);
        return total > 0f ? cooldownRemaining[(int)currentElement] / total : 0f;
    }

    private void ChargeFromHit(float damage)
    {
        if (casterData == null) return;

        SetMana(currentMana + casterData.GetCharge(damage));
    }

    private void SetMana(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, MaxMana);
        if (Mathf.Approximately(clamped, currentMana)) return;

        currentMana = clamped;
        OnManaChanged?.Invoke(currentMana, MaxMana);
    }

    private void RaiseCooldownChanged() => OnCooldownChanged?.Invoke(GetCurrentCooldown01());

    private bool CanCast()
    {
        if (state == null) return false;
        return !state.IsDead && !state.IsRolling && !state.IsAttacking;
    }

    private bool LaunchSkill(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire:
                return LaunchFireball();
            default:
                Debug.Log($"[Phép] Kỹ năng {element} chưa làm, không tốn mana");
                return false;
        }
    }

    private bool LaunchFireball()
    {
        if (fireballData == null || fireballPrefab == null) return false;

        Vector3 direction = GetCastDirection();
        transform.rotation = Quaternion.LookRotation(direction);

        Vector3 position = transform.position
                           + Vector3.up * fireballData.spawnHeight
                           + direction * fireballData.spawnForward;

        GetFreeFireball().Launch(fireballData, character, position, direction);
        Debug.Log("[Phép] Cầu Lửa");
        return true;
    }

    private Vector3 GetCastDirection()
    {
        Transform target = combat != null ? combat.LockOnTransform : null;
        Vector3 direction = target != null ? target.position - transform.position : transform.forward;
        direction.y = 0f;

        return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
    }

    private Fireball GetFreeFireball()
    {
        foreach (Fireball fireball in fireballPool)
        {
            if (!fireball.gameObject.activeSelf)
                return fireball;
        }

        Fireball created = Instantiate(fireballPrefab);
        created.gameObject.SetActive(false);
        fireballPool.Add(created);
        return created;
    }
}
