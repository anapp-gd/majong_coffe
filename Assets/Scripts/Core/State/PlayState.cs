using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

public class PlayState : State
{
    [SerializeField] protected AudioClip _audioWin;
    [SerializeField] protected AudioClip _audioLose;

    public bool InProgress = false;
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
        AnalyticsHolder.LevelStart(PlayerEntity.Instance.GetCurrentLevel);

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
        _window.Finish();

        _client.Finish(()=>
        {
            _winConditions.SetCompleted(WinCondition.RemoveAllTiles, true);
        });
    }

    public virtual void ForceTakeDish(Action callback)
    {
        _client.ForceTakeDish(callback);
    }

    public virtual void SetTableValue(bool value)
    {
        _winConditions.SetCompleted(WinCondition.TableClear, value);
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
        AnalyticsHolder.LevelFinish(PlayerEntity.Instance.GetCurrentLevel);

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
        AnalyticsHolder.LevelFinish(PlayerEntity.Instance.GetCurrentLevel);

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
        if (InProgress) return;
        if (!clickedTile.IsAvailable()) return;

        if (!_window.IsFree)
        {
            if (_firstTile != null)
            {
                _firstTile?.Deselect();
                _firstTile = null;
            }
            return;
        }

        if (_firstTile == null)
        {
            _firstTile = clickedTile;
            _firstTile?.Select();
            return;
        }

        if (_firstTile == clickedTile)
        {
            _firstTile?.Deselect();
            _firstTile = null;
            return;
        }

        if (_firstTile.CompareType(clickedTile.TileType))
        { 
            StartCoroutine(MergeAndCreateDish(_firstTile, clickedTile));

            _firstTile?.Deselect();
            _firstTile = null;
        }
        else
        {
            _firstTile?.Deselect();
            _firstTile = clickedTile;
            _firstTile?.Select();
        }
    }

    private IEnumerator MergeAndCreateDish(TileView a, TileView b)
    {
        SwitchProgress();

        // Точка объединения
        Vector3 joinPoint = (a.transform.position + b.transform.position) / 2f;

        // Запускаем анимацию слияния плиток
        var seq = _board.InvokeMergeEvent(a, b, InvokeMergeEffect);

        if (seq != null) yield return seq.WaitForCompletion();

        SwitchProgress();

        // Создаём блюдо после завершения анимации
        if (DishMapping.TryGetDish(a.TileType, out Enums.DishType type))
        {
            var textureConfig = ConfigModule.GetConfig<TextureConfig>();
            if (textureConfig.TryGetTextureData(type, out DishTextureData data))
            {
                var dish = new Dish(type, data.TextureDish);

                // Ставим блюдо в очередь и ждём, пока оно будет добавлено на стол
                bool completed = false;
                _window.EnqueueDish(dish, joinPoint, () => completed = true);

                yield return new WaitUntil(() => completed);
            }
        }
    }

    void SwitchProgress() => InProgress = !InProgress;

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
