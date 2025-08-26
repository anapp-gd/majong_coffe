using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    public event Action OnLose;
    public event Action OnWin;

    [SerializeField] private TileView tileViewPrefab;
    [SerializeField] private float tileSize = 1f;
    [SerializeField, Range(0f, 1f)] private float lowerLayerDarken = 0.5f;

    private LevelData _levelData;
    private PlayState _state;

    private readonly List<TileView> _allTiles = new List<TileView>();
    public int CurrentLayer { get; private set; }
    private int _layersCount = 3;
    private PlayState.PlayStatus _status;

    public void Init(PlayState state, Vector2 customOffset)
    {
        _state = state;
        _allTiles.Clear();

        _state.PlayStatusChanged += OnStatusChange;

        int currentLevel = PlayerEntity.Instance.GetCurrentLevel;

        if (ConfigModule.GetConfig<LevelConfig>().TryGetLevelData(currentLevel, out var levelData))
        {
            _levelData = levelData;
        }

        var data = MadjongGenerator.Generate(_levelData);

        _layersCount = data.layersCount;
        CurrentLayer = _layersCount - 1;
        _state.SetHashDishes = data.dishes;
         
        int maxX = 0, maxY = 0;
        foreach (var t in data.tiles)
        {
            if (t.WorldPos.x > maxX) maxX = t.GridPos.x;
            if (t.WorldPos.y > maxY) maxY = t.GridPos.y;
        }

        float offsetX = -(maxX * tileSize) / 2f + customOffset.x;
        float offsetY = -(maxY * tileSize) / 2f + customOffset.y;

        foreach (var tileData in data.tiles)
        {
            Vector3 worldPos = new Vector3
            (
                tileData.WorldPos.x * tileSize + offsetX,
                tileData.WorldPos.y * tileSize + offsetY,
                0f
            );

            var tile = Instantiate(tileViewPrefab, worldPos, Quaternion.identity, transform);
            tile.GridPos = tileData.GridPos;
            tile.WorldPos = worldPos;
            tile.Init(state, tileData.TileType, tileData.Layer);

            _allTiles.Add(tile);

            var spriteRenderer = tile.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = tileData.Layer;
                spriteRenderer.color = Color.HSVToRGB(((int)tile.TileType % 12) / 12f, 0.8f, 1f);

                if (tileData.Layer < CurrentLayer)
                {
                    spriteRenderer.color *= lowerLayerDarken;
                    tile.Disable();
                }
            }
        }
    } 

    void OnStatusChange(PlayState.PlayStatus playStatus)
    {
        _status = playStatus;
    }
    public void RemoveTiles(TileView a, TileView b)
    {
        if (_status != PlayState.PlayStatus.play) return;

        Vector3 joinPoint = (a.transform.position + b.transform.position) / 2f;

        _allTiles.Remove(a);
        _allTiles.Remove(b);

        Sequence seq = DOTween.Sequence();

        seq.Join(a.RemoveWithJoin(joinPoint));
        seq.Join(b.RemoveWithJoin(joinPoint));

        // Когда обе анимации завершены
        seq.OnComplete(() =>
        {
            CheckLayerAfterRemove(a.LayerIndex);
        });
    }
    private void CheckLayerAfterRemove(int layer)
    {
        if (IsLayerCleared(layer))
        {
            int nextLayer = layer - 1;
            CurrentLayer = nextLayer;

            Debug.Log($"Слой {layer} очищен! Теперь активен слой {CurrentLayer}");

            if (CurrentLayer >= 0)
            {
                ApplyNormalColorToLayer(CurrentLayer);

                if (!HasAvailableMoves(CurrentLayer))
                {
                    Debug.Log("Нет доступных ходов — GAME OVER");
                    OnLose?.Invoke();
                }
            }
            else
            {
                Debug.Log("Все слои очищены — WIN");
                _state.BoardClean();
                OnWin?.Invoke();
            }
        }
        else
        {
            if (!HasAvailableMoves(layer))
            {
                Debug.Log("Нет доступных ходов — GAME OVER");
                OnLose?.Invoke();
            }
        }
    }

    public bool IsBoardClear()
    {
        return _allTiles.Count == 0;
    }
    private bool IsLayerCleared(int layer)
    {
        return !_allTiles.Any(t => t.LayerIndex == layer);
    }
    public bool HasAvailableMoves(int layer)
    {
        var clickable = GetTilesOnLayer(layer)
            .Where(t => t.IsAvailable(layer))
            .ToList();

        for (int i = 0; i < clickable.Count; i++)
        {
            for (int j = i + 1; j < clickable.Count; j++)
            {
                if (clickable[i].TileType == clickable[j].TileType)
                { 
                    return true;
                }
            }
        }

        return false;
    }
    public IEnumerable<TileView> GetTilesOnLayer(int layer)
    {
        return _allTiles.Where(t => t.LayerIndex == layer);
    } 
    private void ApplyNormalColorToLayer(int layer)
    {
        foreach (var tile in GetTilesOnLayer(layer))
        {
            tile.Enable();
            var spriteRenderer = tile.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.HSVToRGB(((int)tile.TileType % 12) / 12f, 0.8f, 1f);
            }
        }
    }
}
