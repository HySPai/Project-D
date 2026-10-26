/// <summary>Where save text lives; kept apart from SaveService so the write rules can be tested without a disk.</summary>
public interface ISaveStorage
{
    bool Exists(string name);
    string Read(string name);
    void Write(string name, string text);
    void Move(string from, string to);
    void Delete(string name);
}
