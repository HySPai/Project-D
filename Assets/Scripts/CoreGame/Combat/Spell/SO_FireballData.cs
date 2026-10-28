using UnityEngine;

[CreateAssetMenu(fileName = "SO_FireballData", menuName = "Game/Spell/Fireball Data")]
public class SO_FireballData : ScriptableObject
{
    [Header("Damage")]
    public float damage = 30f;
    public float explosionRadius = 1.5f;

    [Header("Flight")]
    public float speed = 15f;
    public float maxRange = 10f;
    public float projectileRadius = 0.3f;

    [Header("Spawn")]
    public float spawnHeight = 1f;
    public float spawnForward = 0.8f;
}
