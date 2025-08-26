using System.Collections.Generic;
using UnityEngine;

public static class MadjongGenerator
{
    public struct GridCell
    {
        public Vector2Int GridPos; // координаты в сетке
        public Vector2 WorldPos;   // мировые координаты

        public GridCell(Vector2Int gridPos, Vector2 worldPos)
        {
            GridPos = gridPos;
            WorldPos = worldPos;
        }

        // чтобы HashSet работал корректно
        public override bool Equals(object obj)
        {
            if (!(obj is GridCell)) return false;
            GridCell other = (GridCell)obj;
            return WorldPos == other.WorldPos; // сравниваем по мировой позиции
        }

        public override int GetHashCode()
        {
            return WorldPos.GetHashCode();
        }
    }

    public struct TileData
    {
        public Vector2 WorldPos;
        public Vector2Int GridPos;
        public Enums.TileType TileType;
        public int Layer;

        public TileData(Vector2 worldPos, Vector2Int gridPos, int layer, Enums.TileType tileType)
        {
            WorldPos = worldPos;
            GridPos = gridPos;
            Layer = layer;
            TileType = tileType;
        }
    }

    // === Генерация ===
    public static (List<TileData> tiles, HashSet<Enums.DishType> dishes, int layersCount) Generate(LevelData data)
    {
        var result = new List<TileData>();
        var availableDishes = new HashSet<Enums.DishType>();

        // словарь: ключ = layer, value = все занятые ячейки
        var grid = new Dictionary<int, HashSet<GridCell>>();

        int pairsLeft = data.pairsCount;
        int actualLayers = 0;

        // === 0-й слой: baseLayer ===
        var baseCells = new List<GridCell>();
        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                int index = y * data.width + x;
                if (data.baseLayer[index] == 1)
                {
                    var gridPos = new Vector2Int(x, y);
                    var worldPos = (Vector2)gridPos;
                    baseCells.Add(new GridCell(gridPos, worldPos));
                }
            }
        }

        PlacePairsOnLayer(baseCells, 0, data, grid, result, availableDishes, ref pairsLeft);
        actualLayers++;

        // === остальные слои ===
        for (int layer = 1; layer < data.layersCount; layer++)
        {
            var positions = GeneratePyramidLayerMask(layer, grid);
            if (positions.Count == 0) break;

            PlacePairsOnLayer(positions, layer, data, grid, result, availableDishes, ref pairsLeft);
            actualLayers++;
        }

        return (result, availableDishes, actualLayers);
    }

    // === Установка пар ===
    private static void PlacePairsOnLayer(
        List<GridCell> positions,
        int layer,
        LevelData data,
        Dictionary<int, HashSet<GridCell>> grid,
        List<TileData> result,
        HashSet<Enums.DishType> availableDishes,
        ref int pairsLeft)
    {
        if (!grid.ContainsKey(layer))
            grid[layer] = new HashSet<GridCell>();

        var availableTileTypesCopy = new List<Enums.TileType>(data.availableTileTypes);

        while (positions.Count >= 2 && pairsLeft > 0)
        {
            // выбираем тип плитки
            Enums.TileType tileType;
            if (availableTileTypesCopy.Count >= pairsLeft)
            {
                int typeIdx = Random.Range(0, availableTileTypesCopy.Count);
                tileType = availableTileTypesCopy[typeIdx];
                availableTileTypesCopy.RemoveAt(typeIdx);
            }
            else
            {
                tileType = data.availableTileTypes[Random.Range(0, data.availableTileTypes.Count)];
            }

            if (DishMapping.TryGetDish(tileType, out var dishType))
                availableDishes.Add(dishType);

            // выбираем 2 случайные ячейки
            int idx1 = Random.Range(0, positions.Count);
            var cell1 = positions[idx1];
            positions.RemoveAt(idx1);

            int idx2 = Random.Range(0, positions.Count);
            var cell2 = positions[idx2];
            positions.RemoveAt(idx2);

            // отмечаем в grid
            grid[layer].Add(cell1);
            grid[layer].Add(cell2);

            // добавляем в результат
            result.Add(new TileData(cell1.WorldPos, cell1.GridPos, layer, tileType));
            result.Add(new TileData(cell2.WorldPos, cell2.GridPos, layer, tileType));

            pairsLeft--;
        }
    }

    // === Генерация маски пирамиды ===
    private static List<GridCell> GeneratePyramidLayerMask(int layer, Dictionary<int, HashSet<GridCell>> grid)
    {
        var positions = new List<GridCell>();
        if (!grid.ContainsKey(layer - 1)) return positions;

        var prevLayer = grid[layer - 1];

        foreach (var cell in prevLayer)
        {
            Vector2 p = cell.WorldPos;

            // проверяем 4 опоры (квадрат 2x2)
            if (prevLayer.Contains(new GridCell(Vector2Int.zero, p)) &&
                prevLayer.Contains(new GridCell(Vector2Int.zero, p + Vector2.right)) &&
                prevLayer.Contains(new GridCell(Vector2Int.zero, p + Vector2.up)) &&
                prevLayer.Contains(new GridCell(Vector2Int.zero, p + Vector2.one)))
            {
                // центр квадрата (смещённая клетка)
                var newWorldPos = p + new Vector2(0.5f, 0.5f);
                var newGridPos = new Vector2Int(Mathf.RoundToInt(newWorldPos.x), Mathf.RoundToInt(newWorldPos.y));
                positions.Add(new GridCell(newGridPos, newWorldPos));
            }
        }

        return positions;
    }
}


/*using System.Collections.Generic;
using UnityEngine;

public static class MadjongGenerator
{
    public struct TileData
    {
        public Vector2 WorldPos;
        public Vector2Int GridPos;
        public Enums.TileType TileType;
        public int Layer;

        public TileData(Vector2 worldPos, Vector2Int gridPos, int layer, Enums.TileType tileType)
        {
            WorldPos = worldPos;
            GridPos = gridPos;
            Layer = layer;
            TileType = tileType;
        }
    }
    public static (List<TileData> tiles, HashSet<Enums.DishType> dishes, int layersCount) Generate(LevelData data)
    {
        var result = new List<TileData>();
        var availableDishes = new HashSet<Enums.DishType>();

        bool[,,] grid = new bool[data.layersCount, data.height, data.width];
        int pairsLeft = data.pairsCount;

        int actualLayers = 0;

        // === 0-й слой: baseLayer ===
        var basePositions = new List<Vector2Int>();
        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                int index = y * data.width + x;
                if (data.baseLayer[index] == 1)
                    basePositions.Add(new Vector2Int(x, y));
            }
        }

        PlacePairsOnLayer(basePositions, 0, data, grid, result, availableDishes, ref pairsLeft);
        actualLayers++; // base layer сгенерирован

        // === остальные слои ===
        for (int layer = 1; layer < data.layersCount; layer++)
        { 
            var positions = GeneratePyramidLayerMask(layer, grid);
            if (positions.Count == 0) break;
            PlacePairsOnLayer(positions, layer, data, grid, result, availableDishes, ref pairsLeft); 
            actualLayers++;
        }

        return (result, availableDishes, actualLayers);
    }

    private static void PlacePairsOnLayer<T>(
        List<T> positions,
        int layer,
        LevelData data,
        bool[,,] grid,
        List<TileData> result,
        HashSet<Enums.DishType> availableDishes,
        ref int pairsLeft) where T : struct
    {
        var availableTileTypesCopy = new List<Enums.TileType>(data.availableTileTypes);

        while (positions.Count >= 2 && pairsLeft > 0)
        {
            // Выбираем тип плитки
            Enums.TileType tileType;
            if (availableTileTypesCopy.Count >= pairsLeft)
            {
                int typeIdx = Random.Range(0, availableTileTypesCopy.Count);
                tileType = availableTileTypesCopy[typeIdx];
                availableTileTypesCopy.RemoveAt(typeIdx);
            }
            else
            {
                tileType = data.availableTileTypes[Random.Range(0, data.availableTileTypes.Count)];
            }

            if (DishMapping.TryGetDish(tileType, out var dishType))
                availableDishes.Add(dishType);

            // Выбираем две позиции для пары
            int idx1 = Random.Range(0, positions.Count);
            T pos1 = positions[idx1];
            positions.RemoveAt(idx1);

            int idx2 = Random.Range(0, positions.Count);
            T pos2 = positions[idx2];
            positions.RemoveAt(idx2);

            Vector2Int gridPos1, gridPos2;
            Vector2 worldPos1, worldPos2;

            if (typeof(T) == typeof(Vector2Int))
            {
                gridPos1 = (Vector2Int)(object)pos1;
                gridPos2 = (Vector2Int)(object)pos2;
                worldPos1 = gridPos1;
                worldPos2 = gridPos2;
            }
            else // Vector2
            {
                worldPos1 = (Vector2)(object)pos1;
                worldPos2 = (Vector2)(object)pos2;
                gridPos1 = new Vector2Int(Mathf.RoundToInt(worldPos1.x), Mathf.RoundToInt(worldPos1.y));
                gridPos2 = new Vector2Int(Mathf.RoundToInt(worldPos2.x), Mathf.RoundToInt(worldPos2.y));
            }

            // отмечаем в grid
            grid[layer, gridPos1.y, gridPos1.x] = true;
            grid[layer, gridPos2.y, gridPos2.x] = true;

            // добавляем в результат
            result.Add(new TileData(worldPos1, gridPos1, layer, tileType));
            result.Add(new TileData(worldPos2, gridPos2, layer, tileType));

            pairsLeft--;
        }
    } 
    private static List<Vector2> GeneratePyramidLayerMask(int layer, bool[,,] grid)
    {
        var positions = new List<Vector2>();
        int height = grid.GetLength(1);
        int width = grid.GetLength(2);

        // проверяем все квадраты 2x2 на нижнем слое
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                // если есть все 4 опоры на предыдущем слое
                if (grid[layer - 1, y, x] &&
                    grid[layer - 1, y, x + 1] &&
                    grid[layer - 1, y + 1, x] &&
                    grid[layer - 1, y + 1, x + 1])
                {
                    // позиция следующего слоя — центр квадрата
                    positions.Add(new Vector2(x + 0.5f, y + 0.5f));
                }
            }
        }

        return positions;
    }
}
*/