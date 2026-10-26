using System;
using System.Collections.Generic;

/// <summary>One play-through's persisted progress; bump CurrentVersion whenever the layout changes.</summary>
[Serializable]
public class SaveData
{
    public const int CurrentVersion = 1;
    public const float DefaultHour = 9f;

    public int version = CurrentVersion;
    public float hourOfDay = DefaultHour;
    public string equippedWeaponId = string.Empty;
    public string lastRestPointId = string.Empty;
    public List<string> flags = new List<string>();
    public int carriedRemnants;
    public float playTimeSeconds;
    public string savedAtUtc = string.Empty;

    public bool HasFlag(string flag) => flags.Contains(flag);

    public bool AddFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag) || flags.Contains(flag)) return false;

        flags.Add(flag);
        return true;
    }
}
