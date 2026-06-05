using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

[System.Flags]
public enum CellType
{
    None = 0,
    Wall = 1 << 0, 
    Item = 1 << 1  
}

[CreateAssetMenu(fileName = "SO_EnumMatrixData", menuName = "ScriptableObjects/Enum Matrix Data")]
public class EnumMatrixData : ScriptableObject
{
    [SerializeField] private int rows = 5;
    [SerializeField] private int cols = 5;

    [SerializeField]
    private CellType[] data; // Unity serialize được

    [TableMatrix(HorizontalTitle = "Enum Matrix", DrawElementMethod = "DrawEnumCell", ResizableColumns = false, RowHeight = 20)]
    public CellType[,] enumMatrix
    {
        get
        {
            var result = new CellType[rows, cols];
            if (data == null || data.Length != rows * cols)
                data = new CellType[rows * cols];

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    result[r, c] = data[r * cols + c];

            return result;
        }
        set
        {
            rows = value.GetLength(0);
            cols = value.GetLength(1);
            data = new CellType[rows * cols];

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    data[r * cols + c] = value[r, c];
        }
    }

    [ShowInInspector, DoNotDrawAsReference]
    [TableMatrix(HorizontalTitle = "Transposed Enum Matrix", DrawElementMethod = "DrawEnumCell", ResizableColumns = false, RowHeight = 20, Transpose = true)]
    public CellType[,] Transposed
    {
        get { return enumMatrix; }
        set { enumMatrix = value; }
    }

    // Vẽ từng ô trong grid
    private static CellType DrawEnumCell(Rect rect, CellType value)
    {
#if UNITY_EDITOR
        // Vẽ dropdown enum thay cho click chuột
        value = (CellType)UnityEditor.EditorGUI.EnumFlagsField(rect, value);

        // Hiển thị màu: nếu có nhiều flag thì mix màu
        Color color = Color.white;
        if (value.HasFlag(CellType.None)) color = Color.black;
        if (value.HasFlag(CellType.Wall)) color = Color.gray;
        if (value.HasFlag(CellType.Item)) color = Color.green;

        UnityEditor.EditorGUI.DrawRect(rect.Padding(1), color);

        GUI.Label(rect, value.ToString(), new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            normal = new GUIStyleState() { textColor = Color.white }
        });
#endif
        return value;
    }

    public CellType GetCell(int r, int c) => data[r * cols + c];
}
