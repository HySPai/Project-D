using UnityEngine;

public class AreaRoom : MonoBehaviour
{
    [SerializeField] private GameObject[] contents;
    [SerializeField] private bool darkOutside;

    public bool IsVisible { get; private set; } = true;

    /// <summary>Indoor rooms hide everything beyond their walls in black; outdoor maps show the sky.</summary>
    public bool DarkOutside => darkOutside;

    /// <summary>Turns everything that belongs to this room on or off, so only the current room exists on screen.</summary>
    public void SetVisible(bool visible)
    {
        IsVisible = visible;
        for (int i = 0; i < contents.Length; i++)
        {
            if (contents[i] != null) contents[i].SetActive(visible);
        }
    }
}
