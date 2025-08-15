using System.Collections.Generic;
using UnityEngine;

public static class MadjongGenerator
{
    public struct TileData
    {
        public Vector2Int position;
        public Enums.TileType TileType;
        public int layer;

        public TileData(Vector2Int pos, int layer, Enums.TileType tyleType)
        {
            this.position = pos; 
            this.layer = layer;
            this.TileType = tyleType;
        }
    }

    public static List<TileData> Generate(int layersCount, int pairsCount, int tileTypesCount)
    {
        List<TileData> result = new List<TileData>();
        bool[,,] grid = new bool[layersCount, 5, 5];

        for (int layer = 0; layer < layersCount; layer++)
        {
            List<Vector2Int> available = GenerateLayerMask(layer, grid);

            while (available.Count >= 2 && pairsCount > 0)
            {
                int type = Random.Range(0, tileTypesCount);

                // Выбираем первую плитку
                int idx1 = Random.Range(0, available.Count);
                Vector2Int pos1 = available[idx1];
                available.RemoveAt(idx1);

                // Ищем вторую плитку, чтобы была не на той же позиции
                int idx2 = Random.Range(0, available.Count);
                Vector2Int pos2 = available[idx2];
                available.RemoveAt(idx2);

                grid[layer, pos1.y, pos1.x] = true;
                grid[layer, pos2.y, pos2.x] = true;

                result.Add(new TileData(new Vector2Int(pos1.x, pos1.y), layer, (Enums.TileType)type));
                result.Add(new TileData(new Vector2Int(pos2.x, pos2.y), layer, (Enums.TileType)type));

                pairsCount--;
            }
        }

        return result;
    }

    private static List<Vector2Int> GenerateLayerMask(int z, bool[,,] grid)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                // Проверка опоры снизу
                if (z > 0 && !grid[z - 1, y, x])
                    continue;

                // Проверка свободной стороны (слева или справа)
                bool freeLeft = (x - 1 < 0) || !grid[z, y, x - 1];
                bool freeRight = (x + 1 >= 5) || !grid[z, y, x + 1];
                if (!freeLeft && !freeRight)
                    continue;

                // Проверка, что сверху нет плитки
                if (z + 1 < grid.GetLength(0) && grid[z + 1, y, x])
                    continue;

                // Вероятность, что здесь будет слот
                if (Random.value > 0.3f)
                    positions.Add(new Vector2Int(x, y));
            }
        }

        return positions;
    }
}
