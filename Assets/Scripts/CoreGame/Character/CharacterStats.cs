using UnityEngine;

public class CharacterStats : ScriptableObject
{
    [Header("Health")]
    public float maxHp = 100f;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float rotateSpeed = 12f;
}
