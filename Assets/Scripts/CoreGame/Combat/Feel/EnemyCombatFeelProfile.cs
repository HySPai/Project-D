using UnityEngine;

[CreateAssetMenu(fileName = "EnemyCombatFeelProfile", menuName = "Game/Combat/Enemy Combat Feel Profile")]
public class EnemyCombatFeelProfile : ScriptableObject
{
    [Header("Telegraph")]
    [SerializeField, Range(0f, 1f)] private float telegraphDuration = 0.35f;
    [SerializeField, Range(0.1f, 1f)] private float windupAnimationSpeed = 0.5f;
    [SerializeField, ColorUsage(true, true)] private Color telegraphColor = new Color(1f, 0.55f, 0.1f, 1f);
    [SerializeField, Range(0.02f, 0.2f)] private float telegraphFlickerPeriod = 0.06f;
    [SerializeField] private AudioClip telegraphSound;

    public float TelegraphDuration => telegraphDuration;
    public float WindupAnimationSpeed => windupAnimationSpeed;
    public Color TelegraphColor => telegraphColor;
    public float TelegraphFlickerPeriod => telegraphFlickerPeriod;
    public AudioClip TelegraphSound => telegraphSound;
    public bool HasTelegraph => telegraphDuration > 0f;
}
