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
    public int CurrentLayer { get; private set; }
    private int _layersCount = 3;
    private PlayState.PlayStatus _status;

    public void Init(PlayState state, Vector2 customOffset, LevelData levelData)
    {
        _state = state;
        _allTiles.Clear();

        _state.PlayStatusChanged += OnStatusChange;

        _levelData = levelData;

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

        float offsetX = -(maxX * tileSizeX) / 2f + customOffset.x;
        float offsetY = -(maxY * tileSizeY) / 2f + customOffset.y;

        foreach (var tileData in data.tiles)
        {
            Vector3 worldPos = new Vector3
            (
                tileData.WorldPos.x * tileSizeX + offsetX,
                tileData.WorldPos.y * tileSizeY + offsetY,
                0f
            );
             
            var tile = Instantiate(tileViewPrefab, worldPos, Quaternion.identity, transform); 
            tile.Init(state, tileData, tileData.TileType, tileData.Layer);

            _allTiles.Add(tile);

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

    void OnStatusChange(PlayState.PlayStatus playStatus)
    {
        _status = playStatus;
    }

    public void RemoveTiles(TileView a, TileView b, Action<Vector3> spawn, Action<Vector3> callback)
    {
        if (_status != PlayState.PlayStatus.play) return;

        Vector3 joinPoint = (a.transform.position + b.transform.position) / 2f;

        _allTiles.Remove(a);
        _allTiles.Remove(b);
         
        Sequence seq = DOTween.Sequence();

        seq.Join(a.RemoveWithJoin(joinPoint, spawn));
        seq.Join(b.RemoveWithJoin(joinPoint, spawn));

        // Когда обе анимации завершены
        seq.OnComplete(() =>
        {
            callback?.Invoke(joinPoint);
            CheckLayerAfterRemove();
        });
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
