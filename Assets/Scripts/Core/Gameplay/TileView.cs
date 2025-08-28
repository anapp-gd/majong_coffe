using DG.Tweening;
using UnityEngine;

public class TileView : MonoBehaviour
{
    public Vector2Int GridPos;
    public Vector2 WorldPos;
    public int LayerIndex => _layer;
    public Enums.TileType TileType => _tileType;

    [SerializeField, Range(0f, 1f)] private float _lowerLayerDarken = 0.5f;
    [SerializeField, ReadOnlyInspector] private Enums.TileType _tileType;
    [SerializeField, ReadOnlyInspector] private int _layer;
    [SerializeField] private SpriteRenderer _renderer;
    private SpriteRenderer _view;
    private PlayState _state;
    private BoxCollider2D _collider;
    private MadjongGenerator.TileData _data;
    private Color _disableColor;
    private Color _enableColor; 
    private Vector3 _baseScale;
    public void Init(PlayState state, MadjongGenerator.TileData data, Enums.TileType type, int layer)
    {
        _state = state;
        _tileType = type;
        _layer = layer;
        _data = data;
        _view = GetComponent<SpriteRenderer>();

        GridPos = data.GridPos;
        WorldPos = data.WorldPos;

        //_view.color = Color.HSVToRGB(((int)data.TileType % 12) / 12f, 0.8f, 1f);
        _view.sortingOrder = data.Layer * 10;
        _renderer.sortingOrder = _view.sortingOrder + 1;

        _collider = GetComponent<BoxCollider2D>();
        _collider.isTrigger = true;

        Color color = _view.color;

        _enableColor = color; 
        _disableColor = new Color
            (
        color.r * _lowerLayerDarken,
        color.g * _lowerLayerDarken,
        color.b * _lowerLayerDarken,
        color.a // альфа остаётся прежней
        );

        _baseScale = transform.localScale; // запоминаем базовый размер

        var textureConfig = ConfigModule.GetConfig<TextureConfig>();

        if (textureConfig.TryGetTexture(type, out Sprite texture))
        { 
            _renderer.sprite = texture;
        }
    }

    public void Disable()
    {
        Debug.Log($"Disable {_tileType}, {GridPos}, layer {_layer}");
        _view.color = _disableColor;
        _renderer.color = _disableColor;
        _collider.enabled = false;
    }

    public void Enable()
    {
        Debug.Log($"Enable {_tileType}, {GridPos}, layer {_layer}");
        _view.color = _enableColor;
        _renderer.color = _enableColor;
        _collider.enabled = true;
    }

    public bool CompareType(Enums.TileType type)
    {
        return type == _tileType;
    } 

    public bool IsAvailable()
    {
        return MadjongGenerator.IsTileFree(_data); 
    }
     
    public void SetColor(Color color)
    { 
        _view.color = color;
    }
    public void Select()
    {
        transform.DOScale(_baseScale * 1.08f, 0.15f)
                 .SetLoops(2, LoopType.Yoyo);
    }
     
    public void Deselect()
    {
        transform.DOScale(_baseScale, 0.1f);
    }

    public Tween RemoveWithJoin(Vector3 to)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(to, 0.25f));
        seq.Append(transform.DOScale(0f, 0.2f).OnComplete(onComplete));
        return seq;
    }

    void onComplete()
    {
        MadjongGenerator.RemoveTile(_data);
        Destroy(gameObject);
    }
}
