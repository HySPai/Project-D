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

    private const float VisibleThreshold = 0.01f;
    private const float ChangeThreshold = 0.001f;

    private MaterialPropertyBlock block;
    private float appliedNight = -1f;
    private bool appliedVisible;
    private bool hasApplied;

    private void Awake() => block = new MaterialPropertyBlock();

    private void OnValidate() => hasApplied = false;

    private void LateUpdate()
    {
        transform.position = followTarget.position;

        float night = dayNight.NightFactor;
        bool visible = night > VisibleThreshold;
        if (hasApplied && visible == appliedVisible && Mathf.Abs(night - appliedNight) <= ChangeThreshold) return;

        hasApplied = true;
        appliedNight = night;
        nightVolume.weight = night;

        if (visible != appliedVisible || !visible)
        {
            for (int i = 0; i < shafts.Length; i++)
                shafts[i].enabled = visible;
        }
        appliedVisible = visible;

        if (!visible) return;

        Color shaft = shaftColor * (shaftIntensity * night);
        for (int i = 0; i < shafts.Length; i++)
        {
            shafts[i].GetPropertyBlock(block);
            block.SetColor(BaseColorId, shaft);
            shafts[i].SetPropertyBlock(block);
        }
    }
}
