using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class TileView : MonoBehaviour, IPointerClickHandler
{
    public Vector3Int GridPos { get; set; }
    public int TypeId { get; private set; }

    [SerializeField] SpriteRenderer face;

    public void Init()
    {

    }

    public void SetType(int typeId, Sprite sprite)
    {
        TypeId = typeId;
        if (face) face.sprite = sprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Нажата плитка {TypeId} на позиции {GridPos}");
        Select();
    }

    public void Select() => transform.DOScale(1.06f, 0.15f).SetLoops(2, LoopType.Yoyo);
    public void Deselect() => transform.DOScale(1f, 0.1f);

    public Tween RemoveWithJoin(Vector3 to)
    {
        return transform.DOMove(to, 0.25f).OnComplete(() =>
        {
            transform.DOScale(0f, 0.2f).OnComplete(() => Destroy(gameObject));
        });
    }
}
