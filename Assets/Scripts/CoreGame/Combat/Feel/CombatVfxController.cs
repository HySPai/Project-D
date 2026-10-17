using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Pooled hit blood: one world-space droplet system plus a ring of decal projectors.
/// Lives on the GameObject that owns the droplet ParticleSystem so it receives OnParticleCollision.
/// Example: combatVfx.PlayBlood(enemy.position, enemy.position - player.position, HitFeelTier.Heavy, enemyBlood);
/// </summary>
public class CombatVfxController : MonoBehaviour
{
    private const int CollisionEventCapacity = 64;

    [SerializeField] private CombatVfxSettings settings;
    [SerializeField] private ParticleSystem bloodDroplets;
    [SerializeField] private ParticleSystem bloodStreaks;
    [SerializeField] private ParticleSystem bloodSplash;
    [SerializeField] private Camera viewCamera;
    [SerializeField] private DecalProjector decalTemplate;
    [SerializeField] private Transform decalRoot;

    private readonly List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>(CollisionEventCapacity);

    private DecalProjector[] decals;
    private Sequence[] decalSequences;
    private int nextDecal;

    private HitBloodProfile lastProfile;
    private BloodBurstSettings lastBurst;
    private ParticleSystem.EmitParams emitParams;

    private void Awake()
    {
        if (settings == null || bloodDroplets == null || decalTemplate == null)
        {
            Debug.LogError($"{nameof(CombatVfxController)} on {name}: settings, droplets or decal template not assigned.", this);
            enabled = false;
            return;
        }

        ParticleSystem.MainModule main = bloodDroplets.main;
        main.maxParticles = settings.MaxDroplets;

        ParticleSystem.CollisionModule collision = bloodDroplets.collision;
        collision.collidesWith = settings.DropletCollisionLayers;

        if (bloodStreaks != null)
        {
            ParticleSystem.MainModule streakMain = bloodStreaks.main;
            streakMain.maxParticles = settings.MaxStreaks;
        }

        if (bloodSplash != null)
        {
            ParticleSystem.MainModule splashMain = bloodSplash.main;
            splashMain.maxParticles = settings.MaxSplashes;
        }

        decalTemplate.gameObject.SetActive(false);

        int size = settings.DecalPoolSize;
        decals = new DecalProjector[size];
        decalSequences = new Sequence[size];
        Transform parent = decalRoot != null ? decalRoot : transform;

        for (int i = 0; i < size; i++)
        {
            DecalProjector decal = Instantiate(decalTemplate, parent);
            decal.gameObject.SetActive(false);
            decals[i] = decal;
        }

        emitParams = new ParticleSystem.EmitParams { applyShapeToPosition = false };
    }

    private void OnDestroy()
    {
        if (decalSequences == null) return;

        for (int i = 0; i < decalSequences.Length; i++)
            decalSequences[i]?.Kill();
    }

    /// <summary>
    /// Sprays droplets out of the far side of the target along the hit direction.
    /// Example: PlayBlood(target.position, target.position - attacker.position, HitFeelTier.Light, profile);
    /// </summary>
    public void PlayBlood(Vector3 targetPosition, Vector3 hitDirection, HitFeelTier tier, HitBloodProfile profile)
    {
        if (!enabled || profile == null) return;

        BloodBurstSettings burst = profile.GetBurst(tier);
        if (burst == null) return;

        Vector3 direction = hitDirection;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;
        direction.Normalize();

        Vector3 origin = targetPosition + Vector3.up * profile.SpawnHeight + direction * profile.SpawnBackOffset;
        Quaternion basis = Quaternion.LookRotation(direction);

        EmitSplash(origin, direction, burst, profile);
        EmitStreaks(origin, basis, burst, profile);
        EmitDroplets(origin, basis, burst, profile);

        lastProfile = profile;
        lastBurst = burst;
    }

    private void EmitSplash(Vector3 origin, Vector3 direction, BloodBurstSettings burst, HitBloodProfile profile)
    {
        if (bloodSplash == null || viewCamera == null || burst.splashCount <= 0) return;

        Transform cam = viewCamera.transform;
        float screenAngle = Mathf.Atan2(Vector3.Dot(direction, cam.up), Vector3.Dot(direction, cam.right)) * Mathf.Rad2Deg;
        Vector3 towardCamera = -cam.forward * settings.SplashCameraOffset;
        Vector3 screenDirection = (cam.right * Mathf.Cos(screenAngle * Mathf.Deg2Rad)
            + cam.up * Mathf.Sin(screenAngle * Mathf.Deg2Rad));

        for (int i = 0; i < burst.splashCount; i++)
        {
            float spread = Random.Range(-burst.splashSpreadAngle, burst.splashSpreadAngle);

            emitParams.position = origin + towardCamera;
            emitParams.velocity = screenDirection * burst.splashDriftSpeed;
            emitParams.startSize = Random.Range(burst.splashSizeRange.x, burst.splashSizeRange.y);
            emitParams.startLifetime = Random.Range(burst.splashLifetimeRange.x, burst.splashLifetimeRange.y);
            emitParams.startColor = profile.SplashColor;
            emitParams.rotation = screenAngle + spread;
            bloodSplash.Emit(emitParams, 1);
        }

        emitParams.rotation = 0f;
    }

    private void EmitStreaks(Vector3 origin, Quaternion basis, BloodBurstSettings burst, HitBloodProfile profile)
    {
        if (bloodStreaks == null || burst.streakCount <= 0) return;

        ParticleSystem.LimitVelocityOverLifetimeModule limit = bloodStreaks.limitVelocityOverLifetime;
        limit.drag = profile.StreakDrag;

        float coneTangent = Mathf.Tan(burst.streakConeAngle * Mathf.Deg2Rad);
        for (int i = 0; i < burst.streakCount; i++)
        {
            emitParams.position = origin;
            emitParams.velocity = basis * ConeDirection(coneTangent, burst.streakUpwardBias)
                * Random.Range(burst.streakSpeedRange.x, burst.streakSpeedRange.y);
            emitParams.startSize = Random.Range(burst.streakWidthRange.x, burst.streakWidthRange.y);
            emitParams.startLifetime = Random.Range(burst.streakLifetimeRange.x, burst.streakLifetimeRange.y);
            emitParams.startColor = profile.StreakColor;
            bloodStreaks.Emit(emitParams, 1);
        }
    }

    private void EmitDroplets(Vector3 origin, Quaternion basis, BloodBurstSettings burst, HitBloodProfile profile)
    {
        int dropletCount = Random.Range(
            Mathf.Max(0, burst.dropletCountRange.x),
            Mathf.Max(burst.dropletCountRange.x, burst.dropletCountRange.y) + 1);
        if (dropletCount <= 0) return;

        ParticleSystem.MainModule main = bloodDroplets.main;
        main.gravityModifier = profile.GravityModifier;

        float coneTangent = Mathf.Tan(burst.coneAngle * Mathf.Deg2Rad);
        for (int i = 0; i < dropletCount; i++)
        {
            emitParams.position = origin;
            emitParams.velocity = basis * ConeDirection(coneTangent, burst.upwardBias)
                * Random.Range(burst.speedRange.x, burst.speedRange.y);
            emitParams.startSize = Random.Range(burst.dropletSizeRange.x, burst.dropletSizeRange.y);
            emitParams.startLifetime = Random.Range(burst.dropletLifetimeRange.x, burst.dropletLifetimeRange.y);
            emitParams.startColor = profile.DropletColor;
            bloodDroplets.Emit(emitParams, 1);
        }
    }

    private static Vector3 ConeDirection(float coneTangent, float upwardBias)
    {
        Vector2 spread = Random.insideUnitCircle * coneTangent;
        return new Vector3(spread.x, spread.y + upwardBias, 1f).normalized;
    }

    private void OnParticleCollision(GameObject other)
    {
        if (lastProfile == null || lastBurst == null) return;

        int count = bloodDroplets.GetCollisionEvents(other, collisionEvents);
        for (int i = 0; i < count; i++)
        {
            if (Random.value > lastBurst.decalChance) continue;

            ParticleCollisionEvent hit = collisionEvents[i];
            if (hit.normal.y < settings.MinDecalNormalY) continue;

            SpawnDecal(hit.intersection, hit.normal, lastProfile, lastBurst);
        }
    }

    private void SpawnDecal(Vector3 point, Vector3 normal, HitBloodProfile profile, BloodBurstSettings burst)
    {
        if (normal.sqrMagnitude < 0.0001f) normal = Vector3.up;

        if (decalSequences == null || decalSequences.Length != decals.Length)
            decalSequences = new Sequence[decals.Length];

        int index = nextDecal;
        nextDecal = (nextDecal + 1) % decals.Length;

        DecalProjector decal = decals[index];
        decalSequences[index]?.Kill();

        float size = Random.Range(burst.decalSizeRange.x, burst.decalSizeRange.y);
        Transform decalTransform = decal.transform;
        decalTransform.position = point + normal * settings.DecalSurfaceOffset;
        decalTransform.rotation = Quaternion.LookRotation(-normal) * Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        if (profile.DecalMaterial != null)
            decal.material = profile.DecalMaterial;

        float depth = profile.DecalProjectionDepth;
        decal.size = new Vector3(size, size, depth);
        decal.pivot = new Vector3(0f, 0f, depth * 0.5f);
        decal.fadeFactor = profile.DecalOpacity;
        decal.gameObject.SetActive(true);

        decalSequences[index] = DOTween.Sequence()
            .AppendInterval(profile.DecalStayDuration)
            .Append(DOTween.To(() => decal.fadeFactor, v => decal.fadeFactor = v, 0f, profile.DecalFadeDuration))
            .OnComplete(() => decal.gameObject.SetActive(false))
            .SetLink(decal.gameObject);
    }
}
