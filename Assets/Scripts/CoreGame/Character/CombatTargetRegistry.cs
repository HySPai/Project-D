using System.Collections.Generic;
using UnityEngine;

public static class CombatTargetRegistry
{
    private static readonly Dictionary<Collider, CharacterControllerBase> targets =
        new Dictionary<Collider, CharacterControllerBase>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnLoad()
    {
        targets.Clear();
    }

    public static void Register(Collider collider, CharacterControllerBase character)
    {
        if (ReferenceEquals(collider, null)) return;
        targets[collider] = character;
    }

    public static void Unregister(Collider collider)
    {
        if (ReferenceEquals(collider, null)) return;
        targets.Remove(collider);
    }

    public static bool TryGet(Collider collider, out CharacterControllerBase character)
    {
        return targets.TryGetValue(collider, out character);
    }
}
