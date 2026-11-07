using UnityEngine;

public class WaterController : MonoBehaviour
{
    private const int MaxRipples = 16;
    private const int MaxBodies = 8;

    private static readonly int RipplesId = Shader.PropertyToID("_WaterRipples");
    private static readonly int RippleDataId = Shader.PropertyToID("_WaterRippleData");
    private static readonly int RippleParamsId = Shader.PropertyToID("_WaterRippleParams");
    private static readonly int BodiesId = Shader.PropertyToID("_WaterBodies");
    private static readonly int BodyMotionId = Shader.PropertyToID("_WaterBodyMotion");
    private static readonly int TimeId = Shader.PropertyToID("_WaterTime");

    [SerializeField] private SO_WaterConfig config;
    [SerializeField] private ParticleSystem droplets;
    [SerializeField] private ParticleSystem foam;

    private readonly Vector4[] ripples = new Vector4[MaxRipples];
    private readonly Vector4[] rippleData = new Vector4[MaxRipples];
    private readonly Vector4[] bodies = new Vector4[MaxBodies];
    private readonly Vector4[] bodyMotion = new Vector4[MaxBodies];
    private int rippleCursor;
    private int bodyCount;

    public SO_WaterConfig Config => config;

    private void Awake()
    {
        ParticleSystem.MainModule main = droplets.main;
        main.gravityModifier = config.dropletGravity;

        for (int i = 0; i < MaxRipples; i++)
            ripples[i] = new Vector4(0f, 0f, 0f, -1000f);
    }

    private void LateUpdate()
    {
        Shader.SetGlobalFloat(TimeId, Time.time);
        Shader.SetGlobalVector(RippleParamsId, new Vector4(config.rippleLifetime, config.rippleWidth, config.trailLifetime, 0f));
        Shader.SetGlobalVectorArray(RipplesId, ripples);
        Shader.SetGlobalVectorArray(RippleDataId, rippleData);

        for (int i = bodyCount; i < MaxBodies; i++)
        {
            bodies[i] = Vector4.zero;
            bodyMotion[i] = Vector4.zero;
        }

        Shader.SetGlobalVectorArray(BodiesId, bodies);
        Shader.SetGlobalVectorArray(BodyMotionId, bodyMotion);
        bodyCount = 0;
    }

    /// <summary>Opens one small ring around the point of contact. Strength runs 0 to 1; bodyRadius is the size of what touched the water.</summary>
    public void AddRipple(Vector3 surfacePoint, float strength, float bodyRadius)
    {
        strength = Mathf.Clamp01(strength);

        WriteRipple(
            surfacePoint,
            new Vector4(strength, bodyRadius, bodyRadius + Mathf.Lerp(config.rippleSpread.x, config.rippleSpread.y, strength), 0f));
    }

    /// <summary>Leaves one thin clean ring at this point, sized to hug the body that just passed. SpeedRatio runs 0 to 1.</summary>
    public void AddTrailPatch(Vector3 surfacePoint, float speedRatio, float bodyRadius)
    {
        WriteRipple(
            surfacePoint,
            new Vector4(
                0.2f,
                bodyRadius * config.trailStartScale,
                bodyRadius + Mathf.Lerp(config.trailSpread.x, config.trailSpread.y, speedRatio),
                1f));
    }

    /// <summary>Marks something in the water this frame. Standing still it gets a tight ring; moving (horizontal velocity) turns the ring into a crescent trailing behind it.</summary>
    public void AddBody(Vector3 surfacePoint, float radius, Vector3 velocity)
    {
        if (bodyCount >= MaxBodies) return;

        velocity.y = 0f;
        float speed = velocity.magnitude;
        Vector3 heading = speed > 0.0001f ? velocity / speed : Vector3.zero;
        float moving = Mathf.InverseLerp(config.crescentMinSpeed, config.crescentFullSpeed, speed);

        bodies[bodyCount] = new Vector4(surfacePoint.x, surfacePoint.y, surfacePoint.z, radius);
        bodyMotion[bodyCount] = new Vector4(heading.x, heading.z, moving, 0f);
        bodyCount++;
    }

    private void WriteRipple(Vector3 surfacePoint, Vector4 data)
    {
        ripples[rippleCursor] = new Vector4(surfacePoint.x, surfacePoint.y, surfacePoint.z, Time.time);
        rippleData[rippleCursor] = data;
        rippleCursor = (rippleCursor + 1) % MaxRipples;
    }

    /// <summary>A fall into the water: one ring, droplets and a little foam, all scaled by strength (0 to 1).</summary>
    public void Splash(Vector3 surfacePoint, float strength, float radius)
    {
        strength = Mathf.Clamp01(strength);

        int dropletTotal = Mathf.RoundToInt(Mathf.Lerp(config.dropletCount.x, config.dropletCount.y, strength));
        EmitDroplets(surfacePoint, dropletTotal, Mathf.Lerp(config.dropletSpeed.x, config.dropletSpeed.y, strength), radius);
        EmitFoam(surfacePoint, strength, radius);
        AddRipple(surfacePoint, strength, radius);
    }

    /// <summary>A few droplets kicked up by something running through the water, with no ring.</summary>
    public void Spray(Vector3 surfacePoint)
    {
        EmitDroplets(surfacePoint, config.wadeSprayCount, config.dropletSpeed.x, config.wadeSprayRadius);
    }

    private void EmitDroplets(Vector3 point, int count, float topSpeed, float radius)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 ring = Random.insideUnitCircle;
            Vector3 outward = new Vector3(ring.x, 0f, ring.y);

            ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams
            {
                position = point + outward * radius,
                velocity = (Vector3.up + outward * config.dropletSpread).normalized * Random.Range(topSpeed * 0.45f, topSpeed),
                startSize = Random.Range(config.dropletSize.x, config.dropletSize.y),
                startLifetime = Random.Range(config.dropletLifetime.x, config.dropletLifetime.y)
            };
            droplets.Emit(emit, 1);
        }
    }

    private void EmitFoam(Vector3 point, float strength, float radius)
    {
        int count = Mathf.RoundToInt(Mathf.Lerp(config.foamCount.x, config.foamCount.y, strength));

        for (int i = 0; i < count; i++)
        {
            Vector2 ring = Random.insideUnitCircle;
            Vector3 outward = new Vector3(ring.x, 0f, ring.y);

            ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams
            {
                position = point + outward * (radius + config.foamSpread * strength),
                velocity = Vector3.up * config.foamRiseSpeed * strength,
                startSize = Mathf.Lerp(config.foamSize.x, config.foamSize.y, strength) * Random.Range(0.6f, 1f),
                startLifetime = Random.Range(config.foamLifetime.x, config.foamLifetime.y)
            };
            foam.Emit(emit, 1);
        }
    }
}
