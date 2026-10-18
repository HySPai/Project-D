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

    [Header("Hit Weight (scales SO_EnemyStats knockback / stun)")]
    [SerializeField, Range(0f, 3f)] private float knockbackScaleLight = 1f;
    [SerializeField, Range(0f, 3f)] private float knockbackScaleHeavy = 1.6f;
    [SerializeField, Range(0f, 3f)] private float stunScaleLight = 1f;
    [SerializeField, Range(0f, 3f)] private float stunScaleHeavy = 1.5f;

    [Header("Blood Amount (falling droplets per hit)")]
    [SerializeField, Range(0, 32)] private int bloodDropletsLight = 2;
    [SerializeField, Range(0, 32)] private int bloodDropletsHeavy = 3;
    [SerializeField, Range(0, 32)] private int bloodDropletsKill = 5;

    [Header("Blood Slash Vfx (prefabs registered in SO_Vfx)")]
    [SerializeField] private VFXName bloodSlashLight = VFXName.vfx_blood_slash_small;
    [SerializeField] private VFXName bloodSlashHeavy = VFXName.vfx_blood_slash_medium;
    [SerializeField] private VFXName bloodSlashKill = VFXName.vfx_blood_slash_large;
    [SerializeField, Range(0.2f, 3f)] private float bloodSlashLifetime = 1f;

    [Header("Intensity")]
    [SerializeField, Range(0.5f, 2f)] private float hitFeedbackIntensityScale = 1f;

    [Header("Sound")]
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private AudioClip hitSoundLight;
    [SerializeField] private AudioClip hitSoundHeavy;
    [SerializeField] private AudioClip killSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;
    [SerializeField, Range(0f, 0.3f)] private float pitchVariation = 0.08f;

    [Header("Input")]
    [SerializeField, Range(0.1f, 0.5f)] private float inputBufferDuration = 0.4f;

    public float IntensityScale => hitFeedbackIntensityScale;
    public AudioClip SwingSound => swingSound;
    public AudioClip HitSoundLight => hitSoundLight;
    public AudioClip HitSoundHeavy => hitSoundHeavy;
    public AudioClip KillSound => killSound;
    public float SoundVolume => soundVolume;
    public float PitchVariation => pitchVariation;
    public float InputBufferDuration => inputBufferDuration;

    public AudioClip GetHitSound(HitFeelTier tier)
    {
        switch (tier)
        {
            case HitFeelTier.Heavy: return hitSoundHeavy;
            case HitFeelTier.Kill: return killSound;
            default: return hitSoundLight;
        }
    }

    public float GetHitStop(HitFeelTier tier)
    {
        switch (tier)
        {
            case HitFeelTier.Heavy: return hitStopHeavy;
            case HitFeelTier.Kill: return hitStopKill;
            default: return hitStopLight;
        }
    }

    /// <summary>Blood droplets this weapon spills for a hit tier. Example: GetBloodDroplets(HitFeelTier.Kill)</summary>
    public int GetBloodDroplets(HitFeelTier tier)
    {
        switch (tier)
        {
            case HitFeelTier.Heavy: return bloodDropletsHeavy;
            case HitFeelTier.Kill: return bloodDropletsKill;
            default: return bloodDropletsLight;
        }
    }

    public float BloodSlashLifetime => bloodSlashLifetime;

    /// <summary>Blood slash prefab this weapon plays for a hit tier. Example: GetBloodSlashVfx(HitFeelTier.Heavy)</summary>
    public VFXName GetBloodSlashVfx(HitFeelTier tier)
    {
        switch (tier)
        {
            case HitFeelTier.Heavy: return bloodSlashHeavy;
            case HitFeelTier.Kill: return bloodSlashKill;
            default: return bloodSlashLight;
        }
    }

    public float GetKnockbackScale(AttackWeight weight) =>
        weight == AttackWeight.Heavy ? knockbackScaleHeavy : knockbackScaleLight;

    public float GetStunScale(AttackWeight weight) =>
        weight == AttackWeight.Heavy ? stunScaleHeavy : stunScaleLight;

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
