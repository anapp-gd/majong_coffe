using UnityEngine;
using UnityEngine.SceneManagement;

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

    int stage;
    bool isTutorInfo;

    protected override void Awake()
    {
        isTutorInfo = true;
        _audioSource = gameObject.AddComponent<AudioSource>();

        stage = PlayerEntity.Instance.TutorialStage;

        _board = Instantiate(board);
        _board.Init(this, offset, levelData);

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

    public override void SetRemoveAllTiles()
    {
        switch (stage)
        {
            case 0:
                _client.Finish();
                _window.Finish();
                Win();
                break;
            case 1:
                _client.Finish();
                _window.Finish();
                Win();
                break;
            case 2:
                _client.Finish();
                _window.Finish();
                Win();
                break;
            default:
                PlayerEntity.Instance.TutorialStage = 3;
                PlayerEntity.Instance.TutorDone = true;
                PlayerEntity.Instance.Save();
                if (UIModule.OpenCanvas<LoadingCanvas>(out var loadingCanvas))
                {
                    loadingCanvas.OpenPanel<LoadingPanel>(false, () =>
                    {
                        SceneManager.LoadScene(1);
                    });
                }
                break;
        }
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
        stage++;

        int indexScene = 0;

        if (stage == 3)
        {
            PlayerEntity.Instance.TutorDone = true;
            AnalyticsHolder.TutorDone();
        }

        PlayerEntity.Instance.TutorialStage = stage;
        PlayerEntity.Instance.Save(); 

        InvokePlayStatusChanged(PlayStatus.win);
        _status = PlayStatus.win;

        _audioSource.PlayOneShot(_audioWin);

        AnalyticsHolder.Victory();

        if (stage == 1) indexScene = 4; 

        if (stage == 2) indexScene = 5;

        if (stage == 3)
        {
            if (UIModule.OpenCanvas<LoadingCanvas>(out var loadingCanvas))
            {
                loadingCanvas.OpenPanel<LoadingPanel>(false, () =>
                {
                    SceneManager.LoadScene(2);
                });
            }
        }
        else if (UIModule.OpenCanvas<LoadingCanvas>(out var loadingCanvas))
        {
            loadingCanvas.OpenPanel<LoadingPanel>(false, () =>
            {
                SceneManager.LoadScene(indexScene);
            });
        }
    }

    public override void Lose()
    {
        InvokePlayStatusChanged(PlayStatus.lose);
        _status = PlayStatus.lose;

        _audioSource.PlayOneShot(_audioLose);

        AnalyticsHolder.Defeat();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    protected override void HandleTileClick(TileView clickedTile)
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

        if (isTutorInfo)
        {
            FindFirstObjectByType<TutorInfo>().gameObject.SetActive(false);
            isTutorInfo = false;
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
            InProgress = true;

            Vector3 joinPoint = (_firstTile.transform.position + clickedTile.transform.position) / 2f;

            _board.InvokeMergeEvent(_firstTile, clickedTile, InvokeMergeEffect, InvokeDish);

            _firstTile?.Deselect();
            _firstTile = null;
        }
        else
        {
            _firstTile.Deselect();
            _firstTile = clickedTile;
            _firstTile.Select();
        }
    }

    void InvokeDish(Enums.TileType tileType, Vector3 mergePos)
    {
        if (DishMapping.TryGetDish(tileType, out Enums.DishType type))
        {
            var textureConfig = ConfigModule.GetConfig<TextureConfig>();

            if (textureConfig.TryGetTextureData(type, out DishTextureData data))
            {
                var dish = new Dish(type, data.TextureDish);
                _window.AddDish(mergePos, dish);
            }
        }

        InProgress = false;
    }
}
