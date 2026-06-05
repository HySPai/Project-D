using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private RuleTile ruleTile;
    [SerializeField] private EnumMatrixData dataMap;
    void Start()
    {
        InitMap();
    }
    void InitMap()
    {
        int rows = dataMap.enumMatrix.GetLength(0); // số hàng
        int cols = dataMap.enumMatrix.GetLength(1); // số cột

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                // Đảo chiều Z
                int flippedY = rows - 1 - row;

                Vector3Int pos = new Vector3Int(col, flippedY, 0);

                if (dataMap.enumMatrix[row, col].HasFlag(CellType.Wall))
                {
                    tilemap.SetTile(pos, ruleTile);
                }
            }
        }
    }
    
}
