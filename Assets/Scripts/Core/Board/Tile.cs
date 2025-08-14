using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int SymbolId { get; private set; }
    public bool IsFree { get; private set; }
    public int Layer { get; private set; }

    private SpriteRenderer spriteRenderer;
    private Board _board;
    private PlayState _state;

    public void Init(int symbolId, int layer, Board board, PlayState state)
    {
        SymbolId = symbolId;
        Layer = layer;
        this._board = board;
        this._state = state;

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Random.ColorHSV();
    }

    public void CheckIfFree(List<Tile> allTiles)
    {
        bool hasAbove = allTiles.Any(t => t.Layer > Layer && Vector2.Distance(t.transform.position, transform.position) < 0.6f);
        bool leftBlocked = allTiles.Any(t => t.Layer == Layer && Mathf.Abs(t.transform.position.y - transform.position.y) < 0.1f && t.transform.position.x < transform.position.x && Mathf.Abs(t.transform.position.x - transform.position.x) < 1.1f);
        bool rightBlocked = allTiles.Any(t => t.Layer == Layer && Mathf.Abs(t.transform.position.y - transform.position.y) < 0.1f && t.transform.position.x > transform.position.x && Mathf.Abs(t.transform.position.x - transform.position.x) < 1.1f);

        IsFree = !hasAbove && (!leftBlocked || !rightBlocked);
        spriteRenderer.color = IsFree ? spriteRenderer.color : Color.gray; // подсветка
    }

    private void OnMouseUpAsButton()
    {
        _state.OnTileClicked(this);
    }

    public void Select()
    {
        transform.DOScale(1.1f, 0.2f).SetLoops(2, LoopType.Yoyo);
    }

    public void Deselect()
    {
        transform.DOScale(1f, 0.2f);
    }

    public void AnimateRemove()
    {
        transform.DOMove(transform.position + Vector3.up * 0.5f, 0.2f)
                 .OnComplete(() => transform.DOScale(0, 0.3f).OnComplete(() => Destroy(gameObject)));
    }
}
