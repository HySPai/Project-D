using Ifreet.Core.Runtime.Audio;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Ifreet.Core.Audio.Editor
{
    [CustomPropertyDrawer(typeof(SfxClip))]
    public class SfxClipDrawer : PropertyDrawer
    {
        private ReorderableList m_List;
        private bool m_Initialized;

        private void Init(SerializedProperty property, GUIContent label)
        {
            var dataProp = property.FindPropertyRelative("m_Data");
            this.m_List = new ReorderableList(property.serializedObject, dataProp, true, true, true, true);

            this.m_List.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, label);
            };

            this.m_List.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = dataProp.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none, true);
            };

            this.m_List.elementHeightCallback = index =>
            {
                var element = dataProp.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true) + 6;
            };

            this.m_Initialized = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!this.m_Initialized) this.Init(property, label);
            this.m_List.DoList(position);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return this.m_List?.GetHeight() ?? EditorGUIUtility.singleLineHeight;
        }
    }
}