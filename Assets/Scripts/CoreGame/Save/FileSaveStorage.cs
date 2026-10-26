using System.IO;
using UnityEngine;

public class FileSaveStorage : ISaveStorage
{
    private readonly string directory;

    public FileSaveStorage() : this(Path.Combine(Application.persistentDataPath, "Saves")) { }

    public FileSaveStorage(string directory)
    {
        this.directory = directory;
        Directory.CreateDirectory(directory);
    }

    public bool Exists(string name) => File.Exists(PathOf(name));

    public string Read(string name) => File.ReadAllText(PathOf(name));

    public void Write(string name, string text) => File.WriteAllText(PathOf(name), text);

    public void Move(string from, string to)
    {
        string target = PathOf(to);
        if (File.Exists(target)) File.Delete(target);
        File.Move(PathOf(from), target);
    }

    public void Delete(string name)
    {
        string path = PathOf(name);
        if (File.Exists(path)) File.Delete(path);
    }

    private string PathOf(string name) => Path.Combine(directory, name);
}
