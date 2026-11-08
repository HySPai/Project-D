using DG.Tweening;
using UnityEngine;

/// <summary>Spawns pooled visual effects by name and hides each one again after its lifetime, measured in game time.</summary>
public class EffectController : SingletonMonoBehaviour<EffectController>
{
    [SerializeField] private SO_Vfx so_Vfx;

    protected override void OnDestroy()
    {
        ObjectPool.Clear();
        base.OnDestroy();
    }

    /// <summary>
    /// Spawns a pooled effect facing rotation and hides it after lifetime seconds; the effect restarts even when the
    /// pooled instance was already playing.
    /// </summary>
    public void SpawnFx(VFXName vfxName, Vector3 position, Quaternion rotation, float lifetime)
    {
        GameObject prefab = so_Vfx.GetVfxPrefab(vfxName);
        if (prefab == null) return;

        GameObject obj = ObjectPool.Get(prefab);
        obj.SetActive(false);
        obj.transform.SetParent(transform, false);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        DOVirtual.DelayedCall(lifetime, () => obj.SetActive(false), false).SetLink(obj);
    }
}
