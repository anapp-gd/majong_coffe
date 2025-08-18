using UnityEngine;

public class PlayState : State
{
    [SerializeField] private Board board;

    private Board _board;
    private ServingWindow _window;
    private Camera _camera;

    private TileView _firstTile;

    protected override void Awake()
    {
        _board = Instantiate(board);
        _board.Init(this);

        _board.OnWin += Win;
        _board.OnLose += Lose;

        UIModule.Inject(this, _board);
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
        // Кликать можно только доступные тайлы
        if (!clickedTile.IsAvailable(_board.GetTilesOnLayer(_board.CurrentLayer), _board.CurrentLayer))
            return;

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

                if ()
                {

                }
            }



            var dish = new Dish(clickedTile.TileType, clickedTile);

            _window.AddDish();

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
