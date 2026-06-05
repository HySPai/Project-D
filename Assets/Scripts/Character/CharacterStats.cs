using UnityEngine;

[CreateAssetMenu(fileName = "SO_CharacterStats", menuName = "Game/Stats/Character Stats")]
public class CharacterStats : ScriptableObject
{
    [Header("Health")]
    public float maxHp = 100f;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float rotateSpeed = 12f;
}
