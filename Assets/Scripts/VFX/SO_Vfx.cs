using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CreateAssetMenu(fileName = "SO_Vfx", menuName = "Vfx/SO_vfx", order = 1)]
[Serializable]
public class SO_Vfx : ScriptableObject
{
    [SerializeField] string[] vfxNames;
    [Button("Generate Enum")]
    public void GenarateEnum()
    {
        EnumGenerator.GenerateEnum("VFXName", "Assets/Scripts/VFX/", vfxNames);
        // Nếu itemVfxs chưa có thì tạo mảng mới
        if (itemVfxs == null)
            itemVfxs = new ItemVfx[vfxNames.Length];

        // Resize nếu số lượng không khớp
        if (itemVfxs.Length != vfxNames.Length)
        {
            Array.Resize(ref itemVfxs, vfxNames.Length);
        }

        // Update từng phần tử
        for (int i = 0; i < vfxNames.Length; i++)
        {
            if (itemVfxs[i] == null)
                itemVfxs[i] = new ItemVfx();

            // Cập nhật enum theo index
            itemVfxs[i].vfxName = (VFXName)i;
        }
    }
    [SerializeField,TableList,ShowIf("@vfxNames != null && vfxNames.Length > 0")]
    private ItemVfx[] itemVfxs;

    public GameObject GetVfxPrefab(VFXName vfxName)
    {
        foreach (var item in itemVfxs)
        {
            if (item.vfxName == vfxName)
            {
                return item.vfxPrefab;
            }
        }
        Debug.LogError("Không tìm thấy VFX: " + vfxName);
        return null;
    }
}
[Serializable]
public class ItemVfx
{
    public VFXName vfxName;
    public GameObject vfxPrefab;
}
