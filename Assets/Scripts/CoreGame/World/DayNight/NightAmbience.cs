using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Night-only mood that follows the player: a teal colour grade and soft moonlight shafts, both faded in by DayNightCycle.NightFactor.</summary>
public class NightAmbience : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField] private DayNightCycle dayNight;
    [SerializeField] private Transform followTarget;

    [Header("Grade")]
    [SerializeField] private Volume nightVolume;

    [Header("Moon Shafts")]
    [SerializeField] private Renderer[] shafts;
    [SerializeField] private Color shaftColor = new Color(0.45f, 0.85f, 0.9f, 1f);
    [SerializeField] private float shaftIntensity = 0.35f;

    private MaterialPropertyBlock block;

    private void Awake() => block = new MaterialPropertyBlock();

    private void LateUpdate()
    {
        transform.position = followTarget.position;

        float night = dayNight.NightFactor;
        nightVolume.weight = night;

        Color shaft = shaftColor * (shaftIntensity * night);
        bool visible = night > 0.01f;
        for (int i = 0; i < shafts.Length; i++)
        {
            shafts[i].enabled = visible;
            if (!visible) continue;

            shafts[i].GetPropertyBlock(block);
            block.SetColor(BaseColorId, shaft);
            shafts[i].SetPropertyBlock(block);
        }
    }
}
