using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerElementCaster : MonoBehaviour
{
    private static readonly int ElementCount = Enum.GetValues(typeof(EElement)).Length;

    [Header("Elements")]
    [SerializeField] private EElement currentElement = EElement.Fire;

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

    public EElement CurrentElement => currentElement;
    public float CurrentMana => currentMana;
    public float MaxMana => casterData != null ? casterData.maxMana : 0f;

    public event Action<EElement> OnElementChanged;
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

    /// <summary>Moves to the next (step 1) or previous (step -1) element in EElement order, wrapping at both ends.</summary>
    public void CycleElement(int step)
    {
        if (step == 0) return;

        int next = ((int)currentElement + step % ElementCount + ElementCount) % ElementCount;
        currentElement = (EElement)next;
        Debug.Log($"[Phép] Đổi nguyên tố: {currentElement}");
        OnElementChanged?.Invoke(currentElement);
        RaiseCooldownChanged();
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

        switch (currentElement)
        {
            case EElement.Fire:
                if (!LaunchFireball()) return;
                break;
            case EElement.Wind:
                Debug.Log("[Phép] Lốc Xoáy (chưa làm)");
                break;
            case EElement.Water:
                Debug.Log("[Phép] Vòng Nước (chưa làm)");
                break;
            case EElement.Stone:
                Debug.Log("[Phép] Đá Văng (chưa làm)");
                break;
        }

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
