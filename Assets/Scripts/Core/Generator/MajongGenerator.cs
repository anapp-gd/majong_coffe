using System;
using System.Collections.Generic;
using UnityEngine;

public static class MajongGenerator
{
    // Описание плитки, возвращаемое генератором
    public class Tile
    {
        public Vector2 WorldPos;       // точная мировая позиция (может иметь .5)
        public Vector2Int ScaledPos;   // целочисленная позиция = RoundToInt(WorldPos * 2)
        public Vector2Int GridPosInt;  // округлённая целая позиция (для удобства UI/индексов)
        public int Layer;
        public bool IsAvailable;       // доступна ли плитка для выбора/мержа (вычисляется)
        public int Id;                 // уникальный id (опционально, для отладки)

        public Tile(Vector2 worldPos, int layer, int id)
        {
            WorldPos = worldPos;
            Layer = layer;
            ScaledPos = ToScaled(worldPos);
            GridPosInt = Vector2Int.RoundToInt(worldPos);
            IsAvailable = false;
            Id = id;
        }

        public override string ToString()
        {
            return $"Tile#{Id} L{Layer} WP{WorldPos} S{ScaledPos} Avail:{IsAvailable}";
        }
    }

    // Внутренний словарь: ключ = (ScaledPos, Layer) -> Tile
    private struct ScaledKey : IEquatable<ScaledKey>
    {
        public Vector2Int Scaled;
        public int Layer;
        public ScaledKey(Vector2Int scaled, int layer) { Scaled = scaled; Layer = layer; }
        public bool Equals(ScaledKey other) => Scaled == other.Scaled && Layer == other.Layer;
        public override bool Equals(object obj) => obj is ScaledKey k && Equals(k);
        public override int GetHashCode() => (Scaled.x * 397) ^ (Scaled.y * 97) ^ Layer;
    }

    // Храним последний сгенерированный набор, чтобы методы типа HasTileAbove/IsTileAvailable работали после Generate
    private static Dictionary<ScaledKey, Tile> _map = new Dictionary<ScaledKey, Tile>();
    private static List<Tile> _tiles = new List<Tile>();
    private static int _nextId = 1;

    // --- Вспомогательные конвертеры ---
    private static Vector2Int ToScaled(Vector2 world) => Vector2Int.RoundToInt(world * 2f);
    private static Vector2Int ToScaled(Vector2Int gridInt) => gridInt * 2; 
    private static Vector2 ScaledToWorld(Vector2Int scaled) => new Vector2(scaled.x * 0.5f, scaled.y * 0.5f);
    private static ScaledKey MakeKey(Vector2Int scaled, int layer) => new ScaledKey(scaled, layer);

    // --- Публичный API ---

    /// <summary>
    /// Сгенерировать плитки.
    /// baseLayer: bool[width,height] (индексация [x,y] — поэтому код читает как baseLayer[x,y]).
    /// totalTilesLimit: общее количество плиток которое нужно поставить (если не хватит позиций — генерация остановится).
    /// targetLayers: целевое число слоёв (0 = только базовый слой).
    /// Возвращает список Tile (мировые позиции, слой и начально IsAvailable=false; после генерации можно вызвать UpdateAllAvailabilities).
    /// </summary>
    public static List<Tile> Generate(bool[,] baseLayer, int totalTilesLimit, int targetLayers)
    {
        // очистка состояния
        _map = new Dictionary<ScaledKey, Tile>();
        _tiles = new List<Tile>();
        _nextId = 1;

        if (baseLayer == null) throw new ArgumentNullException(nameof(baseLayer));
        int width = baseLayer.GetLength(0);
        int height = baseLayer.GetLength(1);

        int placed = 0;

        // 1) заполняем базовый слой (layer = 0). world positions = (x,y) целые
        var basePositions = new List<Vector2>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (baseLayer[x, y])
                    basePositions.Add(new Vector2(x, y));

        Shuffle(basePositions);

        foreach (var wp in basePositions)
        {
            if (placed >= totalTilesLimit) break;
            var tile = NewTile(wp, 0);
            PutTile(tile);
            placed++;
        }

        // 2) строим верхние слои до targetLayers-1 (если targetLayers == 1, только базовый)
        for (int layer = 1; layer < targetLayers; layer++)
        {
            // генерируем все возможные позиции на этом слое из текущего состояния предыдущего слоя
            var candidates = GenerateCandidatesForLayer(layer);
            if (candidates.Count == 0) break; // дальше некуда ставить

            Shuffle(candidates);

            foreach (var wp in candidates)
            {
                if (placed >= totalTilesLimit) break;
                // защита от повторной вставки (может появиться дубликат кандидата)
                var key = MakeKey(ToScaled(wp), layer);
                if (_map.ContainsKey(key)) continue;

                var tile = NewTile(wp, layer);
                PutTile(tile);
                placed++;
            }

            if (placed >= totalTilesLimit) break;
        }

        // Обновим доступности сразу
        UpdateAllAvailabilities();

        // копируем результат (чтобы не дать наружу ссылку на внутренний список)
        return new List<Tile>(_tiles);
    }

    /// <summary>Пересчитать IsAvailable для всех плиток (вызывать после удаления/слияния).</summary>
    public static void UpdateAllAvailabilities()
    {
        // простая стратегия: сначала сбросим, затем для каждой плитки посчитаем
        foreach (var t in _tiles) t.IsAvailable = false;
        foreach (var t in _tiles)
        {
            t.IsAvailable = IsTileAvailable(t);
        }
    }

    /// <summary>Проверить, есть ли сверху плитка (true если есть).</summary>
    public static bool HasTileAbove(Tile tile)
    {
        if (tile == null) return false;
        return HasTileAboveAtWorld(tile.WorldPos, tile.Layer);
    }

    /// <summary>Вернуть первую найденную плитку сверху (или null).</summary>
    public static Tile GetTileAbove(Tile tile)
    {
        if (tile == null) return null;
        return GetTileAboveAtWorld(tile.WorldPos, tile.Layer);
    }

    /// <summary>Проверка доступности одной плитки (без побочных эффектов).</summary>
    public static bool IsTileAvailable(Tile tile)
    {
        if (tile == null) return false;

        // 1) если есть плитка выше - недоступна
        if (HasTileAbove(tile)) return false;

        // 2) нужно, чтобы была свободна хотя бы одна сторона (лево или право) на этом же слое
        // используем scaled-координаты для точной проверки соседей
        var s = tile.ScaledPos;
        int layer = tile.Layer;

        // соседи "влево" и "вправо" в scaled координатах — обычно ±2 по X (т.е. 1 в world)
        // но из-за смещений центр/опоры могут располагаться по диагонали; мы проверим стандартный набор соседей,
        // который покрывает возможные блокировки: X ±2 на тех же Y, а также X ±2 на Y ±1 (маленькая расширенная проверка)
        Vector2Int[] leftCandidates = {
            new Vector2Int(s.x - 2, s.y),
            new Vector2Int(s.x - 2, s.y + 1),
            new Vector2Int(s.x - 2, s.y - 1)
        };

        Vector2Int[] rightCandidates = {
            new Vector2Int(s.x + 2, s.y),
            new Vector2Int(s.x + 2, s.y + 1),
            new Vector2Int(s.x + 2, s.y - 1)
        };

        bool leftBlocked = false;
        foreach (var c in leftCandidates)
            if (_map.ContainsKey(MakeKey(c, layer))) { leftBlocked = true; break; }

        bool rightBlocked = false;
        foreach (var c in rightCandidates)
            if (_map.ContainsKey(MakeKey(c, layer))) { rightBlocked = true; break; }

        // доступна если хотя бы одна сторона свободна
        return !(leftBlocked && rightBlocked);
    }

    // --- Внутренние вспомогательные ---

    // Создать новый Tile с уникальным id
    private static Tile NewTile(Vector2 worldPos, int layer)
    {
        var t = new Tile(worldPos, layer, _nextId++);
        return t;
    }

    // Положить в карты и списки
    private static void PutTile(Tile tile)
    {
        var key = MakeKey(tile.ScaledPos, tile.Layer);
        if (!_map.ContainsKey(key))
        {
            _map[key] = tile;
            _tiles.Add(tile);
        }
    }

    // Генерация кандидатных позиций для слоя L (опирается на наличие 4 опор в L-1)
    private static List<Vector2> GenerateCandidatesForLayer(int layer)
    {
        var candidates = new List<Vector2>();

        // собираем scaled-координаты предыдущего слоя
        var prevScaled = new HashSet<Vector2Int>();
        foreach (var kv in _map)
            if (kv.Key.Layer == layer - 1)
                prevScaled.Add(kv.Key.Scaled);

        // для каждого scaled в prevScaled проверяем, есть ли полный квадрат 2x2: s, s+(2,0), s+(0,2), s+(2,2)
        var seenCenters = new HashSet<Vector2Int>();
        foreach (var s in prevScaled)
        {
            var a = s;
            var b = s + new Vector2Int(2, 0);
            var c = s + new Vector2Int(0, 2);
            var d = s + new Vector2Int(2, 2);

            if (prevScaled.Contains(a) && prevScaled.Contains(b) && prevScaled.Contains(c) && prevScaled.Contains(d))
            {
                // центр в scaled coords
                var centerScaled = s + new Vector2Int(1, 1);
                if (seenCenters.Contains(centerScaled)) continue;
                seenCenters.Add(centerScaled);

                // превратить в world pos
                Vector2 world = ScaledToWorld(centerScaled);

                // защита от существования плитки уже в этой позиции (может быть добавлена ранее)
                var key = MakeKey(centerScaled, layer);
                if (!_map.ContainsKey(key))
                    candidates.Add(world);
            }
        }

        return candidates;
    }

    // Проверить наличие плитки "над" позицией worldPos на слое+1.
    // Учитываем, что опора смещена: соответствующие верхние scaled позиции будут находиться на +/-1 по обеим осям от базового scaled
    private static bool HasTileAboveAtWorld(Vector2 worldPos, int layer)
    {
        int targetLayer = layer + 1;
        var baseScaled = ToScaled(worldPos);

        // возможные позиции "центров" выше: (baseScaled + (±1, ±1))
        Vector2Int[] offsets = {
            new Vector2Int(1, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, -1)
        };

        foreach (var off in offsets)
        {
            var key = MakeKey(baseScaled + off, targetLayer);
            if (_map.ContainsKey(key)) return true;
        }
        return false;
    }

    private static Tile GetTileAboveAtWorld(Vector2 worldPos, int layer)
    {
        int targetLayer = layer + 1;
        var baseScaled = ToScaled(worldPos);

        Vector2Int[] offsets = {
            new Vector2Int(1, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, -1)
        };

        foreach (var off in offsets)
        {
            var key = MakeKey(baseScaled + off, targetLayer);
            if (_map.TryGetValue(key, out var t)) return t;
        }
        return null;
    }

    // --- Утилиты ---

    // Лёгкий Fisher-Yates shuffle
    private static System.Random _rand = new System.Random();
    private static void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = _rand.Next(n--);
            T tmp = list[n];
            list[n] = list[k];
            list[k] = tmp;
        }
    }

    // --- Доп. полезные методы ---

    /// <summary>Удаляет плитку (например, после слияния) и обновляет внутренние структуры. Возвращает true если удалено.</summary>
    public static bool RemoveTile(Tile tile)
    {
        if (tile == null) return false;
        var key = MakeKey(tile.ScaledPos, tile.Layer);
        if (!_map.ContainsKey(key)) return false;
        _map.Remove(key);
        _tiles.Remove(tile);
        return true;
    }

    /// <summary>Найти Tile по worldPos и layer (если есть) — удобно для связи TileView -> Tile.</summary>
    public static Tile FindTileByWorldPos(Vector2 worldPos, int layer)
    {
        var key = MakeKey(ToScaled(worldPos), layer);
        if (_map.TryGetValue(key, out var t)) return t;
        return null;
    }
}
