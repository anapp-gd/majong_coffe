using UnityEngine;

public class PlayState : State
{
    [SerializeField] private Board board;
    private Tile selectedTile;

    protected override void Awake()
    {

    }

    protected override void Start()
    {

    }

    public void OnTileClicked(Tile tile)
    {
        if (!tile.IsFree) return;

        if (selectedTile == null)
        { 
            selectedTile = tile;
            tile.Select();
        }
        else if (selectedTile == tile)
        {
            selectedTile.Deselect();
            selectedTile = null;
        }
        else
        {
            if (selectedTile.SymbolId == tile.SymbolId)
            {
                board.RemovePair(selectedTile, tile);
                selectedTile = null;
            }
            else
            {
                selectedTile.Deselect();
                selectedTile = tile;
                tile.Select();
            }
        }
    }
}
