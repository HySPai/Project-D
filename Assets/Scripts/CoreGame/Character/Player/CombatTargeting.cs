using UnityEngine;

/// <summary>Stateless helpers shared by the player's aim assist and hit window.</summary>
public static class CombatTargeting
{
    private const float DirectionEpsilon = 0.0001f;

    /// <summary>Resolves a physics hit to a live character other than self through CombatTargetRegistry.</summary>
    public static bool TryResolveTarget(Collider targetCollider, CharacterControllerBase self, out CharacterControllerBase target)
    {
        if (!CombatTargetRegistry.TryGet(targetCollider, out target)) return false;
        if (target == null || target == self) return false;

        CharacterStateBase targetState = target.GetState;
        return targetState != null && !targetState.IsDead;
    }

    /// <summary>Horizontal capsule radius of a character in world units, or 0 without a CharacterController.</summary>
    public static float GetTargetRadius(CharacterControllerBase target)
    {
        CharacterController controller = target.CharacterController;
        if (controller == null) return 0f;

        Vector3 scale = target.transform.lossyScale;
        return controller.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
    }

    /// <summary>Forward of the transform flattened onto the ground plane, or world forward when it points straight up or down.</summary>
    public static Vector3 GetFlatForward(Transform transform)
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        return forward.sqrMagnitude > DirectionEpsilon ? forward.normalized : Vector3.forward;
    }
}
