using System.Collections.Generic;
using Ifreet.Core.Runtime.Audio.Data;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Ifreet.Core.Audio.Editor
{
    [CustomPropertyDrawer(typeof(BackgroundClipData))]
    public class BackgroundClipDataDrawer : PropertyDrawer
    {
        // PRIVATE MEMBERS: --------------------------------------------------------------------------------------------
        
        private const float ID_WIDTH = 120f;
        private const float SPACING = 4f;
        private const float CLIP_WIDTH = 60f;

        private Vector2 m_ScrollPos;
        private readonly Dictionary<string, ReorderableList> m_ReorderableLists = new();

        // GUI DRAW METHOD: --------------------------------------------------------------------------------------------
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var idProp = property.FindPropertyRelative("m_ID");
            var clipsProp = property.FindPropertyRelative("m_Clips");
            var viewModeProp = property.FindPropertyRelative("m_ViewMode"); // nếu không tồn tại, ta fallback

            EditorGUI.BeginProperty(position, label, property);

            // --- ID ---
            var idRect = new Rect(position.x, position.y, ID_WIDTH, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(idRect, idProp, GUIContent.none);

            // --- View mode dropdown (nếu có field) ---
            var modeRect = new Rect(idRect.xMax + SPACING, position.y, 90f, EditorGUIUtility.singleLineHeight);
            if (viewModeProp != null)
                EditorGUI.PropertyField(modeRect, viewModeProp, GUIContent.none);

            // vùng content phía dưới ID + spacing
            var startY = position.y + EditorGUIUtility.singleLineHeight + SPACING;
            var contentRect = new Rect(position.x, startY,
                position.width, position.height - EditorGUIUtility.singleLineHeight - SPACING);

            // chọn mode (fallback Grid nếu không có property)
            var mode = ClipViewMode.Grid;
            if (viewModeProp != null) mode = (ClipViewMode)viewModeProp.enumValueIndex;

            switch (mode)
            {
                case ClipViewMode.Grid:
                    this.DrawGrid(contentRect, clipsProp);
                    break;
                case ClipViewMode.Scroll:
                    this.DrawScroll(contentRect, clipsProp);
                    break;
                case ClipViewMode.Reorderable:
                    this.DrawReorderable(contentRect, clipsProp);
                    break;
            }

            EditorGUI.EndProperty();
        }

        // PRIVATE METHODS: --------------------------------------------------------------------------------------------
        
        private void DrawGrid(Rect position, SerializedProperty clipsProp)
        {
            var x = position.x;
            var y = position.y;
            var maxX = position.x + position.width;

            for (var i = 0; i < clipsProp.arraySize; i++)
            {
                var clipProp = clipsProp.GetArrayElementAtIndex(i);
                var clipRect = new Rect(x, y, CLIP_WIDTH, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(clipRect, clipProp, GUIContent.none);

                x += CLIP_WIDTH + SPACING;
                if (x + CLIP_WIDTH > maxX)
                {
                    x = position.x;
                    y += EditorGUIUtility.singleLineHeight + SPACING;
                }
            }

            var addRect = new Rect(x, y, CLIP_WIDTH, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(addRect, "+"))
            {
                clipsProp.arraySize++;
            }
        }

        // --- SCROLL ---
        private void DrawScroll(Rect position, SerializedProperty clipsProp)
        {
            var viewWidth = clipsProp.arraySize * (CLIP_WIDTH + SPACING) + CLIP_WIDTH;
            var content = new Rect(0, 0, viewWidth, EditorGUIUtility.singleLineHeight);

            this.m_ScrollPos = GUI.BeginScrollView(position, this.m_ScrollPos, content);

            var x = 0f;
            for (var i = 0; i < clipsProp.arraySize; i++)
            {
                var clipProp = clipsProp.GetArrayElementAtIndex(i);
                var clipRect = new Rect(x, 0, CLIP_WIDTH, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(clipRect, clipProp, GUIContent.none);
                x += CLIP_WIDTH + SPACING;
            }

            var addRect = new Rect(x, 0, CLIP_WIDTH, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(addRect, "+")) clipsProp.arraySize++;

            GUI.EndScrollView();
        }

        // --- REORDERABLE ---
        private void DrawReorderable(Rect position, SerializedProperty clipsProp)
        {
            var list = this.GetReorderableList(clipsProp);
            list.DoList(position);
        }

        private ReorderableList GetReorderableList(SerializedProperty clipsProp)
        {
            var key = clipsProp.propertyPath;
            if (!this.m_ReorderableLists.TryGetValue(key, out var list))
            {
                list = new ReorderableList(
                    clipsProp.serializedObject, clipsProp, true, true, true, true);

                list.drawElementCallback = (rect, index, active, focused) =>
                {
                    var element = clipsProp.GetArrayElementAtIndex(index);
                    rect.height = EditorGUIUtility.singleLineHeight;
                    rect.y += 2;
                    EditorGUI.PropertyField(rect, element, GUIContent.none);
                };

                list.drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Clips");
                };

                list.elementHeight = EditorGUIUtility.singleLineHeight + 4;

                this.m_ReorderableLists[key] = list;
            }

            return list;
        }

        // --- HEIGHT ---
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var clipsProp = property.FindPropertyRelative("m_Clips");
            var viewModeProp = property.FindPropertyRelative("m_ViewMode");

            var mode = ClipViewMode.Grid;
            if (viewModeProp != null) mode = (ClipViewMode)viewModeProp.enumValueIndex;

            switch (mode)
            {
                case ClipViewMode.Grid:
                    var itemsPerRow =
                        Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - ID_WIDTH - SPACING - 90f)
                                         / (CLIP_WIDTH + SPACING));
                    itemsPerRow = Mathf.Max(1, itemsPerRow);

                    var rowCount = Mathf.CeilToInt((float)clipsProp.arraySize / itemsPerRow);
                    if (clipsProp.arraySize == 0) rowCount = 1;

                    return EditorGUIUtility.singleLineHeight               // ID + mode
                         + SPACING
                         + rowCount * (EditorGUIUtility.singleLineHeight + SPACING);

                case ClipViewMode.Scroll:
                    return EditorGUIUtility.singleLineHeight               // ID + mode
                         + SPACING
                         + (EditorGUIUtility.singleLineHeight * 2f);       // show 2 lines of scroll area

                case ClipViewMode.Reorderable:
                    var list = this.GetReorderableList(clipsProp);
                    return EditorGUIUtility.singleLineHeight + SPACING     // ID + mode
                         + list.GetHeight();

                default:
                    return EditorGUIUtility.singleLineHeight;
            }
        }
    }
}
