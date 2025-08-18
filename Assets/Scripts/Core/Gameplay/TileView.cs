using DG.Tweening;
using UnityEngine;

public class TileView : MonoBehaviour
{
    public Vector2Int GridPos { get; set; }
    public int LayerIndex => _layer;
    public Enums.TileType TileType => _tileType;

    [SerializeField, ReadOnlyInspector] private Enums.TileType _tileType;
    [SerializeField, ReadOnlyInspector] private int _layer;
    SpriteRenderer _view;
    private PlayState _state;
    private BoxCollider2D _collider;
    public void Init(PlayState state, Enums.TileType type, int layer)
    {
        _state = state;
        _tileType = type;
        _layer = layer;
        _view = GetComponent<SpriteRenderer>();

        _collider = gameObject.AddComponent<BoxCollider2D>();
        _collider.isTrigger = true;
    }

    public void Disable()
    {
        _collider.enabled = false;
    }

    public void Enable()
    {
        _collider.enabled = true;
    }

    public bool CompareType(Enums.TileType type)
    {
        return type == _tileType;
    }

    public bool IsAvailable(TileView[,] layerTiles, int currentLayer)
    {
        if (LayerIndex != currentLayer)
            return false;

        bool freeLeft = GridPos.x - 1 < 0 || layerTiles[GridPos.x - 1, GridPos.y] == null;
        bool freeRight = GridPos.x + 1 >= layerTiles.GetLength(0) || layerTiles[GridPos.x + 1, GridPos.y] == null;

        return freeLeft || freeRight;
    }

    public void SetColor(Color color)
    { 
        _view.color = color;
    }
     
    public void Select()
    {
        transform.DOScale(1.08f, 0.15f).SetLoops(2, LoopType.Yoyo);
    }

    public void Deselect()
    {
        transform.DOScale(1f, 0.1f);
    }

    public Tween RemoveWithJoin(Vector3 to)
    {
        return transform.DOMove(to, 0.25f).OnComplete(() =>
        {
            transform.DOScale(0f, 0.2f).OnComplete(() => Destroy(gameObject));
        });
    }
}
