using UnityEngine;

[CreateAssetMenu(fileName = "EnemyCombatFeelProfile", menuName = "Game/Combat/Enemy Combat Feel Profile")]
public class EnemyCombatFeelProfile : ScriptableObject
{
    [Header("Telegraph (slowed windup)")]
    [SerializeField, Range(0f, 1f)] private float telegraphDuration = 0.35f;
    [SerializeField, Range(0.1f, 1f)] private float windupAnimationSpeed = 0.5f;

    public float TelegraphDuration => telegraphDuration;
    public float WindupAnimationSpeed => windupAnimationSpeed;
    public bool HasTelegraph => telegraphDuration > 0f;
}
