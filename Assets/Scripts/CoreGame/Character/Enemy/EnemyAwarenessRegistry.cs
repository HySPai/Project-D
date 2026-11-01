using System.Collections.Generic;
using UnityEngine;

/// <summary>Enemies that currently have the player as their target.</summary>
public static class EnemyAwarenessRegistry
{
    private static readonly HashSet<CharacterStateBase> aware = new HashSet<CharacterStateBase>();
    private static readonly List<CharacterStateBase> stale = new List<CharacterStateBase>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnLoad()
    {
        aware.Clear();
    }

    public static void Add(CharacterStateBase enemy)
    {
        if (enemy != null) aware.Add(enemy);
    }

    public static void Remove(CharacterStateBase enemy)
    {
        if (!ReferenceEquals(enemy, null)) aware.Remove(enemy);
    }

    /// <summary>True while at least one living, active enemy is targeting the player.</summary>
    public static bool HasAwareEnemy
    {
        get
        {
            bool found = false;
            stale.Clear();

            foreach (CharacterStateBase enemy in aware)
            {
                if (enemy == null || enemy.IsDead || !enemy.isActiveAndEnabled)
                {
                    stale.Add(enemy);
                    continue;
                }

                found = true;
            }

            for (int i = 0; i < stale.Count; i++)
                aware.Remove(stale[i]);

            return found;
        }
    }
}
