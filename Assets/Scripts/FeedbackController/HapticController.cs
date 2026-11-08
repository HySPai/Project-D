using Lofelt.NiceVibrations;
using UnityEngine;

/// <summary>Plays a light haptic tap, honouring the player's haptic setting and a short anti-spam cooldown.</summary>
public class HapticController : SingletonMonoBehaviour<HapticController>
{
    private const float HapticCooldown = 0.1f;

    private float lastHapticTime = float.NegativeInfinity;

    /// <summary>Plays a light impact unless haptics are off or one just played.</summary>
    public void PlayHaptic()
    {
        if (PlayerprefSave.Haptic == 0f) return;
        if (Time.time - lastHapticTime < HapticCooldown) return;

        lastHapticTime = Time.time;
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
    }
}
