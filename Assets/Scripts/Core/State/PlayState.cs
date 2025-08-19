using System.Collections.Generic;
using UnityEngine;

public class PlayState : State
{
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

    protected override void Awake()
    {
        _board = Instantiate(board);
        _board.Init(this, offset);

        _window = Instantiate(window);
        _window.Init(this);

        _client = Instantiate(client);
        _client.Init(this, _haseDish);

        _board.OnWin += Win;
        _board.OnLose += Lose;

        UIModule.Inject(this, _board, _window, _client);
    }

    protected override void Start()
    {
        _camera = Camera.main;

        if (UIModule.OpenCanvas<PlayCanvas>(out var playCanvas))
        {
            playCanvas.OpenPanel<PlayPanel>();
        }
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
        _board.OnWin -= Win;
    }

    public void Win()
    {
        if (UIModule.TryGetCanvas<PlayCanvas>(out var playCanvas))
        {
            playCanvas.OpenPanel<WinPanel>();
        }
    }

    public void Lose()
    {
        if (UIModule.TryGetCanvas<PlayCanvas>(out var playCanvas))
        {
            playCanvas.OpenPanel<LosePanel>();
        }
    }

    private void HandleTileClick(TileView clickedTile)
    {
        if (!clickedTile.IsAvailable(_board.GetTilesOnLayer(_board.CurrentLayer), _board.CurrentLayer))
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
            if (DishMapping.TryGetDish(clickedTile.TileType, out Enums.DishType type))
            {
                var textureConfig = ConfigModule.GetConfig<TextureConfig>();

                if (textureConfig.TryGetTextureData(type, out DishTextureData data))
                {
                    var dish = new Dish(type, data.TextureDish); 
                    _window.AddDish(dish); 
                }
            }

            _board.RemoveTiles(_firstTile, clickedTile);
            _firstTile = null;
        }
        else
        {
            _firstTile.Deselect();
            _firstTile = clickedTile;
            _firstTile.Select();
        }
    }

}
