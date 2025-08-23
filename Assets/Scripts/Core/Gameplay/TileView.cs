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
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(to, 0.25f));
        seq.Append(transform.DOScale(0f, 0.2f).OnComplete(() => Destroy(gameObject)));
        return seq;
    }
}
