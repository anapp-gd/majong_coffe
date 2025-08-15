using UnityEngine;

public class PlayState : State
{
    [SerializeField] private Board board;
    private Board _board;
    private Camera _camera;

    private TileView _firstTile;

    protected override void Awake()
    {
        _board = Instantiate(board);
        _board.Init(this);

        UIModule.Inject(this, _board);
    }

    protected override void Start()
    {
        _camera = Camera.main;
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

    private void HandleTileClick(TileView clickedTile)
    {
        // Если первая плитка не выбрана
        if (_firstTile == null)
        {
            _firstTile = clickedTile;
            _firstTile.Select();
            return;
        }

        // Если нажали на ту же плитку — снимаем выбор
        if (_firstTile == clickedTile)
        {
            _firstTile.Deselect();
            _firstTile = null;
            return;
        }

        // Проверяем совпадение типа
        if (_firstTile.TypeId == clickedTile.TypeId)
        {
            Vector3 joinPoint = (_firstTile.transform.position + clickedTile.transform.position) / 2f;

            _firstTile.RemoveWithJoin(joinPoint);
            clickedTile.RemoveWithJoin(joinPoint);

            _firstTile = null;
        }
        else
        {
            // Типы не совпали — снимаем выбор
            _firstTile.Deselect();
            _firstTile = clickedTile;
            _firstTile.Select();
        }
    }
}
