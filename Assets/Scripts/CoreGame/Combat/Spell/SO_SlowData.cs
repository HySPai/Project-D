using UnityEngine;

[CreateAssetMenu(fileName = "SO_SlowData", menuName = "Game/Spell/Slow Data")]
public class SO_SlowData : ScriptableObject
{
    public float duration = 3f;
    [Range(0.05f, 1f)] public float speedMultiplier = 0.6f;
}
