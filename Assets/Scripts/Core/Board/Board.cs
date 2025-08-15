using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    // Событие: нет ходов
    public event Action OnNoMoves;
    // (Опционально) событие: вся доска очищена (победа)
    public event Action OnBoardCleared;

    [SerializeField] TileView tileView;
    [SerializeField] int layersCount = 3;          // Кол-во слоёв
    [SerializeField] int pairsCount = 10;          // Кол-во пар плиток
    [SerializeField] int tileTypesCount = 5;       // Кол-во типов плиток
    [SerializeField] float tileSize = 1f;          // Размер одной плитки
    [SerializeField, Range(0f, 1f)] float lowerLayerDarken = 0.5f; // Затемнение нижних слоёв

    public int CurrentLayer { get; private set; }

    // Храним все тайлы по слоям
    private Dictionary<int, TileView[,]> _layers = new Dictionary<int, TileView[,]>();

    public void Init(PlayState state)
    {
        CurrentLayer = layersCount - 1; // Начинаем с верхнего слоя
        _layers.Clear();

        var tiles = MadjongGenerator.Generate(layersCount, pairsCount, tileTypesCount);

        // Чтобы всё было по центру
        int maxX = 0, maxY = 0;
        foreach (var t in tiles)
        {
            if (t.position.x > maxX) maxX = t.position.x;
            if (t.position.y > maxY) maxY = t.position.y;
        }

        float offsetX = -(maxX * tileSize) / 2f;
        float offsetY = -(maxY * tileSize) / 2f;

        // Создаём массивы для каждого слоя
        for (int l = 0; l < layersCount; l++)
        {
            _layers[l] = new TileView[maxX + 1, maxY + 1];
        }

        // Создаём тайлы
        foreach (var data in tiles)
        {
            Vector3 worldPos = new Vector3(
                data.position.x * tileSize + offsetX,
                data.position.y * tileSize + offsetY,
                0f
            );

            var tile = Instantiate(tileView, worldPos, Quaternion.identity, transform);
            tile.GridPos = data.position;
            tile.Init(state, data.TileType, data.layer);

            _layers[data.layer][data.position.x, data.position.y] = tile;

            var spriteRenderer = tile.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = data.layer;

                spriteRenderer.color = Color.HSVToRGB(((int)data.TileType % 12) / 12f, 0.8f, 1f);

                if (data.layer < CurrentLayer)
                {
                    spriteRenderer.color *= lowerLayerDarken;
                    tile.Disable();
                }
            }
        }
    }


    public void RemoveTiles(TileView a, TileView b)
    {
        Vector3 joinPoint = (a.transform.position + b.transform.position) / 2f;

        _layers[a.LayerIndex][a.GridPos.x, a.GridPos.y] = null;
        _layers[b.LayerIndex][b.GridPos.x, b.GridPos.y] = null;

        a.RemoveWithJoin(joinPoint);
        b.RemoveWithJoin(joinPoint);

        StartCoroutine(WaitAndCheckLayer(a.LayerIndex));
    }

    private IEnumerator WaitAndCheckLayer(int layer)
    {
        yield return new WaitForSeconds(0.3f); // ждём завершения анимации

        if (IsLayerCleared(layer))
        {
            int nextLayer = layer - 1;
            CurrentLayer = nextLayer;

            Debug.Log($"Слой {layer} очищен! Теперь активен слой {CurrentLayer}");

            // Если открылся новый слой, восстановим нормальный цвет для него
            if (CurrentLayer >= 0)
            {
                ApplyNormalColorToLayer(CurrentLayer);

                // Проверяем доступные ходы
                if (!HasAvailableMoves(CurrentLayer))
                {
                    Debug.Log("Нет доступных ходов — GAME OVER");
                    OnNoMoves?.Invoke();
                }
            }
            else
            {
                // Все слои очищены — игрок победил
                Debug.Log("Все слои очищены — WIN");
                OnBoardCleared?.Invoke();
            }
        }
    }


    private bool IsLayerCleared(int layer)
    {
        var layerTiles = GetTilesOnLayer(layer);
        if (layerTiles == null) return true;

        for (int x = 0; x < layerTiles.GetLength(0); x++)
        {
            for (int y = 0; y < layerTiles.GetLength(1); y++)
            {
                if (layerTiles[x, y] != null)
                    return false;
            }
        }
        return true;
    }
    public TileView[,] GetTilesOnLayer(int layer)
    {
        return _layers.ContainsKey(layer) ? _layers[layer] : null;
    }

    // Проверяет: есть ли на слое хотя бы одна пара плиток, которые можно выбрать (оба кликабельны и одного типа)
    public bool HasAvailableMoves(int layer)
    {
        var layerTiles = GetTilesOnLayer(layer);
        if (layerTiles == null) return false;

        // Собираем все кликабельные плитки на слое
        List<TileView> clickable = new List<TileView>();
        for (int x = 0; x < layerTiles.GetLength(0); x++)
        {
            for (int y = 0; y < layerTiles.GetLength(1); y++)
            {
                var tile = layerTiles[x, y];
                if (tile == null) continue;

                // Используем существующий метод IsClickable
                if (tile.IsAvailable(layerTiles, CurrentLayer))
                {
                    clickable.Add(tile);
                }
            }
        }

        // Группируем по типу — если у какого-то типа >=2 => есть ход
        Dictionary<Enums.TileType, int> counts = new Dictionary<Enums.TileType, int>();
        foreach (var t in clickable)
        {
            if (!counts.ContainsKey(t.TileType)) counts[t.TileType] = 0;
            counts[t.TileType]++;
            if (counts[t.TileType] >= 2) return true;
        }

        return false;
    }

    // Восстанавливаем "нормальный" цвет для слоя (ту же формулу, что использовалась при создании)
    private void ApplyNormalColorToLayer(int layer)
    {
        var layerTiles = GetTilesOnLayer(layer);
        if (layerTiles == null) return;

        for (int x = 0; x < layerTiles.GetLength(0); x++)
        {
            for (int y = 0; y < layerTiles.GetLength(1); y++)
            {
                var tile = layerTiles[x, y];

                if (tile == null) continue;
                tile.Enable();

                var spriteRenderer = tile.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.HSVToRGB(((int)tile.TileType % 12) / 12f, 0.8f, 1f);
                }
            }
        }
    }
}
