using UnityEngine;

[CreateAssetMenu(fileName = "SO_StoneBurstData", menuName = "Game/Spell/Stone Burst Data")]
public class SO_StoneBurstData : ScriptableObject
{
    public float damage = 35f;
    public float range = 4f;
    [Range(0f, 360f)] public float arc = 90f;
    public float windup = 0.35f;
    public float stunDuration = 1f;
    public bool breaksArmor = true;
}
