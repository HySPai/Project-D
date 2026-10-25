using UnityEngine;

public static class CameraZoneMath
{
    /// <summary>True when a world point lies inside a box defined in the given local space.</summary>
    public static bool Contains(Matrix4x4 worldToLocal, Vector3 center, Vector3 size, Vector3 worldPoint)
    {
        Vector3 local = worldToLocal.MultiplyPoint3x4(worldPoint) - center;
        Vector3 half = size * 0.5f;
        return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
    }

    /// <summary>
    /// Passes values through untouched until <paramref name="softness"/> before the limit, then eases them
    /// toward <paramref name="halfExtent"/> without ever reaching it, so motion slows instead of stopping dead.
    /// </summary>
    public static float SoftLimit(float value, float halfExtent, float softness)
    {
        float soft = Mathf.Min(softness, halfExtent);
        if (soft <= 0f) return Mathf.Clamp(value, -halfExtent, halfExtent);

        float knee = halfExtent - soft;
        float magnitude = Mathf.Abs(value);
        if (magnitude <= knee) return value;

        float eased = knee + soft * (1f - Mathf.Exp(-(magnitude - knee) / soft));
        return Mathf.Sign(value) * eased;
    }

    /// <summary>Soft-limits a world point to a box on its local X and Z axes, leaving height untouched.</summary>
    public static Vector3 SoftClampXZ(
        Matrix4x4 worldToLocal, Matrix4x4 localToWorld, Vector3 center, Vector3 size, float softness, Vector3 worldPoint)
    {
        Vector3 local = worldToLocal.MultiplyPoint3x4(worldPoint);
        local.x = center.x + SoftLimit(local.x - center.x, size.x * 0.5f, softness);
        local.z = center.z + SoftLimit(local.z - center.z, size.z * 0.5f, softness);

        Vector3 result = localToWorld.MultiplyPoint3x4(local);
        result.y = worldPoint.y;
        return result;
    }

    /// <summary>0 at <paramref name="start"/>, 1 at <paramref name="end"/>, smoothstepped along the segment between them.</summary>
    public static float RampWeight(Vector3 localPoint, Vector3 start, Vector3 end)
    {
        Vector3 axis = end - start;
        float lengthSq = axis.sqrMagnitude;
        if (lengthSq < 0.0001f) return 1f;

        float t = Mathf.Clamp01(Vector3.Dot(localPoint - start, axis) / lengthSq);
        return t * t * (3f - 2f * t);
    }

    /// <summary>Frame-rate independent exponential approach; sharpness 0 or less snaps.</summary>
    public static float Damp(float current, float target, float sharpness, float deltaTime)
    {
        if (sharpness <= 0f) return target;
        return Mathf.Lerp(current, target, 1f - Mathf.Exp(-sharpness * deltaTime));
    }

    /// <summary>Vector form of <see cref="Damp(float, float, float, float)"/>.</summary>
    public static Vector3 Damp(Vector3 current, Vector3 target, float sharpness, float deltaTime)
    {
        if (sharpness <= 0f) return target;
        return Vector3.Lerp(current, target, 1f - Mathf.Exp(-sharpness * deltaTime));
    }
}
