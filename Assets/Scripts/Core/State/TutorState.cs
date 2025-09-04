using UnityEngine;

public class TutorState : PlayState
{
    [SerializeField] private  LevelData levelData;
     
    public static new TutorState Instance
    {
        get
        {
            return (TutorState)State.Instance;
        }
    } 

    protected override void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
         
        _board = Instantiate(board);
        _board.Init(this, offset, levelData);
         
        _window = Instantiate(window);
        _window.Init(this);

        _client = Instantiate(client);
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

    public override void SetRemoveAllTiles()
    {
        _winConditions.SetCompleted(WinCondition.RemoveAllTiles, true);
    }

    public override void SetTableClear()
    {
        _winConditions.SetCompleted(WinCondition.TableClear, true);
    }

    public override void AddValue(int value)
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

    public override void Win()
    {
        PlayerEntity.Instance.TutorDone = true;

        PlayerEntity.Instance.Save();

        if (UIModule.TryGetCanvas<PlayCanvas>(out var playCanvas))
        {
            playCanvas.OpenPanel<WinPanel>(true).OpenWindow<WinWindow>();
        }

        InvokePlayStatusChanged(PlayStatus.win);
        _status = PlayStatus.win;

        _audioSource.PlayOneShot(_audioWin);
    }

    public override void Lose()
    {
        if (UIModule.TryGetCanvas<PlayCanvas>(out var playCanvas))
        {
            playCanvas.OpenPanel<LosePanel>(true).OpenWindow<LoseWindow>();
        }

        InvokePlayStatusChanged(PlayStatus.lose);
        _status = PlayStatus.lose;

        _audioSource.PlayOneShot(_audioLose);
    }

    protected override void HandleTileClick(TileView clickedTile)
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
}
