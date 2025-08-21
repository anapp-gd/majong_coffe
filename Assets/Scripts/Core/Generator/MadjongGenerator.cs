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
            position = pos;
            this.layer = layer;
            TileType = tileType;
        }
    }

    /// <summary>
    /// Генерация тайлов + список доступных блюд (DishType).
    /// Условия:
    /// 1) Пустые слои не создаём (если нет позиций для хотя бы одной пары — выходим).
    /// 2) Каждый следующий слой формируется "в шахматном порядке": 
    ///    - нечётные слои (1,3,5,...) — над горизонтальными парами снизу (x и x+1);
    ///    - чётные слои (>0: 2,4,6,...) — над вертикальными парами снизу (y и y+1).
    ///    На позицию слоя допускаем тайл только если под ним есть две опоры снизу.
    /// </summary>
    public static (List<TileData> tiles, HashSet<Enums.DishType> dishes) Generate(LevelData data)
    {
        List<TileData> result = new List<TileData>();
        HashSet<Enums.DishType> availableDishes = new HashSet<Enums.DishType>();

        bool[,,] grid = new bool[data.layersCount, data.height, data.width];
        int pairsLeft = data.pairsCount;

        // === 0-й слой: берём только клетки, отмеченные в baseLayer ===
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

        PlacePairsOnPositions(basePositions, 0, data, grid, result, availableDishes, ref pairsLeft);

        // === верхние слои: строгая поддержка "двумя опорами" и запрет пустых слоёв ===
        for (int layer = 1; layer < data.layersCount && pairsLeft > 0; layer++)
        {
            var available = GenerateLayerMaskWithSupport(layer, grid);

            // если нельзя поставить хотя бы одну пару — дальше слои невозможны, выходим
            if (available.Count < 2)
                break;

            PlacePairsOnPositions(available, layer, data, grid, result, availableDishes, ref pairsLeft);
        }

        return (result, availableDishes);
    }

    /// <summary>
    /// Размещает пары тайлов на доступных позициях (positions) текущего слоя.
    /// </summary>
    private static void PlacePairsOnPositions(
        List<Vector2Int> positions,
        int layer,
        LevelData data,
        bool[,,] grid,
        List<TileData> result,
        HashSet<Enums.DishType> availableDishes,
        ref int pairsLeft)
    {
        // копия типов — если хочешь избегать повторов, можно использовать её;
        // если типов меньше, чем пар — разрешим повторы.
        var uniquePool = new List<Enums.TileType>(data.availableTileTypes);

        while (positions.Count >= 2 && pairsLeft > 0)
        {
            Enums.TileType tileType;
            if (uniquePool.Count >= 1 && uniquePool.Count >= pairsLeft)
            {
                int typeIdx = Random.Range(0, uniquePool.Count);
                tileType = uniquePool[typeIdx];
                uniquePool.RemoveAt(typeIdx);
            }
            else
            {
                tileType = data.availableTileTypes[Random.Range(0, data.availableTileTypes.Count)];
            }

            if (DishMapping.TryGetDish(tileType, out var dishType))
                availableDishes.Add(dishType);

            // берём 2 любые позиции под пару
            int idx1 = Random.Range(0, positions.Count);
            Vector2Int pos1 = positions[idx1];
            positions.RemoveAt(idx1);

            int idx2 = Random.Range(0, positions.Count);
            Vector2Int pos2 = positions[idx2];
            positions.RemoveAt(idx2);

            grid[layer, pos1.y, pos1.x] = true;
            grid[layer, pos2.y, pos2.x] = true;

            result.Add(new TileData(pos1, layer, tileType));
            result.Add(new TileData(pos2, layer, tileType));

            pairsLeft--;
        }
    }

    /// <summary>
    /// "Шахматная" маска для слоя:
    ///  - Нечётный слой (1,3,5,...) — допускаем клетку (x,y), если снизу стоят две по горизонтали: (x,y) и (x+1,y).
    ///  - Чётный слой (>0: 2,4,6,...) — допускаем клетку (x,y), если снизу стоят две по вертикали: (x,y) и (x,y+1).
    /// Никаких одинарных опор — только чёткие пары. Это предотвращает “просто поверх”.
    /// </summary>
    private static List<Vector2Int> GenerateLayerMaskChess(int z, bool[,,] grid)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        int height = grid.GetLength(1);
        int width = grid.GetLength(2);

        // для z==0 маска не нужна — мы используем baseLayer
        if (z <= 0) return positions;

        bool horizontal = (z % 2 == 1); // 1-й, 3-й, ... — горизонтальные мостики; 2-й, 4-й, ... — вертикальные

        if (horizontal)
        {
            // Требуем две опоры: (x,y) и (x+1,y) на слое z-1
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    if (grid[z - 1, y, x] && grid[z - 1, y, x + 1])
                    {
                        positions.Add(new Vector2Int(x, y));
                    }
                }
            }
        }
        else
        {
            // Требуем две опоры: (x,y) и (x,y+1) на слое z-1
            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (grid[z - 1, y, x] && grid[z - 1, y + 1, x])
                    {
                        positions.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        return positions;
    }

    /// <summary>
    /// Генерация маски слоя: плитки ставятся в шахматном порядке относительно нижнего слоя.
    /// Плитка считается допустимой, если у неё есть хотя бы две диагональные опоры снизу.
    /// </summary>
    private static List<Vector2Int> GenerateLayerMaskWithSupport(int z, bool[,,] grid)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        int height = grid.GetLength(1);
        int width = grid.GetLength(2);

        if (z == 0) return positions;

        for (int y = 0; y < height - 1; y++) // минус 1, чтобы проверка не выходила за границы
        {
            for (int x = 0; x < width - 1; x++)
            {
                // Проверяем "диагональный квадрат" 2x2 снизу
                int support = 0;
                if (grid[z - 1, y, x]) support++;
                if (grid[z - 1, y, x + 1]) support++;
                if (grid[z - 1, y + 1, x]) support++;
                if (grid[z - 1, y + 1, x + 1]) support++;

                // Если хотя бы две опоры, можно поставить плитку "в шахматку"
                if (support >= 2)
                {
                    // Сместим координаты: ставим плитку в "центр квадрата" => визуально шахматный порядок
                    positions.Add(new Vector2Int(x, y));
                }
            }
        }

        return positions;
    }

}
