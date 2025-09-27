using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;

public class Board : MonoBehaviour
{
    public event Action OnLose;

    [SerializeField] private TileView tileViewPrefab;
    [SerializeField] private float tileSizeX = 1f;
    [SerializeField] private float tileSizeY = 1f;

    private LevelData _levelData;
    private PlayState _state;

    private readonly List<TileView> _allTiles = new List<TileView>();
    private readonly List<MadjongGenerator.TilePair> _pairs = new List<MadjongGenerator.TilePair>();
    public int CurrentLayer { get; private set; }
    private int _layersCount = 3;
    private PlayState.PlayStatus _status;

    public void Init(PlayState state, Vector2 customOffset, LevelData levelData)
    {
        _state = state;
        _allTiles.Clear();
        _pairs.Clear();

        _state.PlayStatusChanged += OnStatusChange;

        _levelData = levelData;

        var data = MadjongGenerator.Generate(_levelData);

        foreach (var pair in data.orderedPairs)
        {
            _pairs.Add(pair); 
        }

        _layersCount = data.layersCount;
        CurrentLayer = _layersCount - 1;
        _state.SetHashDishes = data.dishes;

        // --- Находим границы сетки
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (var t in data.tiles)
        {
            if (t.GridPos.x < minX) minX = t.GridPos.x;
            if (t.GridPos.x > maxX) maxX = t.GridPos.x;
            if (t.GridPos.y < minY) minY = t.GridPos.y;
            if (t.GridPos.y > maxY) maxY = t.GridPos.y;
        }

        // Центрирование по X, с учётом размера тайла
        float centerX = ((minX + maxX) / 2f) * tileSizeX;
        // Центрирование по Y (но можно смещать только customOffset.y)
        float centerY = ((minY + maxY) / 2f) * tileSizeY;

        foreach (var tileData in data.tiles)
        {
            Vector3 worldPos = new Vector3
            (
                tileData.WorldPos.x * tileSizeX - centerX,
                tileData.WorldPos.y * tileSizeY - centerY + customOffset.y,
                0f
            );

            var tile = Instantiate(tileViewPrefab, worldPos, Quaternion.identity, transform);
            tile.Init(state, tileData, tileData.TileType, tileData.Layer);

            _allTiles.Add(tile);

            if (tile.IsAvailable())
                tile.Enable();
            else
                tile.Disable();
        }
    }

    void OnStatusChange(PlayState.PlayStatus playStatus)
    {
        _status = playStatus;
    }

    public Sequence InvokeMergeEvent(TileView a, TileView b, Action<Vector3> spawn)
    {
        if (_status != PlayState.PlayStatus.play) return null;

        Vector3 joinPoint = (a.transform.position + b.transform.position) / 2f;

        _allTiles.Remove(a);
        _allTiles.Remove(b);

        a.RemoveInGenerator();
        b.RemoveInGenerator();

        Sequence seq = DOTween.Sequence();

        seq.Join(a.RemoveWithJoin(joinPoint, spawn));
        seq.Join(b.RemoveWithJoin(joinPoint, spawn));

        seq.OnComplete(() =>
        {
            CheckLayerAfterRemove();
        });

        return seq;
    }

    private void UpdateAllTiles()
    {
        foreach (var tile in _allTiles)
        {
            if (tile.IsAvailable())
            {
                tile.Enable();
            }
            else
            {
                tile.Disable();
            }
        }
    }

    private void CheckLayerAfterRemove()
    { 
        UpdateAllTiles();

        if (IsBoardClear())
        {
            _state.SetRemoveAllTiles();
        }
        else if(!HasAvailableMoves()) 
        {
            OnLose?.Invoke();
        }
    }

    public bool IsBoardClear()
    {
        return _allTiles.Count == 0;
    }
     
    public bool HasAvailableMoves()
    {
        var availables = MadjongGenerator.GetFreeTiles();

        for (int i = 0; i < availables.Count; i++)
        {
            for (int j = i + 1; j < availables.Count; j++)
            {
                if (availables[i].TileType == availables[j].TileType)
                {
                    return true;
                }
            }
        } 

        return false;
    }

    public List<MadjongGenerator.TilePair> GetTilesInOrder()
    {
        return _pairs;
    }

    public List<Enums.TileType> GetAvaiableTiles()
    { 
        var list = new List<Enums.TileType>();

        var availables = MadjongGenerator.GetFreeTiles();

        for (int i = 0; i < availables.Count; i++)
        {
            for (int j = i + 1; j < availables.Count; j++)
            {
                if (availables[i].TileType == availables[j].TileType)
                {
                    list.Add(availables[i].TileType);
                }
            }
        }

        return list;
    }

    public IEnumerable<TileView> GetTilesOnLayer(int layer)
    {
        return _allTiles.Where(t => t.LayerIndex == layer);
    }  
}
