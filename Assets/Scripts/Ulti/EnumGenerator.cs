#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.IO;

public static class EnumGenerator
{
    public static void GenerateEnum(string enumName,string path, string[] values)
    {
        string _path = path + enumName + ".cs";

        using (StreamWriter sw = new StreamWriter(_path))
        {
            sw.WriteLine("public enum " + enumName);
            sw.WriteLine("{");

            foreach (string val in values)
            {
                string safeVal = val.Replace(" ", "_");
                sw.WriteLine("    " + safeVal + ",");
            }

            sw.WriteLine("}");
        }

        Debug.Log("✅ Enum generated at: " + _path);
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}
