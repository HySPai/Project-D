using UnityEngine;

public enum HitFeelTier
{
    Light,
    Heavy,
    Kill
}

[CreateAssetMenu(fileName = "WeaponFeelProfile", menuName = "Game/Combat/Weapon Feel Profile")]
public class WeaponFeelProfile : ScriptableObject
{
    [Header("Hit Stop (seconds)")]
    [SerializeField, Range(0f, 0.15f)] private float hitStopLight = 0.05f;
    [SerializeField, Range(0f, 0.2f)] private float hitStopHeavy = 0.05f;
    [SerializeField, Range(0f, 0.25f)] private float hitStopKill = 0.1f;

    [Header("Camera Shake Velocity")]
    [SerializeField] private Vector3 shakeVelocityLight = new Vector3(0.08f, -0.1f, 0f);
    [SerializeField] private Vector3 shakeVelocityHeavy = new Vector3(0.08f, -0.1f, 0f);
    [SerializeField] private Vector3 shakeVelocityKill = new Vector3(0.16f, -0.2f, 0f);

    [Header("Intensity")]
    [SerializeField, Range(0.5f, 2f)] private float hitFeedbackIntensityScale = 1f;

    public float IntensityScale => hitFeedbackIntensityScale;

    public float GetHitStop(HitFeelTier tier)
    {
        switch (tier)
        {
            case HitFeelTier.Heavy: return hitStopHeavy;
            case HitFeelTier.Kill: return hitStopKill;
            default: return hitStopLight;
        }
    }

    public Vector3 GetShakeVelocity(HitFeelTier tier)
    {
        switch (tier)
        {
            case HitFeelTier.Heavy: return shakeVelocityHeavy;
            case HitFeelTier.Kill: return shakeVelocityKill;
            default: return shakeVelocityLight;
        }
    }
}
