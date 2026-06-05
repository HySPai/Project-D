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