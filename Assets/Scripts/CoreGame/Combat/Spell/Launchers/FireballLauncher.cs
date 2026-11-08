using UnityEngine;

/// <summary>Fire skill: throws a pooled fireball straight along the cast direction.</summary>
public sealed class FireballLauncher : ISpellLauncher
{
    private readonly SO_FireballData data;
    private readonly Fireball prefab;
    private readonly SpellCastContext context;

    public FireballLauncher(SO_FireballData data, Fireball prefab, SpellCastContext context)
    {
        this.data = data;
        this.prefab = prefab;
        this.context = context;
    }

    public ElementType Element => ElementType.Fire;
    public bool IsBusy => false;

    public bool TryLaunch()
    {
        if (data == null || prefab == null) return false;

        Vector3 direction = context.FaceCastDirection();
        Vector3 position = context.CasterTransform.position
                           + Vector3.up * data.spawnHeight
                           + direction * data.spawnForward;

        ObjectPool.Get(prefab).Launch(data, context.Character, position, direction);
        GameLog.Info("[Phép] Cầu Lửa");
        return true;
    }
}
