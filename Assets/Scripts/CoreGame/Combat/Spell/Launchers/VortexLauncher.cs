using UnityEngine;
using UnityEngine.AI;

/// <summary>Wind skill: opens a pooled vortex ahead of the caster, stopped short by navmesh edges and walls.</summary>
public sealed class VortexLauncher : ISpellLauncher
{
    private readonly SO_WindVortexData data;
    private readonly WindVortex prefab;
    private readonly SpellCastContext context;

    public VortexLauncher(SO_WindVortexData data, WindVortex prefab, SpellCastContext context)
    {
        this.data = data;
        this.prefab = prefab;
        this.context = context;
    }

    public ElementType Element => ElementType.Wind;
    public bool IsBusy => false;

    public bool TryLaunch()
    {
        if (data == null || prefab == null) return false;

        Vector3 direction = context.FaceCastDirection();
        Vector3 position = ResolvePosition(context.CasterTransform.position, direction * data.castDistance, data.navMeshSampleDistance);

        ObjectPool.Get(prefab).Launch(data, context.Character, position);
        GameLog.Info($"[Phép] Lốc Xoáy tại {position}");
        return true;
    }

    /// <summary>The point offset away from origin, pulled back to where the navmesh path toward it is first blocked.</summary>
    public static Vector3 ResolvePosition(Vector3 origin, Vector3 offset, float sampleDistance)
    {
        Vector3 desired = origin + offset;

        if (!NavMesh.SamplePosition(origin, out NavMeshHit start, sampleDistance, NavMesh.AllAreas))
            return desired;

        return NavMesh.Raycast(start.position, desired, out NavMeshHit hit, NavMesh.AllAreas)
            ? hit.position
            : desired;
    }
}
