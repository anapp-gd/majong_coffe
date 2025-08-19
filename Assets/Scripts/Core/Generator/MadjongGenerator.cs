using System.Collections.Generic;
using UnityEngine;

public static class MadjongGenerator
{
    public struct TileData
    {
        public Vector2Int position;
        public Enums.TileType TileType;
        public int layer;

        public TileData(Vector2Int pos, int layer, Enums.TileType tileType)
        {
            this.position = pos;
            this.layer = layer;
            this.TileType = tileType;
        }
    }
    /// <summary>
    /// Генерация тайлов + список доступных блюд (DishType).
    /// </summary>
    public static (List<TileData> tiles, HashSet<Enums.DishType> dishes) Generate(LevelData data)
    {
        List<TileData> result = new List<TileData>();
        HashSet<Enums.DishType> availableDishes = new HashSet<Enums.DishType>();

        bool[,,] grid = new bool[data.layersCount, data.height, data.width];
        int pairsLeft = data.pairsCount;

        // === 0-й слой: из baseLayer ===
        List<Vector2Int> basePositions = new List<Vector2Int>();
        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                int index = y * data.width + x;
                if (data.baseLayer[index] == 1)
                    basePositions.Add(new Vector2Int(x, y));
            }
        }

        var availableTileTypesCopy = new List<Enums.TileType>(data.availableTileTypes);

        while (basePositions.Count >= 2 && pairsLeft > 0 && availableTileTypesCopy.Count > 0)
        {
            int typeIdx = Random.Range(0, availableTileTypesCopy.Count);
            var tileType = availableTileTypesCopy[typeIdx];
            availableTileTypesCopy.RemoveAt(typeIdx);

            if (DishMapping.TryGetDish(tileType, out var dishType))
            {
                availableDishes.Add(dishType);
            }

            int idx1 = Random.Range(0, basePositions.Count);
            Vector2Int pos1 = basePositions[idx1];
            basePositions.RemoveAt(idx1);

            int idx2 = Random.Range(0, basePositions.Count);
            Vector2Int pos2 = basePositions[idx2];
            basePositions.RemoveAt(idx2);

            grid[0, pos1.y, pos1.x] = true;
            grid[0, pos2.y, pos2.x] = true;

            result.Add(new TileData(pos1, 0, tileType));
            result.Add(new TileData(pos2, 0, tileType));

            pairsLeft--;
        }

        // === остальные слои ===
        for (int layer = 1; layer < data.layersCount; layer++)
        {
            List<Vector2Int> available = GenerateLayerMask(layer, grid);

            // создаём копию типов заново для каждого слоя
            var layerTileTypesCopy = new List<Enums.TileType>(data.availableTileTypes);

            while (available.Count >= 2 && pairsLeft > 0 && layerTileTypesCopy.Count > 0)
            {
                int typeIdx = Random.Range(0, layerTileTypesCopy.Count);
                var tileType = layerTileTypesCopy[typeIdx];
                layerTileTypesCopy.RemoveAt(typeIdx);

                if (DishMapping.TryGetDish(tileType, out var dishType))
                {
                    availableDishes.Add(dishType);
                }

                int idx1 = Random.Range(0, available.Count);
                Vector2Int pos1 = available[idx1];
                available.RemoveAt(idx1);

                int idx2 = Random.Range(0, available.Count);
                Vector2Int pos2 = available[idx2];
                available.RemoveAt(idx2);

                grid[layer, pos1.y, pos1.x] = true;
                grid[layer, pos2.y, pos2.x] = true;

                result.Add(new TileData(pos1, layer, tileType));
                result.Add(new TileData(pos2, layer, tileType));

                pairsLeft--;
            }
        }

        return (result, availableDishes);
    } 
    private static List<Vector2Int> GenerateLayerMask(int z, bool[,,] grid)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        int height = grid.GetLength(1);
        int width = grid.GetLength(2);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (z > 0 && !grid[z - 1, y, x]) // опора снизу
                    continue;

                bool freeLeft = (x - 1 < 0) || !grid[z, y, x - 1];
                bool freeRight = (x + 1 >= width) || !grid[z, y, x + 1];
                if (!freeLeft && !freeRight)
                    continue;

                if (z + 1 < grid.GetLength(0) && grid[z + 1, y, x]) // сверху занято
                    continue;

                if (Random.value > 0.3f)
                    positions.Add(new Vector2Int(x, y));
            }
        }

        return positions;
    }
}
