using UnityEngine;

public enum WeaponClass
{
    StraightSword,
    Greatsword,
    DualBlades
}

public enum AttackWeight
{
    Light,
    Heavy
}

[System.Serializable]
public class AttackData
{
    public string animationName;
    public float damage = 25f;
    public float stamina = 10f;

    [Header("Hit Shape")]
    public float range = 1.9f;
    public float arc = 160f;

    [Header("Feel")]
    public float animationSpeed = 1f;
    public float impact = 1f;
    public AttackWeight weight = AttackWeight.Light;
}

[System.Serializable]
public class GuardData
{
    public bool canGuard = true;
    public string animationSet = "Off";

    [Header("Block")]
    public float blockArc = 120f;
    public float blockedDamageRatio = 1f;
    public float staminaPerBlockedPoint = 0.8f;
    public float moveSpeedMultiplier = 0.5f;
    public float blockInvulnerability = 0.12f;
    public float mediumBlockPoints = 15f;
    public float heavyBlockPoints = 30f;

    [Header("Guard Break")]
    public float guardBreakStun = 1f;
    public float guardStunThreshold = 1f;
    public float guardStunDuration = 0.6f;

    [Header("Parry")]
    public bool canParry = true;
    public float parryWindow = 0.15f;
    public float parryStaminaCost = 10f;
    public float parryCooldown = 0.4f;
    public float parryStaggerDuration = 1.5f;
    public float parrySlowScale = 0.2f;
    public float parrySlowDuration = 0.1f;
}

[CreateAssetMenu(fileName = "WeaponItem", menuName = "Game/Weapon Item")]
public class WeaponItem : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;
    public Sprite weaponIcon;
    public WeaponClass weaponClass;

    [Header("Light Combo")]
    public AttackData[] lightAttacks;

    [Header("Running Attack")]
    public AttackData runningAttack;

    [Header("Rolling Attack")]
    public AttackData rollingAttack;

    [Header("Backstep")]
    public string backstepAnimation = "Backstep_Main";
    public AttackData backstepAttack;

    [Header("Guard")]
    public GuardData guard;
    public float counterMultiplier = 3f;

    [Header("Feel")]
    public WeaponFeelProfile feelProfile;
}
