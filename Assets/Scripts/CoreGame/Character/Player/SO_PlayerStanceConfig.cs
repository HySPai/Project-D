using UnityEngine;

[CreateAssetMenu(fileName = "SO_PlayerStanceConfig", menuName = "Game/Stats/Player Stance Config")]
public class SO_PlayerStanceConfig : ScriptableObject
{
    [Header("Detection")]
    public float combatExitDelay = 3f;

    [Header("Locomotion")]
    public float locomotionBlendTime = 0.35f;

    [Header("Equip Layer")]
    public float equipLayerFadeTime = 0.12f;

    [Header("Draw")]
    public string drawStateName = "Equip_Draw";
    public float drawDuration = 0.6f;
    public float weaponReadyTime = 0.35f;
    public float drawRevealDelay = 0.08f;
    public float drawRevealDuration = 0.4f;

    [Header("Sheathe")]
    public string sheatheStateName = "Equip_Sheathe";
    [Range(0f, 1f)] public float sheatheStartNormalizedTime = 1f;
    public float sheatheDuration = 0.6f;
    public float sheatheDissolveDelay = 0.1f;
    public float sheatheDissolveDuration = 0.35f;

    [Header("Reveal Look")]
    [Range(0.01f, 0.4f)] public float revealEdgeWidth = 0.08f;
    [ColorUsage(true, true)] public Color revealEdgeColor = new Color(2.4f, 1.6f, 0.6f, 1f);
    public float revealNoiseScale = 22f;
    [Range(0f, 0.3f)] public float revealNoiseStrength = 0.06f;
}
