using UnityEngine;

public static class WaterDistanceField
{
    private const float Far = 1e6f;

    /// <summary>
    /// Euclidean distance from each cell to the nearest solid cell or grid border, in metres. Returns a width * height array in row order.
    /// </summary>
    public static float[] Compute(bool[] solid, int width, int height, float cellSize)
    {
        int count = width * height;
        Vector2[] offsets = new Vector2[count];

        for (int i = 0; i < count; i++)
            offsets[i] = solid[i] ? Vector2.zero : new Vector2(Far, Far);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Relax(offsets, width, height, x, y, -1, 0);
                Relax(offsets, width, height, x, y, 0, -1);
                Relax(offsets, width, height, x, y, -1, -1);
                Relax(offsets, width, height, x, y, 1, -1);
            }

            for (int x = width - 1; x >= 0; x--)
                Relax(offsets, width, height, x, y, 1, 0);
        }

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = width - 1; x >= 0; x--)
            {
                Relax(offsets, width, height, x, y, 1, 0);
                Relax(offsets, width, height, x, y, 0, 1);
                Relax(offsets, width, height, x, y, 1, 1);
                Relax(offsets, width, height, x, y, -1, 1);
            }

            for (int x = 0; x < width; x++)
                Relax(offsets, width, height, x, y, -1, 0);
        }

        float[] result = new float[count];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = x + y * width;
                float border = Mathf.Min(Mathf.Min(x, width - 1 - x), Mathf.Min(y, height - 1 - y));
                float nearest = solid[index] ? 0f : Mathf.Min(offsets[index].magnitude, border);
                result[index] = nearest * cellSize;
            }
        }

        return result;
    }

    private static void Relax(Vector2[] offsets, int width, int height, int x, int y, int dx, int dy)
    {
        int nx = x + dx;
        int ny = y + dy;
        if (nx < 0 || ny < 0 || nx >= width || ny >= height) return;

        Vector2 candidate = offsets[nx + ny * width] + new Vector2(dx, dy);
        if (candidate.sqrMagnitude < offsets[x + y * width].sqrMagnitude)
            offsets[x + y * width] = candidate;
    }
}
