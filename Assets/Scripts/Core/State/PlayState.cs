using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayState : State
{
    public event Action<PlayStatus> PlayStatusChanged;
    private PlayStatus _status;

    public static new PlayState Instance
    {
        get
        {
            return (PlayState)State.Instance;
        }
    }

    [SerializeField] private Transform mergeEffect;
    [SerializeField] private Vector2 offset; 
    [SerializeField] private Board board;
    public ServingWindow ServingWindow
    {
        get => _window; 
    }
    [SerializeField] private ServingWindow window;
    public ClientService ClientService
    {
        get => _client;
    }
    [SerializeField] private ClientService client;
    
    private Board _board;
    private ServingWindow _window;
    private ClientService _client;
    private Camera _camera;

    private TileView _firstTile;

    private HashSet<Enums.DishType> _haseDish;
    public HashSet<Enums.DishType> SetHashDishes 
    { 
        set
        {
            _haseDish = value;
        }
    }

    public event Action BoardCleanChange;

    public int GetResaultValue => _resultValue;
    private int _resultValue;
    private WinConditions _winConditions;
    public bool IsWin => _winConditions.IsVictory();

    protected override void Awake()
    {
        _board = Instantiate(board);
        _board.Init(this, offset);

        _window = Instantiate(window);
        _window.Init(this);

        _client = Instantiate(client);
        _client.Init(this, _haseDish);

        _board.OnLose += Lose;

        _window.OnServingUpdate += OnServingWindowUpdate;

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

    public void SetRemoveAllTiles()
    {
        _winConditions.SetCompleted(WinCondition.RemoveAllTiles, true);
    }

    public void SetTableClear()
    {
        _winConditions.SetCompleted(WinCondition.TableClear, true);
    } 

    public void AddValue(int value)
    {
        _resultValue += value;
    }

    protected override void Start()
    {
        _camera = Camera.main;

        if (UIModule.OpenCanvas<PlayCanvas>(out var playCanvas))
        {
            playCanvas.OpenPanel<PlayPanel>();
        }

        PlayStatusChanged?.Invoke(PlayStatus.play);

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

    private void OnDestroy()
    {
        _board.OnLose -= Lose; 
    }

    void OnServingWindowUpdate(List<Dish> currentDishes)
    {
        if (IsWin) Win(); 
    }

    public void Win()
    {
        PlayerEntity.Instance.SetNextLevel();

        if (UIModule.TryGetCanvas<PlayCanvas>(out var playCanvas))
        {
            playCanvas.OpenPanel<WinPanel>().OpenWindow<WinWindow>();
        }

        PlayStatusChanged?.Invoke(PlayStatus.win);
        _status = PlayStatus.win;
    }

    public void Lose()
    {
        if (UIModule.TryGetCanvas<PlayCanvas>(out var playCanvas))
        {
            playCanvas.OpenPanel<LosePanel>().OpenWindow<LoseWindow>();
        }
         
        PlayStatusChanged?.Invoke(PlayStatus.lose);
        _status = PlayStatus.lose;
        
    }

    private void HandleTileClick(TileView clickedTile)
    {
        if (_status != PlayStatus.play) return;

        if (!clickedTile.IsAvailable())
            return;

        if (_window.IsFull())
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

    void InvokeMergeEffect(Vector3 joinPoint)
    {
        var effect = Instantiate(mergeEffect, joinPoint, Quaternion.identity);

        StartCoroutine(WaitDestroy(effect));
    }


    IEnumerator WaitDestroy(Transform destroyed)
    {
        yield return new WaitForSeconds(1f);

        Destroy(destroyed.gameObject);
    }

    public enum PlayStatus { play, pause, win, lose}
}
