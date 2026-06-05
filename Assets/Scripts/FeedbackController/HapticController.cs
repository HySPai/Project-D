
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lofelt.NiceVibrations;

public class HapticController : SingletonMonoBehaviour<HapticController>
{
    private const float HapticCooldown = 0.1f;
    private float lastHapticTime = 0f;
    public void PlayHaptic()
    {
        if (PlayerprefSave.Haptic == 0)
        {
            return; // Haptics are disabled in settings
        }
        if (Time.time - lastHapticTime < HapticCooldown)
        {
            return; // Prevent haptic spam
        }
        lastHapticTime = Time.time;
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
    }
}
