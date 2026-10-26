using UnityEngine;
using UnityEngine.Rendering;

public class BlackoutFade : MonoBehaviour
{
    private const float FullBlackExposure = 10f;
    private const float MinVisibleLight = 0.001f;

    [SerializeField] private Volume blackoutVolume;

    /// <summary>Sets how black the screen is, 0 clear and 1 fully black; the volume carries a darkening exposure, so the weight is mapped to keep the fade even to the eye.</summary>
    public void SetBlackness(float blackness)
    {
        blackness = Mathf.Clamp01(blackness);
        float remainingLight = Mathf.Max(1f - blackness, MinVisibleLight);
        blackoutVolume.weight = blackness <= 0f ? 0f : Mathf.Clamp01(-Mathf.Log(remainingLight, 2f) / FullBlackExposure);
    }
}
