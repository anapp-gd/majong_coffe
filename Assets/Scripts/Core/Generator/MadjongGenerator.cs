using System.Collections.Generic;
using UnityEngine;

public static class MadjongGenerator
{
    // Структура данных для возврата
    public struct TileData
    {
        public Vector2Int position;
        public int type;

        public TileData(Vector2Int pos, int type)
        {
            this.position = pos;
            this.type = type;
        }
    }

    // Простая маска поля
    static int[,] mask = new int[,] {
        {0, 1, 1, 1, 0},
        {1, 1, 0, 1, 1},
        {1, 0, 0, 0, 1},
        {1, 1, 0, 1, 1},
        {0, 1, 1, 1, 0}
    };

    public static List<TileData> Generate(int pairsCount)
    {
        if (!TryGetAvailablePoints(out List<Vector2Int> availablePositions))
        {
            Debug.LogError("Нет доступных позиций для генерации!");
            return new List<TileData>();
        }

        // Если мест меньше, чем пар, уменьшаем количество пар
        int maxPairs = availablePositions.Count / 2;
        if (pairsCount > maxPairs)
        {
            Debug.LogWarning($"Запрошено {pairsCount} пар, но доступно только {maxPairs}. Уменьшаем до {maxPairs}.");
            pairsCount = maxPairs;
        }

        List<TileData> result = new List<TileData>();

        for (int i = 0; i < pairsCount; i++)
        {
            int type = Random.Range(0, 5);

            int index1 = Random.Range(0, availablePositions.Count);
            Vector2Int pos1 = availablePositions[index1];
            availablePositions.RemoveAt(index1);

            int index2 = Random.Range(0, availablePositions.Count);
            Vector2Int pos2 = availablePositions[index2];
            availablePositions.RemoveAt(index2);

            result.Add(new TileData(pos1, type));
            result.Add(new TileData(pos2, type));
        }

        return result;
    }

    private static bool TryGetAvailablePoints(out List<Vector2Int> availablePositions)
    {
        availablePositions = new List<Vector2Int>();

        for (int y = 0; y < mask.GetLength(0); y++)
        {
            for (int x = 0; x < mask.GetLength(1); x++)
            {
                if (mask[y, x] == 1)
                    availablePositions.Add(new Vector2Int(x, y));
            }
        }

        return availablePositions.Count > 0;
    }
}
