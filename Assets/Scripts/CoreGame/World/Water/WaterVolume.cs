using System.Collections.Generic;
using UnityEngine;

/// <summary>A static body of water: tracks what wades through it for ripples and spray, floats rigidbodies and bakes the shore distance field.</summary>
[RequireComponent(typeof(BoxCollider))]
public class WaterVolume : MonoBehaviour
{
    private sealed class Contact
    {
        public Collider collider;
        public Vector3 lastPosition;
        public Vector3 velocity;
        public float trailTravelled;
        public float sprayTravelled;
        public float phase;
    }

    private static readonly int SdfTexId = Shader.PropertyToID("_WaterSdfTex");
    private static readonly int SdfRectId = Shader.PropertyToID("_WaterSdfRect");
    private static readonly int SdfMaxId = Shader.PropertyToID("_WaterSdfMax");

    private const float MinSurfaceRadiusSqr = 0.0025f;
    private const float MinSubmergeHeight = 0.001f;
    private const float FullCircle = Mathf.PI * 2f;
    private const float ByteMax = 255f;

    [SerializeField] private WaterController controller;
    [SerializeField] private BoxCollider volume;
    [SerializeField] private Renderer surface;
    [SerializeField] private LayerMask contactLayers;
    [SerializeField] private LayerMask solidLayers;

    private readonly List<Contact> contacts = new List<Contact>();
    private Texture2D distanceTexture;
    private float surfaceHeight;
    private bool surfaceCached;

    /// <summary>World height of the water surface, read once since the volume never moves.</summary>
    public float SurfaceHeight
    {
        get
        {
            if (!surfaceCached) CacheSurface();
            return surfaceHeight;
        }
    }

    /// <summary>Number of colliders currently touching the water.</summary>
    public int ContactCount => contacts.Count;

    private void Start()
    {
        Physics.SyncTransforms();
        CacheSurface();
        BakeDistanceField();
    }

    private void CacheSurface()
    {
        surfaceHeight = volume.bounds.max.y;
        surfaceCached = true;
    }

    private void OnDisable() => contacts.Clear();

    private void OnDestroy()
    {
        if (distanceTexture != null) Destroy(distanceTexture);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsContactCollider(other) || FindContact(other) != null) return;

        contacts.Add(new Contact
        {
            collider = other,
            lastPosition = other.bounds.center,
            phase = Random.value * FullCircle
        });

        PlayEntry(other);
    }

    private void OnTriggerExit(Collider other)
    {
        Contact contact = FindContact(other);
        if (contact != null) contacts.Remove(contact);
    }

    private void Update()
    {
        for (int i = contacts.Count - 1; i >= 0; i--)
        {
            Contact contact = contacts[i];
            if (contact.collider == null || !contact.collider.enabled || !contact.collider.gameObject.activeInHierarchy)
            {
                contacts.RemoveAt(i);
                continue;
            }

            TrackContact(contact);
        }
    }

    private void FixedUpdate()
    {
        for (int i = contacts.Count - 1; i >= 0; i--)
        {
            if (contacts[i].collider != null) ApplyBuoyancy(contacts[i]);
        }
    }

    private void TrackContact(Contact contact)
    {
        Bounds bounds = contact.collider.bounds;
        float surfaceHeight = SurfaceHeight;
        Vector3 center = bounds.center;
        Vector3 previous = contact.lastPosition;
        contact.lastPosition = center;

        if (bounds.min.y > surfaceHeight || bounds.max.y < surfaceHeight) return;

        SO_WaterConfig config = controller.Config;
        Vector3 surfacePoint = new Vector3(center.x, surfaceHeight, center.z);

        Vector3 step = center - previous;
        step.y = 0f;
        float deltaTime = Time.deltaTime;
        if (deltaTime > 0f)
        {
            float blend = 1f - Mathf.Exp(-config.velocitySmoothing * deltaTime);
            contact.velocity = Vector3.Lerp(contact.velocity, step / deltaTime, blend);
        }

        float radius = RadiusAtSurface(contact.collider, bounds, surfaceHeight);
        controller.AddBody(surfacePoint, radius, contact.velocity);

        float speed = contact.velocity.magnitude;
        float moved = step.magnitude;
        UpdateTrail(contact, surfacePoint, radius, speed, moved);
        UpdateSpray(contact, surfacePoint, speed, moved);
    }

    private void UpdateTrail(Contact contact, Vector3 surfacePoint, float radius, float speed, float moved)
    {
        SO_WaterConfig config = controller.Config;

        if (speed < config.trailMinSpeed)
        {
            contact.trailTravelled = 0f;
            return;
        }

        contact.trailTravelled += moved;
        if (contact.trailTravelled < config.trailSpacing) return;

        contact.trailTravelled = 0f;
        Vector3 behind = surfacePoint - contact.velocity / speed * (radius * config.trailBehindFactor);
        controller.AddTrailPatch(behind, Mathf.InverseLerp(config.trailMinSpeed, config.trailFullSpeed, speed), radius);
    }

    private void UpdateSpray(Contact contact, Vector3 surfacePoint, float speed, float moved)
    {
        SO_WaterConfig config = controller.Config;

        if (speed < config.wadeSpraySpeed)
        {
            contact.sprayTravelled = 0f;
            return;
        }

        contact.sprayTravelled += moved;
        if (contact.sprayTravelled < config.wadeSpraySpacing) return;

        contact.sprayTravelled = 0f;
        controller.Spray(surfacePoint);
    }

    private static float RadiusAtSurface(Collider collider, Bounds bounds, float surfaceHeight)
    {
        float radius = Mathf.Max(bounds.extents.x, bounds.extents.z);
        if (!(collider is SphereCollider)) return radius;

        float height = surfaceHeight - bounds.center.y;
        return Mathf.Sqrt(Mathf.Max(radius * radius - height * height, MinSurfaceRadiusSqr));
    }

    private void ApplyBuoyancy(Contact contact)
    {
        Rigidbody body = contact.collider.attachedRigidbody;
        if (body == null || body.isKinematic) return;

        SO_WaterConfig config = controller.Config;
        Bounds bounds = contact.collider.bounds;
        Vector3 size = bounds.size;
        float submerged = Mathf.Clamp01((SurfaceHeight - bounds.min.y) / Mathf.Max(size.y, MinSubmergeHeight));
        if (submerged <= 0f) return;

        float displaced = SolidVolume(contact.collider, size) * submerged * config.displacementFactor;
        float bob = 1f + Mathf.Sin(Time.time * config.bobSpeed + contact.phase) * config.bobAmount;
        float lift = config.waterDensity * displaced * -Physics.gravity.y * bob;

        body.AddForce(Vector3.up * lift, ForceMode.Force);
        body.AddForce(-body.linearVelocity * config.linearDrag * submerged, ForceMode.Acceleration);
        body.AddTorque(-body.angularVelocity * config.angularDrag * submerged, ForceMode.Acceleration);
    }

    private static float SolidVolume(Collider collider, Vector3 boundsSize)
    {
        Vector3 scale = collider.transform.lossyScale;

        if (collider is BoxCollider box)
            return Mathf.Abs(box.size.x * scale.x * box.size.y * scale.y * box.size.z * scale.z);

        if (collider is SphereCollider sphere)
        {
            float radius = sphere.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            return 4f / 3f * Mathf.PI * radius * radius * radius;
        }

        return boundsSize.x * boundsSize.y * boundsSize.z;
    }

    private void PlayEntry(Collider other)
    {
        SO_WaterConfig config = controller.Config;
        Bounds bounds = other.bounds;
        float surfaceHeight = SurfaceHeight;
        Vector3 point = new Vector3(bounds.center.x, surfaceHeight, bounds.center.z);
        float radius = RadiusAtSurface(other, bounds, surfaceHeight);
        float speed = EntrySpeed(other);

        if (speed < config.splashMinSpeed)
        {
            controller.AddRipple(point, config.gentleEntryStrength, radius);
            return;
        }

        controller.Splash(point, Mathf.InverseLerp(config.splashMinSpeed, config.splashMaxSpeed, speed), radius);
    }

    private static float EntrySpeed(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        if (body != null) return body.linearVelocity.magnitude;

        CharacterController character = other as CharacterController;
        return character != null ? character.velocity.magnitude : 0f;
    }

    private bool IsContactCollider(Collider other)
    {
        if (other.isTrigger) return false;

        return (contactLayers.value & (1 << other.gameObject.layer)) != 0;
    }

    private Contact FindContact(Collider other)
    {
        for (int i = 0; i < contacts.Count; i++)
        {
            if (contacts[i].collider == other) return contacts[i];
        }

        return null;
    }

    private void BakeDistanceField()
    {
        SO_WaterConfig config = controller.Config;
        Bounds bounds = volume.bounds;
        float surfaceHeight = SurfaceHeight;
        int width = Mathf.Max(2, Mathf.CeilToInt(bounds.size.x * config.bakeTexelsPerMeter));
        int height = Mathf.Max(2, Mathf.CeilToInt(bounds.size.z * config.bakeTexelsPerMeter));
        float cellX = bounds.size.x / width;
        float cellZ = bounds.size.z / height;
        Vector3 halfCell = new Vector3(cellX * 0.5f, config.bakeProbeHeight, cellZ * 0.5f);

        bool[] solid = new bool[width * height];
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 probe = new Vector3(bounds.min.x + (x + 0.5f) * cellX, surfaceHeight, bounds.min.z + (z + 0.5f) * cellZ);
                solid[x + z * width] = Physics.CheckBox(probe, halfCell, Quaternion.identity, solidLayers, QueryTriggerInteraction.Ignore);
            }
        }

        float[] distances = WaterDistanceField.Compute(solid, width, height, Mathf.Min(cellX, cellZ));

        distanceTexture = new Texture2D(width, height, TextureFormat.R8, false, true)
        {
            name = name + "_Distance",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Color32[] pixels = new Color32[distances.Length];
        for (int i = 0; i < distances.Length; i++)
        {
            byte value = (byte)Mathf.RoundToInt(Mathf.Clamp01(distances[i] / config.bakeMaxDistance) * ByteMax);
            pixels[i] = new Color32(value, value, value, 255);
        }

        distanceTexture.SetPixels32(pixels);
        distanceTexture.Apply(false, true);

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        surface.GetPropertyBlock(block);
        block.SetTexture(SdfTexId, distanceTexture);
        block.SetVector(SdfRectId, new Vector4(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z));
        block.SetFloat(SdfMaxId, config.bakeMaxDistance);
        surface.SetPropertyBlock(block);
    }
}
