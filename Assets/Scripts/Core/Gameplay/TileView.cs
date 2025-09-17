using DG.Tweening;
using System;
using UnityEngine;

public class TileView : MonoBehaviour
{
    public Vector2Int GridPos;
    public Vector2 WorldPos;
    public int LayerIndex => _layer;
    public Enums.TileType TileType => _tileType;

    [SerializeField] private AudioClip _audioMerge;
    [SerializeField] private AudioClip _audioTap;
    [SerializeField, Range(0f, 1f)] private float _lowerLayerDarken = 0.5f;
    [SerializeField, ReadOnlyInspector] private Enums.TileType _tileType;
    [SerializeField, ReadOnlyInspector] private int _layer;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private Transform _selected;
    private SpriteRenderer _viewBack;
    private PlayState _state;
    private BoxCollider2D _collider;
    private MadjongGenerator.TileData _data;
    private Color _disableColor;
    private Color _enableColor; 
    private Vector3 _baseScale;
    private AudioSource _audioSource;
    private int _sortingView;
    private int _sortingBack;
    public void Init(PlayState state, MadjongGenerator.TileData data, Enums.TileType type, int layer)
    {
        _state = state;
        _tileType = type;
        _layer = layer;
        _data = data;
        _viewBack = GetComponent<SpriteRenderer>();
        _audioSource = gameObject.AddComponent<AudioSource>();
        GridPos = data.GridPos;
        WorldPos = data.WorldPos;
        //_view.color = Color.HSVToRGB(((int)data.TileType % 12) / 12f, 0.8f, 1f);
        _viewBack.sortingOrder = data.Layer * 10;
        _renderer.sortingOrder = _viewBack.sortingOrder + 1;

        _sortingView = _renderer.sortingOrder;
        _sortingBack = _viewBack.sortingOrder;

        _collider = GetComponent<BoxCollider2D>();
        _collider.isTrigger = true;

        Color color = _viewBack.color;

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

        if (_selected) _selected.gameObject.SetActive(false);
    }

    public void Disable()
    {
        Debug.Log($"Disable {_tileType}, {GridPos}, layer {_layer}");
        _viewBack.color = _disableColor;
        _renderer.color = _disableColor;
        _collider.enabled = false;
        if (_selected) _selected.gameObject.SetActive(false);
    }

    public void Enable()
    {
        Debug.Log($"Enable {_tileType}, {GridPos}, layer {_layer}");
        _viewBack.color = _enableColor;
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
        _viewBack.color = color;
    }
    public void Select()
    {
        if (_selected) _selected.gameObject.SetActive(true);

        if (PlayerEntity.Instance.IsSound) _audioSource.PlayOneShot(_audioTap);

        SetOverlay(true);

        transform.DOScale(_baseScale * 1.08f, 0.15f)
                 .SetLoops(2, LoopType.Yoyo);
    }
     
    void SetOverlay(bool value)
    {
        if (value)
        {
            _viewBack.sortingOrder = 98;
            _renderer.sortingOrder = 99;
        }
        else
        {
            _viewBack.sortingOrder = _sortingBack;
            _renderer.sortingOrder = _sortingView;
        }
    }

    public void Deselect()
    {
        if (_selected) _selected.gameObject.SetActive(false);

        if (PlayerEntity.Instance.IsSound) _audioSource.PlayOneShot(_audioTap);

        SetOverlay(false);

        transform.DOScale(_baseScale, 0.1f);
    }

    public Tween RemoveWithJoin(Vector3 to, Action<Vector3> spawnMergeEffect)
    {
        SetOverlay(true); 

        Sequence seq = DOTween.Sequence();

        // 1. Двигаем
        seq.Append(transform.DOMove(to, 0.15f));

        // 2. Колбэк между движением и скейлом
        if (spawnMergeEffect != null)
            seq.AppendCallback(() => spawnMergeEffect(to));

        seq.AppendCallback(() =>
        {
            if (PlayerEntity.Instance.IsSound) _audioSource.PlayOneShot(_audioMerge);
        });

        // 3. Скалируем
        seq.Append(transform.DOScale(0f, 0.15f).OnComplete(onComplete));

        return seq;
    }

    void onComplete()
    {
        if (_selected) _selected.gameObject.SetActive(false);
        MadjongGenerator.RemoveTile(_data);
        Destroy(gameObject);
    }
}
