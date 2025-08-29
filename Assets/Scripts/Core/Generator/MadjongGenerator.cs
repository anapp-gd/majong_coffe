using System;
using System.Collections.Generic;
using UnityEngine;

public static class MadjongGenerator
{
    // единица сетки = 0.5 world units -> чтобы избежать проблем с float-равенством
    public struct GridPos3
    {
        public int X2; // world.x * 2 (rounded)
        public int Y2; // world.y * 2 (rounded)
        public int Layer;

        public GridPos3(Vector2 worldXY, int layer)
        {
            X2 = Mathf.RoundToInt(worldXY.x * 2f);
            Y2 = Mathf.RoundToInt(worldXY.y * 2f);
            Layer = layer;
        }

        public GridPos3(int x2, int y2, int layer)
        {
            X2 = x2;
            Y2 = y2;
            Layer = layer;
        }

        public Vector2 ToWorld()
        {
            return new Vector2(X2 / 2f, Y2 / 2f);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GridPos3)) return false;
            var o = (GridPos3)obj;
            return X2 == o.X2 && Y2 == o.Y2 && Layer == o.Layer;
        }

        public override int GetHashCode()
        {
            // простая, но рабочая хеш-функция для трёх int
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + X2;
                hash = hash * 31 + Y2;
                hash = hash * 31 + Layer;
                return hash;
            }
        }
    }

    public struct TileData
    {
        public Vector2 WorldPos;
        public Vector2Int GridPos; // округлённая позиция для удобства
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

    // все плитки
    private static Dictionary<GridPos3, TileData> gridAll;

    // событие, чтобы UI мог подписаться и обновиться
    public static event Action OnTilesUpdated;

    // --- Генерация ---
    public static (List<TileData> tiles, HashSet<Enums.DishType> dishes, int layersCount) Generate(LevelData data)
    {
        gridAll = new Dictionary<GridPos3, TileData>();
        var result = new List<TileData>();
        var availableDishes = new HashSet<Enums.DishType>();

        int pairsLeft = data.pairsCount;
        int actualLayers = 0;

        // 0-й слой
        var baseCells = new List<Vector2>();
        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                int index = y * data.width + x;
                if (data.baseLayer[index] == 1)
                {
                    baseCells.Add(new Vector2(x, y));
                }
            }
        }

        PlacePairsOnLayer(baseCells, 0, data, result, availableDishes, ref pairsLeft);
        actualLayers++;

        // остальные слои (пирамида)
        for (int layer = 1; layer < data.layersCount; layer++)
        {
            var positions = GeneratePyramidLayerMask(layer);
            if (positions.Count == 0) break;
            PlacePairsOnLayer(positions, layer, data, result, availableDishes, ref pairsLeft);
            actualLayers++;
        }

        OnTilesUpdated?.Invoke();
        return (result, availableDishes, actualLayers);
    }

    private static void PlacePairsOnLayer(
    List<Vector2> positions,
    int layer,
    LevelData data,
    List<TileData> result,
    HashSet<Enums.DishType> availableDishes,
    ref int pairsLeft)
    {
        var availableTileTypesCopy = new List<Enums.TileType>(data.availableTileTypes);

        // --- гарантируем первую пару ---
        if (pairsLeft > 0 && positions.Count >= 2)
        {
            Vector2 wp1, wp2;

            // сортируем позиции по X, чтобы легко брать крайние
            positions.Sort((a, b) => a.x.CompareTo(b.x));

            int mode = UnityEngine.Random.Range(0, 3); // 0=две слева, 1=две справа, 2=по краям
            if (mode == 0)
            {
                wp1 = positions[0];
                wp2 = positions[1];
            }
            else if (mode == 1)
            {
                wp1 = positions[positions.Count - 1];
                wp2 = positions[positions.Count - 2];
            }
            else
            {
                wp1 = positions[0];
                wp2 = positions[positions.Count - 1];
            }

            positions.Remove(wp1);
            positions.Remove(wp2);

            var tileType = data.availableTileTypes[UnityEngine.Random.Range(0, data.availableTileTypes.Count)];
            if (DishMapping.TryGetDish(tileType, out var dishType))
                availableDishes.Add(dishType);

            var td1 = new TileData(wp1, Vector2Int.RoundToInt(wp1), layer, tileType);
            var td2 = new TileData(wp2, Vector2Int.RoundToInt(wp2), layer, tileType);

            result.Add(td1);
            result.Add(td2);

            gridAll[new GridPos3(wp1, layer)] = td1;
            gridAll[new GridPos3(wp2, layer)] = td2;

            pairsLeft--;
        }

        // --- остальные пары рандомом ---
        while (positions.Count >= 2 && pairsLeft > 0)
        {
            Enums.TileType tileType;
            if (availableTileTypesCopy.Count >= pairsLeft)
            {
                int typeIdx = UnityEngine.Random.Range(0, availableTileTypesCopy.Count);
                tileType = availableTileTypesCopy[typeIdx];
                availableTileTypesCopy.RemoveAt(typeIdx);
            }
            else
            {
                tileType = data.availableTileTypes[UnityEngine.Random.Range(0, data.availableTileTypes.Count)];
            }

            if (DishMapping.TryGetDish(tileType, out var dishType))
                availableDishes.Add(dishType);

            int idx1 = UnityEngine.Random.Range(0, positions.Count);
            var wp1 = positions[idx1];
            positions.RemoveAt(idx1);

            int idx2 = UnityEngine.Random.Range(0, positions.Count);
            var wp2 = positions[idx2];
            positions.RemoveAt(idx2);

            var td1 = new TileData(wp1, Vector2Int.RoundToInt(wp1), layer, tileType);
            var td2 = new TileData(wp2, Vector2Int.RoundToInt(wp2), layer, tileType);

            result.Add(td1);
            result.Add(td2);

            gridAll[new GridPos3(wp1, layer)] = td1;
            gridAll[new GridPos3(wp2, layer)] = td2;

            pairsLeft--;
        }
    }
    // маска для размещения плиток слоя (пирамида: центр 2x2)
    private static List<Vector2> GeneratePyramidLayerMask(int layer)
    {
        var positions = new List<Vector2>();

        // ищем все возможные центры: для каждой плитки в layer-1 проверяем квадрат 2x2
        foreach (var kvp in gridAll)
        {
            if (kvp.Key.Layer != layer - 1) continue;

            var p = kvp.Value.WorldPos;

            if (HasTileAt(p, layer - 1) &&
                HasTileAt(p + Vector2.right, layer - 1) &&
                HasTileAt(p + Vector2.up, layer - 1) &&
                HasTileAt(p + Vector2.one, layer - 1))
            {
                var center = p + new Vector2(.5f,.5f); // центр квадрата
                positions.Add(center);
            }
        }

        return positions;
    }

    // === Проверки соседей ===
    // Проверяем все смещения { -0.5, 0, +0.5 } по X и Y относительно переданной позиции на слое layer+1
    public static bool HasTileAbove(Vector2 xy, int layer)
    {
        if (gridAll == null) return false;

        // dx,dy в шагах 0.5: -1 -> -0.5, 0 -> 0, +1 -> +0.5
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                var off = new Vector2(dx * 0.5f, dy * 0.5f);
                if (gridAll.ContainsKey(new GridPos3(xy + off, layer + 1)))
                    return true;
            }
        }

        return false;
    }

    public static bool HasTileAt(Vector2 xy, int layer)
    {
        if (gridAll == null) return false;
        return gridAll.ContainsKey(new GridPos3(xy, layer));
    }

    // проверяем, есть ли блокирующая плитка справа/слева (берём небольшую вертикальную область ±0.5)
    private static bool HasBlockingTileOnSide(Vector2 xy, int layer, int dir) // dir: -1 left, +1 right
    {
        // dx = dir * 1.0f (один полный шаг по X)
        float dx = dir * 1f;

        // проверяем dy = -0.5, 0, +0.5 (частичные смещения по Y)
        for (int dy = -1; dy <= 1; dy++)
        {
            var off = new Vector2(dx, dy * 0.5f);
            if (HasTileAt(xy + off, layer))
                return true;
        }

        return false;
    }

    public static bool IsTileFree(TileData tile)
    {
        if (gridAll == null) return false;

        // если есть плитка сверху — блокирована
        if (HasTileAbove(tile.WorldPos, tile.Layer))
            return false;

        // если хотя бы один бок свободен — плитка доступна
        bool leftBlocked = HasBlockingTileOnSide(tile.WorldPos, tile.Layer, -1);
        bool rightBlocked = HasBlockingTileOnSide(tile.WorldPos, tile.Layer, +1);

        return !(leftBlocked && rightBlocked);
    }

    // ===== Удаление / обновление =====
    public static void RemoveTile(TileData tile)
    {
        if (gridAll == null) return;

        var key = new GridPos3(tile.WorldPos, tile.Layer);
        gridAll.Remove(key);

        OnTilesUpdated?.Invoke();
    }

    public static void RemoveTileAt(Vector2 worldPos, int layer)
    {
        if (gridAll == null) return;
        var key = new GridPos3(worldPos, layer);
        gridAll.Remove(key);
        OnTilesUpdated?.Invoke();
    }

    public static List<TileData> GetFreeTiles()
    {
        var free = new List<TileData>();
        if (gridAll == null) return free;

        foreach (var kvp in gridAll)
        {
            if (IsTileFree(kvp.Value))
                free.Add(kvp.Value);
        }
        return free;
    }

    // вернуть плитку над данной (если несколько — вернёт первую найденную)
    public static TileData? GetTileAbove(Vector2 xy, int layer)
    {
        if (gridAll == null) return null;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                var off = new Vector2(dx * 0.5f, dy * 0.5f);
                var key = new GridPos3(xy + off, layer + 1);
                if (gridAll.TryGetValue(key, out var td))
                    return td;
            }
        }

        return null;
    }
}

/*using System.Collections.Generic;
using UnityEngine;

public static class MadjongGenerator
{
    public struct GridPos3
    {
        public Vector2 XY;   // float (важно!)
        public int Layer;

        public GridPos3(Vector2 xy, int layer)
        {
            XY = xy;
            Layer = layer;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GridPos3)) return false;
            var other = (GridPos3)obj;
            return XY == other.XY && Layer == other.Layer;
        }

        public override int GetHashCode()
        {
            return XY.GetHashCode() ^ Layer.GetHashCode();
        }
    }

    public struct TileData
    {
        public Vector2 WorldPos;
        public Vector2Int GridPos; // для удобства логики (округлённая позиция)
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

    // === Все плитки в одном словаре ===
    private static Dictionary<GridPos3, TileData> gridAll;

    // === Генерация ===
    public static (List<TileData> tiles, HashSet<Enums.DishType> dishes, int layersCount) Generate(LevelData data)
    {
        gridAll = new Dictionary<GridPos3, TileData>();
        var result = new List<TileData>();
        var availableDishes = new HashSet<Enums.DishType>();

        int pairsLeft = data.pairsCount;
        int actualLayers = 0;

        // === 0-й слой: baseLayer ===
        var baseCells = new List<Vector2>();
        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                int index = y * data.width + x;
                if (data.baseLayer[index] == 1)
                {
                    baseCells.Add(new Vector2(x, y));
                }
            }
        }

        PlacePairsOnLayer(baseCells, 0, data, result, availableDishes, ref pairsLeft);
        actualLayers++;

        // === остальные слои ===
        for (int layer = 1; layer < data.layersCount; layer++)
        {
            var positions = GeneratePyramidLayerMask(layer);
            if (positions.Count == 0) break;

            PlacePairsOnLayer(positions, layer, data, result, availableDishes, ref pairsLeft);
            actualLayers++;
        }

        return (result, availableDishes, actualLayers);
    }

    // === Установка пар ===
    private static void PlacePairsOnLayer(
        List<Vector2> positions,
        int layer,
        LevelData data,
        List<TileData> result,
        HashSet<Enums.DishType> availableDishes,
        ref int pairsLeft)
    {
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

            // берём 2 случайные позиции
            int idx1 = Random.Range(0, positions.Count);
            var wp1 = positions[idx1];
            positions.RemoveAt(idx1);

            int idx2 = Random.Range(0, positions.Count);
            var wp2 = positions[idx2];
            positions.RemoveAt(idx2);

            var td1 = new TileData(wp1, Vector2Int.RoundToInt(wp1), layer, tileType);
            var td2 = new TileData(wp2, Vector2Int.RoundToInt(wp2), layer, tileType);

            result.Add(td1);
            result.Add(td2);

            gridAll[new GridPos3(wp1, layer)] = td1;
            gridAll[new GridPos3(wp2, layer)] = td2;

            pairsLeft--;
        }
    }

    // === Генерация маски пирамиды ===
    private static List<Vector2> GeneratePyramidLayerMask(int layer)
    {
        var positions = new List<Vector2>();

        foreach (var kvp in gridAll)
        {
            if (kvp.Key.Layer != layer - 1) continue;

            var cell = kvp.Value;
            Vector2 p = cell.WorldPos;

            // проверяем квадрат 2x2 внизу
            if (gridAll.ContainsKey(new GridPos3(p, layer - 1)) &&
                gridAll.ContainsKey(new GridPos3(p + Vector2.right, layer - 1)) &&
                gridAll.ContainsKey(new GridPos3(p + Vector2.up, layer - 1)) &&
                gridAll.ContainsKey(new GridPos3(p + Vector2.one, layer - 1)))
            {
                // центр квадрата (смещение на 0.5f)
                var newWorldPos = p + new Vector2(0.5f, 0.5f);
                positions.Add(newWorldPos);
            }
        }

        return positions;
    }

    // === Проверка соседей ===
    public static bool HasTileAbove(Vector2 xy, int layer)
    {
        // смещения, которые могут перекрывать плитку сверху
        Vector2[] offsets =
        {
            Vector2.zero,
            new Vector2(0.5f, 0f),
            new Vector2(-0.5f, 0f),
            new Vector2(0f, 0.5f),
            new Vector2(0f, -0.5f),
        };

        foreach (var off in offsets)
        {
            if (gridAll.ContainsKey(new GridPos3(xy + off, layer + 1)))
                return true;
        }

        return false;
    }

    public static bool HasTileAt(Vector2 xy, int layer)
    {
        return gridAll.ContainsKey(new GridPos3(xy, layer));
    }

    public static bool IsTileFree(TileData tile)
    {
        // Заблокирована, если сверху есть плитка
        if (HasTileAbove(tile.GridPos, tile.Layer))
            return false;

        // Свободна, если хотя бы один бок пустой
        bool leftFree = !HasTileAt(tile.WorldPos + Vector2.left, tile.Layer);
        bool rightFree = !HasTileAt(tile.WorldPos + Vector2.right, tile.Layer);

        return leftFree || rightFree;
    }

    // === Удаление плитки ===
    public static void RemoveTile(TileData tile)
    {
        var key = new GridPos3(tile.WorldPos, tile.Layer);
        gridAll.Remove(key);
    }

    // === Получить все свободные плитки ===
    public static List<TileData> GetFreeTiles()
    {
        var freeTiles = new List<TileData>();
        foreach (var kvp in gridAll)
        {
            if (IsTileFree(kvp.Value))
                freeTiles.Add(kvp.Value);
        }
        return freeTiles;
    }

    public static TileData? GetTileAbove(Vector2 xy, int layer)
    {
        var key = new GridPos3(xy, layer + 1);
        if (gridAll.TryGetValue(key, out var td))
            return td;
        return null;
    }
}*/
/*using System.Collections.Generic;
using UnityEngine;

public static class MadjongGenerator
{
    public struct GridPos3
    {
        public Vector2 XY;   // float (важно!)
        public int Layer;

        public GridPos3(Vector2 xy, int layer)
        {
            XY = xy;
            Layer = layer;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GridPos3)) return false;
            var other = (GridPos3)obj;
            return XY == other.XY && Layer == other.Layer;
        }

        public override int GetHashCode()
        {
            return XY.GetHashCode() ^ Layer.GetHashCode();
        }
    }

    public struct TileData
    {
        public Vector2 WorldPos;
        public Vector2Int GridPos; // для удобства логики (округлённая позиция)
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

    // === Все плитки в одном словаре ===
    private static Dictionary<GridPos3, TileData> gridAll;

    // === Генерация ===
    public static (List<TileData> tiles, HashSet<Enums.DishType> dishes, int layersCount) Generate(LevelData data)
    {
        gridAll = new Dictionary<GridPos3, TileData>();
        var result = new List<TileData>();
        var availableDishes = new HashSet<Enums.DishType>();

        int pairsLeft = data.pairsCount;
        int actualLayers = 0;

        // === 0-й слой: baseLayer ===
        var baseCells = new List<Vector2>();
        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                int index = y * data.width + x;
                if (data.baseLayer[index] == 1)
                {
                    baseCells.Add(new Vector2(x, y));
                }
            }
        }

        PlacePairsOnLayer(baseCells, 0, data, result, availableDishes, ref pairsLeft);
        actualLayers++;

        // === остальные слои ===
        for (int layer = 1; layer < data.layersCount; layer++)
        {
            var positions = GeneratePyramidLayerMask(layer);
            if (positions.Count == 0) break;

            PlacePairsOnLayer(positions, layer, data, result, availableDishes, ref pairsLeft);
            actualLayers++;
        }

        return (result, availableDishes, actualLayers);
    }

    // === Установка пар ===
    private static void PlacePairsOnLayer(
        List<Vector2> positions,
        int layer,
        LevelData data,
        List<TileData> result,
        HashSet<Enums.DishType> availableDishes,
        ref int pairsLeft)
    {
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

            // берём 2 случайные позиции
            int idx1 = Random.Range(0, positions.Count);
            var wp1 = positions[idx1];
            positions.RemoveAt(idx1);

            int idx2 = Random.Range(0, positions.Count);
            var wp2 = positions[idx2];
            positions.RemoveAt(idx2);

            var td1 = new TileData(wp1, Vector2Int.RoundToInt(wp1), layer, tileType);
            var td2 = new TileData(wp2, Vector2Int.RoundToInt(wp2), layer, tileType);

            result.Add(td1);
            result.Add(td2);

            gridAll[new GridPos3(wp1, layer)] = td1;
            gridAll[new GridPos3(wp2, layer)] = td2;

            pairsLeft--;
        }
    }

    // === Генерация маски пирамиды ===
    private static List<Vector2> GeneratePyramidLayerMask(int layer)
    {
        var positions = new List<Vector2>();

        foreach (var kvp in gridAll)
        {
            if (kvp.Key.Layer != layer - 1) continue;

            var cell = kvp.Value;
            Vector2 p = cell.WorldPos;

            // проверяем квадрат 2x2 внизу
            if (gridAll.ContainsKey(new GridPos3(p, layer - 1)) &&
                gridAll.ContainsKey(new GridPos3(p + Vector2.right, layer - 1)) &&
                gridAll.ContainsKey(new GridPos3(p + Vector2.up, layer - 1)) &&
                gridAll.ContainsKey(new GridPos3(p + Vector2.one, layer - 1)))
            {
                // центр квадрата (смещение на 0.5f)
                var newWorldPos = p + new Vector2(0.5f, 0.5f);
                positions.Add(newWorldPos);
            }
        }

        return positions;
    }

    // === Проверка соседей ===
    public static bool HasTileAbove(Vector2 xy, int layer)
    {
        return gridAll.ContainsKey(new GridPos3(xy, layer + 1));
    }

    public static bool HasTileBelow(Vector2 xy, int layer)
    {
        if (layer == 0) return false;
        return gridAll.ContainsKey(new GridPos3(xy, layer - 1));
    }

    public static TileData? GetTileAbove(Vector2 xy, int layer)
    {
        var key = new GridPos3(xy, layer + 1);
        if (gridAll.TryGetValue(key, out var td))
            return td;
        return null;
    }
}*/
/*using System.Collections.Generic;
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
*/
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