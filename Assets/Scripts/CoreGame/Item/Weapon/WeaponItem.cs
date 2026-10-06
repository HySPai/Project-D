using UnityEngine;

public enum WeaponClass
{
    StraightSword,
    Greatsword
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
    public float lungeDistance = 1.5f;
    public float impact = 1f;
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
}