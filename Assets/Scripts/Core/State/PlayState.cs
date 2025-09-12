using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

public class PlayState : State
{
    [SerializeField] protected AudioClip _audioWin;
    [SerializeField] protected AudioClip _audioLose;

    public event Action<PlayStatus> PlayStatusChanged;
    protected PlayStatus _status;
    protected AudioSource _audioSource;
    public static new PlayState Instance
    {
        get
        {
            return (PlayState)State.Instance;
        }
    }

    [SerializeField] protected Transform mergeEffect;
    [SerializeField] protected Vector2 offset;

    [SerializeField] protected Board board;
    [SerializeField] protected ClientService service;
    public ServingWindow ServingWindow
    {
        get => _window; 
    } 
    public ClientService ClientService
    {
        get => _client;
    } 

    protected Board _board;
    protected ServingWindow _window;
    protected ClientService _client;
    protected Camera _camera;

    protected TileView _firstTile;

    protected HashSet<Enums.DishType> _haseDish;
    public HashSet<Enums.DishType> SetHashDishes 
    { 
        set
        {
            _haseDish = value;
        }
    } 

    public int GetResaultValue => _resultValue;
    protected int _resultValue;
    protected WinConditions _winConditions; 

    protected override void Awake()
    { 
        _audioSource = gameObject.AddComponent<AudioSource>();

        int currentLevel = PlayerEntity.Instance.GetCurrentLevel;

        if (ConfigModule.GetConfig<LevelConfig>().TryGetLevelData(currentLevel, out var levelData))
        { 
            _board = Instantiate(board);
            _board.Init(this, offset, levelData);
        }

        _window = FindFirstObjectByType<ServingWindow>(); 
        _window.Init(this);

        _client = Instantiate(service);
        _client.Init(this, _haseDish);

        _board.OnLose += Lose; 

        UIModule.Inject(this, _board, _window, _client);

        _winConditions = new WinConditions(new[] 
        {
            WinCondition.TableClear, WinCondition.RemoveAllTiles
        });
    }

    public override void Close()
    {
        if (UIModule.TryGetCanvas<PlayCanvas>(out var playCanvas))
        {
            playCanvas.OpenPanel<PlayPanel>();
        }
    }

    public virtual void SetRemoveAllTiles()
    {
        _client.Finish();
        _window.Finish();
        _winConditions.SetCompleted(WinCondition.RemoveAllTiles, true);
    }

    public virtual void ForceTakeDish()
    {
        _client.ForceTakeDish();
    }

    public virtual void SetTableClear()
    {
        _winConditions.SetCompleted(WinCondition.TableClear, true);
    } 

    public virtual void AddValue(int value)
    {
        _resultValue += value;
    }

    public List<MadjongGenerator.TilePair> GetTilesInOrder()
    {
        return _board.GetTilesInOrder();
    }

    public List<Enums.TileType> GetAvaiablesTiles()
    {
        return _board.GetAvaiableTiles();
    }

    protected override void Start()
    {
        _camera = Camera.main;

        if (UIModule.OpenCanvas<PlayCanvas>(out var playCanvas))
        {
            playCanvas.OpenPanel<PlayPanel>();
        }

        InvokePlayStatusChanged(PlayStatus.play);

        _status = PlayStatus.play;
    }

    protected override void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null && hit.transform.TryGetComponent(out TileView tile))
            {
                HandleTileClick(tile);
            }
        }
    }

    protected virtual void OnDestroy()
    {
        _board.OnLose -= Lose; 
    }
     

    public virtual void Win()
    {
        if (PlayerEntity.Instance.IsSound) _audioSource.PlayOneShot(_audioWin);

        PlayerEntity.Instance.SetNextLevel();

        if (UIModule.TryGetCanvas<PlayCanvas>(out var playCanvas))
        {
            playCanvas.OpenPanel<WinPanel>(true).OpenWindow<WinWindow>();
        }

        InvokePlayStatusChanged(PlayStatus.win);
        _status = PlayStatus.win;

        AnalyticsHolder.Victory();
    }

    public virtual void Lose()
    {
        if (PlayerEntity.Instance.IsSound) _audioSource.PlayOneShot(_audioLose);
        if (UIModule.TryGetCanvas<PlayCanvas>(out var playCanvas))
        {
            playCanvas.OpenPanel<LosePanel>(true).OpenWindow<LoseWindow>();
        }

        InvokePlayStatusChanged(PlayStatus.lose);
       _status = PlayStatus.lose;

        AnalyticsHolder.Defeat();
    }

    protected virtual void HandleTileClick(TileView clickedTile)
    {
        if (_status != PlayStatus.play) return;

        if (!clickedTile.IsAvailable())
            return;

        if (!_window.IsFree)
        {
            if (_firstTile != null)
            {
                _firstTile.Deselect();
                _firstTile = null;
            } 
            return;
        }

        if (_firstTile == null)
        {
            _firstTile = clickedTile;
            _firstTile.Select();
            return;
        }

        if (_firstTile == clickedTile)
        {
            _firstTile.Deselect();
            _firstTile = null;
            return;
        }

        if (_firstTile.CompareType(clickedTile.TileType))
        {
            Vector3 joinPoint = (_firstTile.transform.position + clickedTile.transform.position) / 2f;

            _board.RemoveTiles(_firstTile, clickedTile, InvokeMergeEffect, x =>
            {  
                if (DishMapping.TryGetDish(clickedTile.TileType, out Enums.DishType type))
                {
                    var textureConfig = ConfigModule.GetConfig<TextureConfig>();

                    if (textureConfig.TryGetTextureData(type, out DishTextureData data))
                    {
                        var dish = new Dish(type, data.TextureDish);
                        _window.AddDish(x, dish);
                    }

                    _firstTile = null;
                }
            }); 
        }
        else
        {
            _firstTile.Deselect();
            _firstTile = clickedTile;
            _firstTile.Select();
        }
    }
    
    protected void InvokePlayStatusChanged(PlayStatus status)
    {
        PlayStatusChanged?.Invoke(status);
    }

    protected virtual void InvokeMergeEffect(Vector3 joinPoint)
    {
        var effect = Instantiate(mergeEffect, joinPoint, Quaternion.identity);

        StartCoroutine(WaitDestroy(effect));
    }


    protected virtual IEnumerator WaitDestroy(Transform destroyed)
    {
        yield return new WaitForSeconds(1f);

        Destroy(destroyed.gameObject);
    }

    public enum PlayStatus { play, pause, win, lose}
}
