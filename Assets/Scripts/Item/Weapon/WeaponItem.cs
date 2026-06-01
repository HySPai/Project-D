using UnityEngine;

[CreateAssetMenu(fileName = "WeaponItem", menuName = "Game/Weapon Item")]
public class WeaponItem : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;

    [Header("Light Combo")]
    public string[] lightAttackAnimations;

    [Header("Running Attack")]
    public string runningAttackAnimation;

    [Header("Rolling Attack")]
    public string rollingAttackAnimation;
}