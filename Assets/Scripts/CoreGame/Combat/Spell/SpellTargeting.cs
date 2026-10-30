using System.Collections.Generic;
using UnityEngine;

public static class SpellTargeting
{
    /// <summary>True when point lies inside the horizontal cone of arcDegrees centred on forward; a point on the origin counts as inside.</summary>
    public static bool IsWithinArc(Vector3 origin, Vector3 forward, Vector3 point, float arcDegrees)
    {
        Vector3 toPoint = point - origin;
        toPoint.y = 0f;
        forward.y = 0f;

        if (toPoint.sqrMagnitude < 0.0001f) return true;
        if (forward.sqrMagnitude < 0.0001f) return false;

        return Vector3.Angle(forward, toPoint) <= arcDegrees * 0.5f;
    }

    /// <summary>Fills results with every living character other than caster whose collider overlaps the sphere, each listed once.</summary>
    public static int CollectTargets(
        Vector3 center,
        float radius,
        LayerMask layer,
        CharacterControllerBase caster,
        Collider[] buffer,
        List<CharacterControllerBase> results)
    {
        results.Clear();

        int count = Physics.OverlapSphereNonAlloc(center, radius, buffer, layer, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            if (!CombatTargetRegistry.TryGet(buffer[i], out CharacterControllerBase target)) continue;
            if (target == null || target == caster) continue;

            CharacterStateBase state = target.GetState;
            if (state == null || state.IsDead) continue;
            if (results.Contains(target)) continue;

            results.Add(target);
        }

        return results.Count;
    }
}
