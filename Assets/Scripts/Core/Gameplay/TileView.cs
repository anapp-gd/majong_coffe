using DG.Tweening;
using UnityEngine;

public class TileView : MonoBehaviour
{
    public Vector2Int GridPos { get; set; }
    public Vector2 WorldPos { get; set; }
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

    /*public bool IsAvailable(TileView[,] layerTiles, int currentLayer)
    {
        if (LayerIndex != currentLayer)
            return false;

        bool freeLeft = GridPos.x - 1 < 0 || layerTiles[GridPos.x - 1, GridPos.y] == null;
        bool freeRight = GridPos.x + 1 >= layerTiles.GetLength(0) || layerTiles[GridPos.x + 1, GridPos.y] == null;

        return freeLeft || freeRight;
    }*/

    public bool IsAvailable(int currentLayer)
    {
        if (LayerIndex != currentLayer)
            return false;

        float checkDistance = 1.2f;
        int layerMask = 1 << gameObject.layer;

        float halfWidth = _collider.bounds.extents.x; // половина ширины коллайдера
        Vector2 originLeft = (Vector2)transform.position + Vector2.left * (halfWidth + 0.01f);
        Vector2 originRight = (Vector2)transform.position + Vector2.right * (halfWidth + 0.01f);

        // Влево
        RaycastHit2D hitLeft = Physics2D.Raycast(originLeft, Vector2.left, checkDistance, layerMask);
        bool freeLeft = hitLeft.collider == null;

        // Вправо
        RaycastHit2D hitRight = Physics2D.Raycast(originRight, Vector2.right, checkDistance, layerMask);
        bool freeRight = hitRight.collider == null;

        // (Опционально для дебага)
        Debug.DrawRay(originLeft, Vector2.left * checkDistance, Color.red, 0.1f);
        Debug.DrawRay(originRight, Vector2.right * checkDistance, Color.green, 0.1f);

        return freeLeft || freeRight;
    }

/*
    public bool IsAvailable(int currentLayer)
    {
        if (LayerIndex != currentLayer)
            return false;

        float checkDistance = 1.2f; // расстояние для проверки
        int layerMask = 1 << gameObject.layer; // тот же Physics2D слой, что и у плиток

        // Влево
        RaycastHit2D hitLeft = Physics2D.Raycast(WorldPos, Vector2.left, checkDistance, layerMask);
        bool freeLeft = hitLeft.collider == null;

        // Вправо
        RaycastHit2D hitRight = Physics2D.Raycast(WorldPos, Vector2.right, checkDistance, layerMask);
        bool freeRight = hitRight.collider == null;

        return freeLeft || freeRight;
    }
*/    
    /*
    public bool IsAvailable(TileView[,] layerTiles, int currentLayer)
    {
        if (LayerIndex != currentLayer)
            return false;

        float checkDistance = 1.2f; // максимально допустимое расстояние для соседей
        bool freeLeft = true;
        bool freeRight = true;

        int width = layerTiles.GetLength(0);
        int height = layerTiles.GetLength(1);

        // проверяем все плитки на том же слое
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileView tile = layerTiles[x, y];
                if (tile == null || tile == this || tile.LayerIndex != currentLayer)
                    continue;

                float dx = tile.WorldPos.x - WorldPos.x;

                if (dx > 0 && dx < checkDistance)
                    freeRight = false;

                if (dx < 0 && Mathf.Abs(dx) < checkDistance)
                    freeLeft = false;
            }
        }

        return freeLeft || freeRight;
    }
*/

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
