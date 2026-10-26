using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class SaveServiceTests
{
    private class MemoryStorage : ISaveStorage
    {
        public readonly Dictionary<string, string> files = new Dictionary<string, string>();
        public string failWriteFor;

        public bool Exists(string name) => files.ContainsKey(name);
        public string Read(string name) => files[name];

        public void Write(string name, string text)
        {
            if (name == failWriteFor) throw new System.IO.IOException("disk full");
            files[name] = text;
        }

        public void Move(string from, string to)
        {
            files[to] = files[from];
            files.Remove(from);
        }

        public void Delete(string name) => files.Remove(name);
    }

    private MemoryStorage storage;
    private SaveService service;

    [SetUp]
    public void SetUp()
    {
        storage = new MemoryStorage();
        service = new SaveService(storage);
    }

    private static SaveData Sample(string rest = "village", int remnants = 7)
    {
        var data = new SaveData { lastRestPointId = rest, equippedWeaponId = "SO_WeaponItem", carriedRemnants = remnants };
        data.AddFlag("rest.village");
        data.AddFlag("boss.spider_king");
        return data;
    }

    [Test]
    public void Load_NoFiles_ReturnsEmpty()
    {
        SaveLoadResult result = service.Load(0);

        Assert.AreEqual(SaveLoadStatus.Empty, result.status);
    }

    [Test]
    public void SaveThenLoad_RoundTripsEveryField()
    {
        service.Save(0, Sample());

        SaveLoadResult result = service.Load(0);

        Assert.AreEqual(SaveLoadStatus.Loaded, result.status);
        Assert.AreEqual("village", result.data.lastRestPointId);
        Assert.AreEqual("SO_WeaponItem", result.data.equippedWeaponId);
        Assert.AreEqual(7, result.data.carriedRemnants);
        Assert.IsTrue(result.data.HasFlag("boss.spider_king"));
        Assert.IsTrue(result.data.HasFlag("rest.village"));
    }

    [Test]
    public void Save_Twice_KeepsPreviousWriteAsBackup()
    {
        service.Save(0, Sample("first"));
        service.Save(0, Sample("second"));

        SaveData backup = JsonUtility.FromJson<SaveData>(storage.files[SaveService.BackupName(0)]);

        Assert.AreEqual("first", backup.lastRestPointId);
        Assert.AreEqual("second", service.Load(0).data.lastRestPointId);
    }

    [Test]
    public void Save_WriteFails_KeepsLastGoodFileAndReportsFailure()
    {
        service.Save(0, Sample("first"));
        storage.failWriteFor = SaveService.TempName(0);

        bool saved = service.Save(0, Sample("second"));

        Assert.IsFalse(saved);
        Assert.AreEqual("first", service.Load(0).data.lastRestPointId);
    }

    [Test]
    public void Load_MainCorruptBackupGood_RecoversFromBackup()
    {
        service.Save(0, Sample("first"));
        service.Save(0, Sample("second"));
        storage.files[SaveService.MainName(0)] = "{ not json";

        SaveLoadResult result = service.Load(0);

        Assert.AreEqual(SaveLoadStatus.RecoveredFromBackup, result.status);
        Assert.AreEqual("first", result.data.lastRestPointId);
    }

    [Test]
    public void Load_MainAndBackupCorrupt_ReturnsCorruptAndLeavesFilesAlone()
    {
        storage.files[SaveService.MainName(0)] = "garbage";
        storage.files[SaveService.BackupName(0)] = "more garbage";

        SaveLoadResult result = service.Load(0);

        Assert.AreEqual(SaveLoadStatus.Corrupt, result.status);
        Assert.AreEqual("garbage", storage.files[SaveService.MainName(0)]);
    }

    [Test]
    public void Load_VersionNewerThanGame_ReturnsTooNew()
    {
        var future = Sample();
        future.version = SaveData.CurrentVersion + 1;
        storage.files[SaveService.MainName(0)] = JsonUtility.ToJson(future);

        SaveLoadResult result = service.Load(0);

        Assert.AreEqual(SaveLoadStatus.TooNew, result.status);
        Assert.IsNull(result.data);
    }

    [Test]
    public void Delete_RemovesMainTempAndBackup()
    {
        service.Save(0, Sample());
        service.Save(0, Sample());

        service.Delete(0);

        Assert.AreEqual(SaveLoadStatus.Empty, service.Load(0).status);
        Assert.AreEqual(0, storage.files.Count);
    }

    [Test]
    public void Save_DifferentSlots_AreIndependent()
    {
        service.Save(0, Sample("a"));
        service.Save(2, Sample("c"));

        Assert.AreEqual("a", service.Load(0).data.lastRestPointId);
        Assert.AreEqual("c", service.Load(2).data.lastRestPointId);
        Assert.AreEqual(SaveLoadStatus.Empty, service.Load(1).status);
    }

    [Test]
    public void AddFlag_DuplicateOrEmpty_IsIgnored()
    {
        var data = new SaveData();

        bool first = data.AddFlag("x");
        bool duplicate = data.AddFlag("x");
        bool empty = data.AddFlag("");

        Assert.IsTrue(first);
        Assert.IsFalse(duplicate);
        Assert.IsFalse(empty);
        Assert.AreEqual(1, data.flags.Count);
    }
}
