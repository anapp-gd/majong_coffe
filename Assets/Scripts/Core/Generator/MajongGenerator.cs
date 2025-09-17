using System;
using System.Collections.Generic;
using UnityEngine;

public static class MajongGenerator
{
    // �������� ������, ������������ �����������
    public class Tile
    {
        public Vector2 WorldPos;       // ������ ������� ������� (����� ����� .5)
        public Vector2Int ScaledPos;   // ������������� ������� = RoundToInt(WorldPos * 2)
        public Vector2Int GridPosInt;  // ���������� ����� ������� (��� �������� UI/��������)
        public int Layer;
        public bool IsAvailable;       // �������� �� ������ ��� ������/����� (�����������)
        public int Id;                 // ���������� id (�����������, ��� �������)

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

    // ���������� �������: ���� = (ScaledPos, Layer) -> Tile
    private struct ScaledKey : IEquatable<ScaledKey>
    {
        public Vector2Int Scaled;
        public int Layer;
        public ScaledKey(Vector2Int scaled, int layer) { Scaled = scaled; Layer = layer; }
        public bool Equals(ScaledKey other) => Scaled == other.Scaled && Layer == other.Layer;
        public override bool Equals(object obj) => obj is ScaledKey k && Equals(k);
        public override int GetHashCode() => (Scaled.x * 397) ^ (Scaled.y * 97) ^ Layer;
    }

    // ������ ��������� ��������������� �����, ����� ������ ���� HasTileAbove/IsTileAvailable �������� ����� Generate
    private static Dictionary<ScaledKey, Tile> _map = new Dictionary<ScaledKey, Tile>();
    private static List<Tile> _tiles = new List<Tile>();
    private static int _nextId = 1;

    // --- ��������������� ���������� ---
    private static Vector2Int ToScaled(Vector2 world) => Vector2Int.RoundToInt(world * 2f);
    private static Vector2Int ToScaled(Vector2Int gridInt) => gridInt * 2; 
    private static Vector2 ScaledToWorld(Vector2Int scaled) => new Vector2(scaled.x * 0.5f, scaled.y * 0.5f);
    private static ScaledKey MakeKey(Vector2Int scaled, int layer) => new ScaledKey(scaled, layer);

    // --- ��������� API ---

    /// <summary>
    /// ������������� ������.
    /// baseLayer: bool[width,height] (���������� [x,y] � ������� ��� ������ ��� baseLayer[x,y]).
    /// totalTilesLimit: ����� ���������� ������ ������� ����� ��������� (���� �� ������ ������� � ��������� �����������).
    /// targetLayers: ������� ����� ���� (0 = ������ ������� ����).
    /// ���������� ������ Tile (������� �������, ���� � �������� IsAvailable=false; ����� ��������� ����� ������� UpdateAllAvailabilities).
    /// </summary>
    public static List<Tile> Generate(bool[,] baseLayer, int totalTilesLimit, int targetLayers)
    {
        // ������� ���������
        _map = new Dictionary<ScaledKey, Tile>();
        _tiles = new List<Tile>();
        _nextId = 1;

        if (baseLayer == null) throw new ArgumentNullException(nameof(baseLayer));
        int width = baseLayer.GetLength(0);
        int height = baseLayer.GetLength(1);

        int placed = 0;

        // 1) ��������� ������� ���� (layer = 0). world positions = (x,y) �����
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

        // 2) ������ ������� ���� �� targetLayers-1 (���� targetLayers == 1, ������ �������)
        for (int layer = 1; layer < targetLayers; layer++)
        {
            // ���������� ��� ��������� ������� �� ���� ���� �� �������� ��������� ����������� ����
            var candidates = GenerateCandidatesForLayer(layer);
            if (candidates.Count == 0) break; // ������ ������ �������

            Shuffle(candidates);

            foreach (var wp in candidates)
            {
                if (placed >= totalTilesLimit) break;
                // ������ �� ��������� ������� (����� ��������� �������� ���������)
                var key = MakeKey(ToScaled(wp), layer);
                if (_map.ContainsKey(key)) continue;

                var tile = NewTile(wp, layer);
                PutTile(tile);
                placed++;
            }

            if (placed >= totalTilesLimit) break;
        }

        // ������� ����������� �����
        UpdateAllAvailabilities();

        // �������� ��������� (����� �� ���� ������ ������ �� ���������� ������)
        return new List<Tile>(_tiles);
    }

    /// <summary>����������� IsAvailable ��� ���� ������ (�������� ����� ��������/�������).</summary>
    public static void UpdateAllAvailabilities()
    {
        // ������� ���������: ������� �������, ����� ��� ������ ������ ���������
        foreach (var t in _tiles) t.IsAvailable = false;
        foreach (var t in _tiles)
        {
            t.IsAvailable = IsTileAvailable(t);
        }
    }

    /// <summary>���������, ���� �� ������ ������ (true ���� ����).</summary>
    public static bool HasTileAbove(Tile tile)
    {
        if (tile == null) return false;
        return HasTileAboveAtWorld(tile.WorldPos, tile.Layer);
    }

    /// <summary>������� ������ ��������� ������ ������ (��� null).</summary>
    public static Tile GetTileAbove(Tile tile)
    {
        if (tile == null) return null;
        return GetTileAboveAtWorld(tile.WorldPos, tile.Layer);
    }

    /// <summary>�������� ����������� ����� ������ (��� �������� ��������).</summary>
    public static bool IsTileAvailable(Tile tile)
    {
        if (tile == null) return false;

        // 1) ���� ���� ������ ���� - ����������
        if (HasTileAbove(tile)) return false;

        // 2) �����, ����� ���� �������� ���� �� ���� ������� (���� ��� �����) �� ���� �� ����
        // ���������� scaled-���������� ��� ������ �������� �������
        var s = tile.ScaledPos;
        int layer = tile.Layer;

        // ������ "�����" � "������" � scaled ����������� � ������ �2 �� X (�.�. 1 � world)
        // �� ��-�� �������� �����/����� ����� ������������� �� ���������; �� �������� ����������� ����� �������,
        // ������� ��������� ��������� ����������: X �2 �� ��� �� Y, � ����� X �2 �� Y �1 (��������� ����������� ��������)
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

        // �������� ���� ���� �� ���� ������� ��������
        return !(leftBlocked && rightBlocked);
    }

    // --- ���������� ��������������� ---

    // ������� ����� Tile � ���������� id
    private static Tile NewTile(Vector2 worldPos, int layer)
    {
        var t = new Tile(worldPos, layer, _nextId++);
        return t;
    }

    // �������� � ����� � ������
    private static void PutTile(Tile tile)
    {
        var key = MakeKey(tile.ScaledPos, tile.Layer);
        if (!_map.ContainsKey(key))
        {
            _map[key] = tile;
            _tiles.Add(tile);
        }
    }

    // ��������� ����������� ������� ��� ���� L (��������� �� ������� 4 ���� � L-1)
    private static List<Vector2> GenerateCandidatesForLayer(int layer)
    {
        var candidates = new List<Vector2>();

        // �������� scaled-���������� ����������� ����
        var prevScaled = new HashSet<Vector2Int>();
        foreach (var kv in _map)
            if (kv.Key.Layer == layer - 1)
                prevScaled.Add(kv.Key.Scaled);

        // ��� ������� scaled � prevScaled ���������, ���� �� ������ ������� 2x2: s, s+(2,0), s+(0,2), s+(2,2)
        var seenCenters = new HashSet<Vector2Int>();
        foreach (var s in prevScaled)
        {
            var a = s;
            var b = s + new Vector2Int(2, 0);
            var c = s + new Vector2Int(0, 2);
            var d = s + new Vector2Int(2, 2);

            if (prevScaled.Contains(a) && prevScaled.Contains(b) && prevScaled.Contains(c) && prevScaled.Contains(d))
            {
                // ����� � scaled coords
                var centerScaled = s + new Vector2Int(1, 1);
                if (seenCenters.Contains(centerScaled)) continue;
                seenCenters.Add(centerScaled);

                // ���������� � world pos
                Vector2 world = ScaledToWorld(centerScaled);

                // ������ �� ������������� ������ ��� � ���� ������� (����� ���� ��������� �����)
                var key = MakeKey(centerScaled, layer);
                if (!_map.ContainsKey(key))
                    candidates.Add(world);
            }
        }

        return candidates;
    }

    // ��������� ������� ������ "���" �������� worldPos �� ����+1.
    // ���������, ��� ����� �������: ��������������� ������� scaled ������� ����� ���������� �� +/-1 �� ����� ���� �� �������� scaled
    private static bool HasTileAboveAtWorld(Vector2 worldPos, int layer)
    {
        int targetLayer = layer + 1;
        var baseScaled = ToScaled(worldPos);

        // ��������� ������� "�������" ����: (baseScaled + (�1, �1))
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

    // --- ������� ---

    // ˸���� Fisher-Yates shuffle
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

    // --- ���. �������� ������ ---

    /// <summary>������� ������ (��������, ����� �������) � ��������� ���������� ���������. ���������� true ���� �������.</summary>
    public static bool RemoveTile(Tile tile)
    {
        if (tile == null) return false;
        var key = MakeKey(tile.ScaledPos, tile.Layer);
        if (!_map.ContainsKey(key)) return false;
        _map.Remove(key);
        _tiles.Remove(tile);
        return true;
    }

    /// <summary>����� Tile �� worldPos � layer (���� ����) � ������ ��� ����� TileView -> Tile.</summary>
    public static Tile FindTileByWorldPos(Vector2 worldPos, int layer)
    {
        var key = MakeKey(ToScaled(worldPos), layer);
        if (_map.TryGetValue(key, out var t)) return t;
        return null;
    }
}
