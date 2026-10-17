using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using UnityEngine;

public class CombatFeelPlayer : MonoBehaviour
{
    private const string HitStopLabel = "Hit Stop";
    private const string CameraShakeLabel = "Camera Shake";

    [Header("Feedbacks (Feel)")]
    [SerializeField] private MMF_Player hitFeedback;
    [SerializeField] private MMF_Player killFeedback;
    [SerializeField] private MMF_Player swingFeedback;

    private MMF_FreezeFrame hitFreeze;
    private MMF_CinemachineImpulse hitImpulse;
    private MMF_FreezeFrame killFreeze;
    private MMF_CinemachineImpulse killImpulse;
    private SoundSlot hitSound;
    private SoundSlot killSoundSlot;
    private SoundSlot swingSound;
    private bool initialized;

    public void Initialize()
    {
        if (initialized) return;
        initialized = true;

        CacheFeedbacks(hitFeedback, out hitFreeze, out hitImpulse);
        CacheFeedbacks(killFeedback, out killFreeze, out killImpulse);

        hitSound = CacheSound(hitFeedback);
        killSoundSlot = CacheSound(killFeedback);
        swingSound = CacheSound(swingFeedback);
    }

    public void PlayHit(WeaponFeelProfile profile, HitFeelTier tier, float attackImpact, Vector3 position)
    {
        Initialize();

        bool useKill = tier == HitFeelTier.Kill && killFeedback != null;
        MMF_Player player = useKill ? killFeedback : hitFeedback;
        if (player == null) return;

        float intensity = attackImpact;

        if (profile != null)
        {
            MMF_FreezeFrame freeze = useKill ? killFreeze : hitFreeze;
            MMF_CinemachineImpulse impulse = useKill ? killImpulse : hitImpulse;

            if (freeze != null)
                freeze.FreezeFrameDuration = profile.GetHitStop(tier);

            if (impulse != null)
                impulse.Velocity = profile.GetShakeVelocity(tier);

            SoundSlot sound = useKill ? killSoundSlot : hitSound;
            sound?.Apply(profile.GetHitSound(tier), profile.SoundVolume, profile.PitchVariation);

            intensity = attackImpact * profile.IntensityScale;
        }

        player.PlayFeedbacks(position, intensity);
    }

    public void PlaySwing(WeaponFeelProfile profile, Vector3 position)
    {
        Initialize();

        if (swingFeedback == null) return;

        if (profile != null)
            swingSound?.Apply(profile.SwingSound, profile.SoundVolume, profile.PitchVariation);

        swingFeedback.PlayFeedbacks(position);
    }

    public static HitFeelTier ResolveTier(AttackData attack, bool killed)
    {
        if (killed) return HitFeelTier.Kill;
        if (attack == null) return HitFeelTier.Light;

        return attack.weight == AttackWeight.Heavy ? HitFeelTier.Heavy : HitFeelTier.Light;
    }

    private void CacheFeedbacks(MMF_Player player, out MMF_FreezeFrame freeze, out MMF_CinemachineImpulse impulse)
    {
        freeze = null;
        impulse = null;
        if (player == null) return;

        freeze = player.GetFeedbackOfType<MMF_FreezeFrame>(HitStopLabel);
        impulse = player.GetFeedbackOfType<MMF_CinemachineImpulse>(CameraShakeLabel);

        if (freeze == null)
            Debug.LogWarning($"[CombatFeelPlayer] '{HitStopLabel}' MMF_FreezeFrame not found on {player.name}", this);
        if (impulse == null)
            Debug.LogWarning($"[CombatFeelPlayer] '{CameraShakeLabel}' MMF_CinemachineImpulse not found on {player.name}", this);
    }

    private SoundSlot CacheSound(MMF_Player player)
    {
        if (player == null) return null;

        MMF_Sound sound = player.GetFeedbackOfType<MMF_Sound>();
        if (sound == null)
        {
            Debug.LogWarning($"[CombatFeelPlayer] MMF_Sound not found on {player.name}", this);
            return null;
        }

        return new SoundSlot(sound);
    }

    private sealed class SoundSlot
    {
        private readonly MMF_Sound sound;
        private readonly AudioClip authoredClip;
        private readonly float authoredMinVolume;
        private readonly float authoredMaxVolume;
        private readonly float authoredMinPitch;
        private readonly float authoredMaxPitch;

        public SoundSlot(MMF_Sound sound)
        {
            this.sound = sound;
            authoredClip = sound.Sfx;
            authoredMinVolume = sound.MinVolume;
            authoredMaxVolume = sound.MaxVolume;
            authoredMinPitch = sound.MinPitch;
            authoredMaxPitch = sound.MaxPitch;
        }

        public void Apply(AudioClip clip, float volume, float pitchVariation)
        {
            if (clip == null)
            {
                sound.Sfx = authoredClip;
                sound.MinVolume = authoredMinVolume;
                sound.MaxVolume = authoredMaxVolume;
                sound.MinPitch = authoredMinPitch;
                sound.MaxPitch = authoredMaxPitch;
                return;
            }

            sound.Sfx = clip;
            sound.MinVolume = volume;
            sound.MaxVolume = volume;
            sound.MinPitch = 1f - pitchVariation;
            sound.MaxPitch = 1f + pitchVariation;
        }
    }
}
