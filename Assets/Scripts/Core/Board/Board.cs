using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{ 
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private LevelData _data;

    private List<Tile> tiles = new List<Tile>();
    private PlayState _state;

    void Start()
    {
        GenerateProceduralField(_data);
    }

    public void Init(PlayState state)
    {
        _state = state;
    }

    public void GenerateProceduralField(LevelData data)
    {
        tiles.Clear();

        // 1. Считаем общее число плиток
        int maxTiles = Mathf.RoundToInt(data.width * data.height * data.layers * data.fillPercent);
        if (maxTiles % 2 != 0) maxTiles--; // чтобы всегда было парное число

        // 2. Создаем список пар
        List<int> symbols = new List<int>();
        for (int i = 0; i < maxTiles / 2; i++)
        {
            int symbol = Random.Range(1, data.symbolTypes + 1);
            symbols.Add(symbol);
            symbols.Add(symbol);
        }

        // 3. Перемешиваем список (Fisher–Yates)
        for (int i = symbols.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (symbols[i], symbols[j]) = (symbols[j], symbols[i]);
        }

        // 4. Создаём позиции по стилю
        List<Vector3Int> positions = GeneratePositions(data);

        // 5. Размещаем плитки
        for (int i = 0; i < symbols.Count && i < positions.Count; i++)
        {
            Vector3Int pos = positions[i];
            var tile = Instantiate(tilePrefab, new Vector3(pos.x, pos.z * 0.2f, 0) + Vector3.up * pos.y, Quaternion.identity, transform);
            tile.Init(symbols[i], pos.z, this, _state);
            tiles.Add(tile);
        }

        UpdateFreeTiles();
    }

    private List<Vector3Int> GeneratePositions(LevelData cfg)
    {
        var positions = new List<Vector3Int>();

        switch (cfg.layoutStyle)
        {
            case LevelData.LayoutStyle.Cluster:
                for (int z = 0; z < cfg.layers; z++)
                {
                    int layerW = Mathf.Max(2, cfg.width - z);
                    int layerH = Mathf.Max(2, cfg.height - z);

                    for (int y = 0; y < layerH; y++)
                    {
                        for (int x = 0; x < layerW; x++)
                        {
                            if (Random.value <= cfg.fillPercent)
                                positions.Add(new Vector3Int(x - cfg.width / 2, y - cfg.height / 2, z));
                        }
                    }
                }
                break;

            case LevelData.LayoutStyle.Grid:
                for (int z = 0; z < cfg.layers; z++)
                {
                    for (int y = 0; y < cfg.height; y++)
                    {
                        for (int x = 0; x < cfg.width; x++)
                        {
                            if (Random.value <= cfg.fillPercent)
                                positions.Add(new Vector3Int(x - cfg.width / 2, y - cfg.height / 2, z));
                        }
                    }
                }
                break;

            case LevelData.LayoutStyle.RandomSpread:
                int total = Mathf.RoundToInt(cfg.width * cfg.height * cfg.layers * cfg.fillPercent);
                for (int i = 0; i < total; i++)
                {
                    int x = Random.Range(-cfg.width / 2, cfg.width / 2 + 1);
                    int y = Random.Range(-cfg.height / 2, cfg.height / 2 + 1);
                    int z = Random.Range(0, cfg.layers);
                    positions.Add(new Vector3Int(x, y, z));
                }
                break;
        }

        // Перемешиваем позиции
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (positions[i], positions[j]) = (positions[j], positions[i]);
        }

        return positions;
    }

    public void UpdateFreeTiles()
    {
        foreach (var tile in tiles)
            tile.CheckIfFree(tiles);
    }

    public void RemovePair(Tile t1, Tile t2)
    {
        t1.AnimateRemove();
        t2.AnimateRemove();
        tiles.Remove(t1);
        tiles.Remove(t2);
        UpdateFreeTiles();
    }
}
