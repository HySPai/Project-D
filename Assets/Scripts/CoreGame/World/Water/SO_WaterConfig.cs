using UnityEngine;

[CreateAssetMenu(fileName = "SO_WaterConfig", menuName = "ScriptableObjects/World/Water Config")]
public class SO_WaterConfig : ScriptableObject
{
    [Header("Entry Ring")]
    public float rippleLifetime = 0.7f;
    public float rippleWidth = 0.06f;
    public Vector2 rippleSpread = new Vector2(0.22f, 0.65f);
    public float gentleEntryStrength = 0.15f;

    [Header("Moving Trail")]
    public float trailMinSpeed = 0.4f;
    public float trailSpacing = 1.8f;
    public float trailLifetime = 0.45f;
    public float trailBehindFactor = 0.35f;
    public float trailStartScale = 0.5f;
    [Range(0f, 1f)] public float trailPatchStrength = 0.2f;
    public Vector2 trailSpread = new Vector2(0.04f, 0.1f);

    [Header("Moving Crescent")]
    public float crescentMinSpeed = 0.2f;
    public float crescentFullSpeed = 2.5f;
    public float trailFullSpeed = 5f;
    public float velocitySmoothing = 9f;

    [Header("Splash")]
    public float splashMinSpeed = 1.5f;
    public float splashMaxSpeed = 9f;

    [Header("Wading Spray")]
    public float wadeSpraySpeed = 3f;
    public float wadeSpraySpacing = 1.4f;
    public int wadeSprayCount = 2;
    public float wadeSprayRadius = 0.15f;

    [Header("Splash Droplets")]
    public Vector2Int dropletCount = new Vector2Int(2, 14);
    public Vector2 dropletSpeed = new Vector2(1f, 3.4f);
    public Vector2 dropletSize = new Vector2(0.02f, 0.06f);
    public Vector2 dropletLifetime = new Vector2(0.3f, 0.6f);
    public float dropletSpread = 0.45f;
    [Range(0f, 1f)] public float dropletMinSpeedRatio = 0.45f;
    public float dropletGravity = 1.6f;

    [Header("Splash Foam")]
    public Vector2Int foamCount = new Vector2Int(0, 4);
    public Vector2 foamSize = new Vector2(0.07f, 0.22f);
    public Vector2 foamLifetime = new Vector2(0.25f, 0.5f);
    public float foamSpread = 0.15f;
    public float foamRiseSpeed = 0.25f;
    [Range(0f, 1f)] public float foamSizeJitterMin = 0.6f;

    [Header("Buoyancy")]
    public float waterDensity = 1000f;
    public float displacementFactor = 1f;
    public float linearDrag = 2.6f;
    public float angularDrag = 2f;
    public float bobAmount = 0.12f;
    public float bobSpeed = 1.5f;

    [Header("Distance Bake")]
    public int bakeTexelsPerMeter = 8;
    public float bakeMaxDistance = 6f;
    public float bakeProbeHeight = 0.04f;
}
