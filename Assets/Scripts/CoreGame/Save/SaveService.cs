using System;
using System.IO;
using UnityEngine;

public enum SaveLoadStatus
{
    Empty,
    Loaded,
    RecoveredFromBackup,
    TooNew,
    Corrupt
}

public readonly struct SaveLoadResult
{
    public readonly SaveLoadStatus status;
    public readonly SaveData data;

    public SaveLoadResult(SaveLoadStatus status, SaveData data)
    {
        this.status = status;
        this.data = data;
    }

    public bool HasData => status == SaveLoadStatus.Loaded || status == SaveLoadStatus.RecoveredFromBackup;
}

/// <summary>Reads and writes the save slots; a write never damages the last good file.</summary>
public class SaveService
{
    public const int SlotCount = 3;

    private readonly ISaveStorage storage;

    public SaveService(ISaveStorage storage) => this.storage = storage;

    public static string MainName(int slot) => $"slot_{slot}.json";
    public static string TempName(int slot) => $"slot_{slot}.json.tmp";
    public static string BackupName(int slot) => $"slot_{slot}.json.bak";

    /// <summary>Writes the new text beside the old file first, then swaps it in, keeping the previous one as the backup.</summary>
    public bool Save(int slot, SaveData data)
    {
        try
        {
            string main = MainName(slot);
            string temp = TempName(slot);

            storage.Write(temp, JsonUtility.ToJson(data));
            if (storage.Exists(main)) storage.Move(main, BackupName(slot));
            storage.Move(temp, main);
            return true;
        }
        catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
        {
            Debug.LogWarning($"Save slot {slot} failed: {e.Message}");
            return false;
        }
    }

    public SaveLoadResult Load(int slot)
    {
        bool hasMain = storage.Exists(MainName(slot));
        bool hasBackup = storage.Exists(BackupName(slot));
        if (!hasMain && !hasBackup) return new SaveLoadResult(SaveLoadStatus.Empty, null);

        if (hasMain && TryParse(MainName(slot), out SaveData main))
            return Classify(main, SaveLoadStatus.Loaded);

        if (hasBackup && TryParse(BackupName(slot), out SaveData backup))
            return Classify(backup, SaveLoadStatus.RecoveredFromBackup);

        return new SaveLoadResult(SaveLoadStatus.Corrupt, null);
    }

    public void Delete(int slot)
    {
        storage.Delete(MainName(slot));
        storage.Delete(TempName(slot));
        storage.Delete(BackupName(slot));
    }

    private static SaveLoadResult Classify(SaveData data, SaveLoadStatus okStatus)
    {
        if (data.version > SaveData.CurrentVersion) return new SaveLoadResult(SaveLoadStatus.TooNew, null);

        data.version = SaveData.CurrentVersion;
        return new SaveLoadResult(okStatus, data);
    }

    private bool TryParse(string name, out SaveData data)
    {
        data = null;
        try
        {
            data = JsonUtility.FromJson<SaveData>(storage.Read(name));
        }
        catch (Exception e) when (e is ArgumentException || e is IOException || e is UnauthorizedAccessException)
        {
            return false;
        }

        return data != null && data.version >= 1;
    }
}
