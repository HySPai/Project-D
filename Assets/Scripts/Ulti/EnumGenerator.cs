#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;

/// <summary>Editor helper that writes a C# enum file from a list of names, spaces turned into underscores.</summary>
public static class EnumGenerator
{
    /// <summary>Writes path/enumName.cs containing one member per value and refreshes the asset database.</summary>
    public static void GenerateEnum(string enumName, string path, string[] values)
    {
        string filePath = path + enumName + ".cs";
        var sb = new StringBuilder();
        sb.AppendLine("public enum " + enumName);
        sb.AppendLine("{");
        foreach (string value in values)
            sb.AppendLine("    " + value.Replace(" ", "_") + ",");
        sb.AppendLine("}");

        File.WriteAllText(filePath, sb.ToString());
        AssetDatabase.Refresh();
    }
}
#endif
