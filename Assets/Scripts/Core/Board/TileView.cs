using DG.Tweening;
using UnityEngine;

public class TileView : MonoBehaviour
{
    public Vector2Int GridPos { get; set; }
    public int LayerIndex { get; set; } // <--- новый параметр
    public int TypeId { get; private set; }

    [SerializeField] SpriteRenderer face;
    private PlayState _state;

    public void Init(PlayState state)
    {
        _state = state;
        gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
    }

    public void SetType(int typeId, Sprite sprite)
    {
        TypeId = typeId;
        if (face) face.sprite = sprite;
    }

    public bool TrySelect()
    {
        Debug.Log($"Select me! Layer: {LayerIndex}");
        transform.DOScale(1.08f, 0.15f).SetLoops(2, LoopType.Yoyo);
        return true;
    }

    public void Select() { }
    public void Deselect()
    {
        Debug.Log($"Deselect me! Layer: {LayerIndex}");
        transform.DOScale(1f, 0.1f);
    }

    public Tween RemoveWithJoin(Vector3 to)
    {
        return transform.DOMove(to, 0.25f).OnComplete(() =>
        {
            transform.DOScale(0f, 0.2f).OnComplete(() => Destroy(gameObject));
        });
    }
}
