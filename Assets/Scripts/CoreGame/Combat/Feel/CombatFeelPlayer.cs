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

    private MMF_FreezeFrame hitFreeze;
    private MMF_CinemachineImpulse hitImpulse;
    private MMF_FreezeFrame killFreeze;
    private MMF_CinemachineImpulse killImpulse;
    private bool initialized;

    public void Initialize()
    {
        if (initialized) return;
        initialized = true;

        CacheFeedbacks(hitFeedback, out hitFreeze, out hitImpulse);
        CacheFeedbacks(killFeedback, out killFreeze, out killImpulse);
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

            intensity = attackImpact * profile.IntensityScale;
        }

        player.PlayFeedbacks(position, intensity);
    }

    public static HitFeelTier ResolveTier(WeaponItem weapon, AttackData attack, bool killed)
    {
        if (killed) return HitFeelTier.Kill;
        if (weapon == null || attack == null) return HitFeelTier.Light;

        if (ReferenceEquals(attack, weapon.runningAttack) || ReferenceEquals(attack, weapon.rollingAttack))
            return HitFeelTier.Heavy;

        return HitFeelTier.Light;
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
}
