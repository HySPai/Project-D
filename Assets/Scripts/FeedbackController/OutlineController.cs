using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineController : SingletonMonoBehaviour<OutlineController>
{
    [SerializeField] private Color outlineColor = Color.green;
    [SerializeField] private float outlineWidth = 2f;
    public void AddOutline(GameObject target)
    {
        if (target == null)
        {
            Debug.LogWarning("Target GameObject is null.");
            return;
        }
        QuickOutline outline = target.GetComponent<QuickOutline>();
        if (outline == null)
        {
            outline = target.AddComponent<QuickOutline>();
            outline.OutlineColor = outlineColor;
            outline.OutlineWidth = outlineWidth;
        }
    }
    public void RemoveOutline(GameObject target)
    {
        if(target == null)
        {
            Debug.LogWarning("Target GameObject is null.");
            return;
        }
        if(target.TryGetComponent<QuickOutline>(out QuickOutline outline))
        {
            Destroy(outline);
        }
        else
        {
            Debug.LogWarning("QuickOutline component not found on the target GameObject.");
        }
    }
}
