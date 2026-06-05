using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Ifreet.Core.Runtime.Audio.Data;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Ifreet.Core.Audio.Editor
{
    [CustomEditor(typeof(AudioClipData))]
    public class AudioClipDataEditor : UnityEditor.Editor
    {
        private static class Paths
        {
            public const string GENERATED_FOLDER_PATH = "Assets/Scripts/Audio/Generated";
            public const string NAMESPACE_NAME = "Ifreet.Core.Runtime.Audio.Generated";

            public const string SFX_ENUM_NAME = "AudioSfxID";
            public const string SFX_FILE_NAME = "AudioSfxID.cs";

            public const string BG_ENUM_NAME = "AudioBackgroundID";
            public const string BG_FILE_NAME = "AudioBackgroundID.cs";
        }
        
        // PRIVATE MEMBERS: --------------------------------------------------------------------------------------------
        
        // --- Serialized ---------------------------------------------------------
        private SerializedProperty m_IDLibrary;
        private SerializedProperty m_Sfx;
        private SerializedProperty m_Background;

        // Library lists
        private ReorderableList m_SfxIDList;
        private ReorderableList m_BgIDList;

        // Foldouts (session-persistent)
        private const string KEY_SHOW_LIB = "AudioClipDataEditor.ShowLibrary";
        private const string KEY_SHOW_SFX = "AudioClipDataEditor.ShowSfx";
        private const string KEY_SHOW_BG  = "AudioClipDataEditor.ShowBg";

        private bool m_ShowLibrary;
        private bool m_ShowSfx;
        private bool m_ShowBackground;

        // INITIALIZERS: -----------------------------------------------------------------------------------------------
        
        private void OnEnable()
        {
            this.m_IDLibrary  = this.serializedObject.FindProperty("m_IDLibrary");
            this.m_Sfx        = this.serializedObject.FindProperty("m_Sfx");
            this.m_Background = this.serializedObject.FindProperty("m_Background");

            // Foldout states
            this.m_ShowLibrary   = SessionState.GetBool(KEY_SHOW_LIB, true);
            this.m_ShowSfx       = SessionState.GetBool(KEY_SHOW_SFX, true);
            this.m_ShowBackground= SessionState.GetBool(KEY_SHOW_BG,  true);

            // Reorderable lists for IDLibrary
            if (this.m_IDLibrary != null && this.m_IDLibrary.serializedObject != null)
            {
                var sfxIDs = this.m_IDLibrary.FindPropertyRelative("m_SfxID");
                var bgIDs  = this.m_IDLibrary.FindPropertyRelative("m_BackgroundID");

                this.m_SfxIDList = this.CreateIDList(sfxIDs, "SFX IDs");
                this.m_BgIDList  = this.CreateIDList(bgIDs,  "Background IDs");
            }
        }

        // GUI DRAW METHOD: --------------------------------------------------------------------------------------------
        
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            // --- LIBRARY --------------------------------------------------------
            this.m_ShowLibrary = EditorGUILayout.Foldout(this.m_ShowLibrary, "Audio ID Library", true);
            SessionState.SetBool(KEY_SHOW_LIB, this.m_ShowLibrary);
            if (this.m_ShowLibrary)
            {
                EditorGUI.indentLevel++;
                if (this.m_SfxIDList != null) this.m_SfxIDList.DoLayoutList();
                if (this.m_BgIDList  != null) this.m_BgIDList.DoLayoutList();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(6);

            // --- SFX ------------------------------------------------------------
            this.m_ShowSfx = EditorGUILayout.Foldout(this.m_ShowSfx, "SFX", true);
            SessionState.SetBool(KEY_SHOW_SFX, this.m_ShowSfx);
            if (this.m_ShowSfx)
            {
                EditorGUI.indentLevel++;
                // nhờ PropertyDrawer của SfxClip render gọn đẹp
                EditorGUILayout.PropertyField(this.m_Sfx, GUIContent.none, true);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(6);

            // --- BACKGROUND -----------------------------------------------------
            this.m_ShowBackground = EditorGUILayout.Foldout(this.m_ShowBackground, "Background", true);
            SessionState.SetBool(KEY_SHOW_BG, this.m_ShowBackground);
            if (this.m_ShowBackground)
            {
                EditorGUI.indentLevel++;
                // nhờ PropertyDrawer của BackgroundClip render gọn đẹp
                EditorGUILayout.PropertyField(this.m_Background, GUIContent.none, true);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(12);

            if (GUILayout.Button("Generate Enums"))
            {
                this.GenerateEnums((AudioClipData)this.target);
            }

            this.serializedObject.ApplyModifiedProperties();
        }

        // PRIVATE METHODS: --------------------------------------------------------------------------------------------

        private ReorderableList CreateIDList(SerializedProperty listProp, string header)
        {
            var list = new ReorderableList(this.serializedObject, listProp, true, true, true, true);

            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, header);

            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                rect.y += 2;
                var element = listProp.GetArrayElementAtIndex(index);
                var idProp  = element.FindPropertyRelative("m_ID"); // string
                EditorGUI.PropertyField(rect, idProp, GUIContent.none);
            };

            list.elementHeight = EditorGUIUtility.singleLineHeight + 6;
            return list;
        }

        private void GenerateEnums(AudioClipData data)
        {
            if (data.Library == null)
            {
                Debug.LogError("AudioClipData.Library is null! Please assign it.");
                return;
            }

            if (!Directory.Exists(Paths.GENERATED_FOLDER_PATH))
                Directory.CreateDirectory(Paths.GENERATED_FOLDER_PATH);

            // SFX
            var sfxPath = Path.Combine(Paths.GENERATED_FOLDER_PATH, Paths.SFX_FILE_NAME);
            this.WriteEnumFile(sfxPath, Paths.NAMESPACE_NAME, Paths.SFX_ENUM_NAME, data.Library.SfxID);

            // BG
            var bgPath = Path.Combine(Paths.GENERATED_FOLDER_PATH, Paths.BG_FILE_NAME);
            this.WriteEnumFile(bgPath, Paths.NAMESPACE_NAME, Paths.BG_ENUM_NAME, data.Library.BackgroundID);

            AssetDatabase.Refresh();
            Debug.Log("Generated Audio ID enums successfully!");
        }

        private void WriteEnumFile(string path, string namespaceName, string enumName, System.Collections.Generic.List<IDLibrary> ids)
        {
            using var writer = new StreamWriter(path, false, Encoding.UTF8);
            writer.WriteLine("// <auto-generated>");
            writer.WriteLine("// This file was generated by AudioClipDataEditor.");
            writer.WriteLine("// </auto-generated>");
            writer.WriteLine();
            writer.WriteLine("namespace " + namespaceName);
            writer.WriteLine("{");
            writer.WriteLine($"    public enum {enumName}");
            writer.WriteLine("    {");

            for (int i = 0; i < ids.Count; i++)
            {
                string raw = ids[i]?.ID ?? string.Empty;
                string id  = string.IsNullOrWhiteSpace(raw) ? $"Undefined_{i}" : MakeValidEnumName(raw);
                writer.WriteLine($"        {id} = {i},");
            }

            writer.WriteLine("    }");
            writer.WriteLine("}");
        }
        
        private string MakeValidEnumName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "_";

            // Replace spaces/hyphens with underscore
            string result = name.Replace(' ', '_').Replace('-', '_');

            // Remove invalid chars
            result = Regex.Replace(result, @"[^a-zA-Z0-9_]", "");

            // If starts with digit, prefix underscore
            if (char.IsDigit(result[0])) result = "_" + result;

            // Avoid empty
            if (string.IsNullOrEmpty(result)) result = "_";

            return result;
        }
    }
}
