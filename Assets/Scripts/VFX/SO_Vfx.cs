using System;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>Maps each VFXName to its prefab. Entries are stored in enum order, so lookups are a direct index.</summary>
[CreateAssetMenu(fileName = "SO_Vfx", menuName = "Vfx/SO_vfx", order = 1)]
public class SO_Vfx : ScriptableObject
{
    [SerializeField] private string[] vfxNames;

    [SerializeField, TableList, ShowIf("@vfxNames != null && vfxNames.Length > 0")]
    private ItemVfx[] itemVfxs;

#if UNITY_EDITOR
    [Button("Generate Enum")]
    private void GenerateEnum()
    {
        EnumGenerator.GenerateEnum("VFXName", "Assets/Scripts/VFX/", vfxNames);

        if (itemVfxs == null) itemVfxs = new ItemVfx[vfxNames.Length];
        if (itemVfxs.Length != vfxNames.Length) Array.Resize(ref itemVfxs, vfxNames.Length);

        for (int i = 0; i < vfxNames.Length; i++)
        {
            if (itemVfxs[i] == null) itemVfxs[i] = new ItemVfx();
            itemVfxs[i].vfxName = (VFXName)i;
        }
    }
#endif

    /// <summary>The prefab registered for vfxName, or null with an error when none is.</summary>
    public GameObject GetVfxPrefab(VFXName vfxName)
    {
        int index = (int)vfxName;
        if (itemVfxs != null && index >= 0 && index < itemVfxs.Length && itemVfxs[index] != null && itemVfxs[index].vfxName == vfxName)
            return itemVfxs[index].vfxPrefab;

        if (itemVfxs != null)
        {
            foreach (ItemVfx item in itemVfxs)
            {
                if (item != null && item.vfxName == vfxName) return item.vfxPrefab;
            }
        }

        Debug.LogError($"VFX not registered: {vfxName}", this);
        return null;
    }
}

/// <summary>One VFXName and the prefab it spawns.</summary>
[Serializable]
public class ItemVfx
{
    public VFXName vfxName;
    public GameObject vfxPrefab;
}
