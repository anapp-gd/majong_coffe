using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class WorldHorizontalLayout : MonoBehaviour
{
    public enum StartEdge { Left, Center, Right }
    public StartEdge startEdge = StartEdge.Center;

    [SerializeField] private int maxCount = 5;
    [SerializeField] private float spacing = 1f;
    [SerializeField] private float edgeOffset = 0.5f;
    [SerializeField] private bool fitToParent = true;

    private Dictionary<Dish, Transform> values = new Dictionary<Dish, Transform>();

    // --- Callbacks ---
    public event Action<Dish, Transform> OnAddStart;
    public event Action<Dish, Transform> OnAddComplete;
    public event Action<Dish, Transform> OnRemoveStart;
    public event Action<Dish, Transform> OnRemoveComplete;
    public event Action OnRearrangeComplete;

    private void Update()
    {
        if (!Application.isPlaying)
            UpdateLayoutEditor();
    }

    public int GetSlotCount() => values.Count;

    private Bounds GetParentBounds()
    {
        var parentRenderer = GetComponent<SpriteRenderer>();
        return parentRenderer != null ? parentRenderer.bounds : new Bounds(Vector3.zero, Vector3.zero);
    }

    // -------------------- ADD --------------------
    public bool AddObject(Dish dish, Transform obj, Action addCallback)
    {
        if (dish == null || obj == null) return false;
        if (values.Count >= maxCount) return false; // очередь полная
        if (values.ContainsKey(dish)) return false; // уже добавлен

        obj.SetParent(transform);
        obj.localScale = Vector3.zero;

        values.Add(dish, obj);
        OnAddStart?.Invoke(dish, obj);

        // Анимация появления + перестройка
        Sequence seq = DOTween.Sequence();
        seq.Append(obj.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
        seq.AppendCallback(UpdateLayoutAnimated);
        seq.OnComplete(() => addCallback());

        return true;
    }

    // -------------------- REMOVE --------------------
    public void RemoveObject(Dish dish)
    {
        if (dish == null || !values.ContainsKey(dish)) return;

        Transform obj = values[dish];
        OnRemoveStart?.Invoke(dish, obj);

        // Анимация исчезновения
        obj.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
        {
            values.Remove(dish);
            Destroy(obj.gameObject);
            UpdateLayoutAnimated();
            OnRemoveComplete?.Invoke(dish, obj);
        });
    }

    // -------------------- UPDATE LAYOUT --------------------
    private void UpdateLayoutAnimated()
    {
        if (values.Count == 0)
        {
            OnRearrangeComplete?.Invoke();
            return;
        }

        Bounds parentBounds = GetParentBounds();
        float currentSpacing = spacing;
        float totalWidth = (values.Count - 1) * currentSpacing;

        if (fitToParent && values.Count > 1)
        {
            float parentWidth = parentBounds.size.x - 2 * edgeOffset;
            if (totalWidth > parentWidth)
            {
                currentSpacing = parentWidth / (values.Count - 1);
                totalWidth = parentWidth;
            }
        }

        Vector3 startPos = Vector3.zero;
        switch (startEdge)
        {
            case StartEdge.Left:
                startPos = new Vector3(parentBounds.min.x + edgeOffset, 0f, 0f);
                break;
            case StartEdge.Center:
                startPos = new Vector3(parentBounds.center.x - totalWidth / 2f, 0f, 0f);
                break;
            case StartEdge.Right:
                startPos = new Vector3(parentBounds.max.x - totalWidth - edgeOffset, 0f, 0f);
                break;
        }

        Sequence seq = DOTween.Sequence();
        int index = 0;
        foreach (var kv in values)
        {
            Vector3 target = startPos + Vector3.right * (index * currentSpacing);
            seq.Join(kv.Value.DOLocalMove(target, 0.5f).SetEase(Ease.OutCubic));
            index++;
        }
        seq.OnComplete(() => OnRearrangeComplete?.Invoke());
    }

    // -------------------- EDITOR --------------------
    private void UpdateLayoutEditor()
    {
        if (values.Count == 0) return;

        Bounds parentBounds = GetParentBounds();
        float currentSpacing = spacing;
        float totalWidth = (values.Count - 1) * currentSpacing;

        if (fitToParent && values.Count > 1)
        {
            float parentWidth = parentBounds.size.x - 2 * edgeOffset;
            if (totalWidth > parentWidth)
            {
                currentSpacing = parentWidth / (values.Count - 1);
                totalWidth = parentWidth;
            }
        }

        Vector3 startPos = Vector3.zero;
        switch (startEdge)
        {
            case StartEdge.Left:
                startPos = new Vector3(parentBounds.min.x + edgeOffset, 0f, 0f);
                break;
            case StartEdge.Center:
                startPos = new Vector3(parentBounds.center.x - totalWidth / 2f, 0f, 0f);
                break;
            case StartEdge.Right:
                startPos = new Vector3(parentBounds.max.x - totalWidth - edgeOffset, 0f, 0f);
                break;
        }

        int index = 0;
        foreach (var kv in values)
        {
            kv.Value.localPosition = startPos + Vector3.right * (index * currentSpacing);
            kv.Value.localScale = Vector3.one;
            index++;
        }
    }

    // -------------------- CLEAR --------------------
    public void ClearQueue()
    {
        foreach (var kv in new Dictionary<Dish, Transform>(values))
        {
            RemoveObject(kv.Key);
        }
    }

    // -------------------- NEXT SLOT --------------------
    public Vector3 GetNextSlot(bool inWorldSpace = true)
    {
        int futureIndex = values.Count; // позиция нового элемента

        Bounds parentBounds = GetParentBounds();
        float currentSpacing = spacing;
        float totalWidth = (futureIndex) * currentSpacing;

        if (fitToParent && futureIndex > 1)
        {
            float parentWidth = parentBounds.size.x - 2 * edgeOffset;
            if (totalWidth > parentWidth)
            {
                currentSpacing = parentWidth / (futureIndex);
                totalWidth = parentWidth;
            }
        }

        Vector3 startPos = Vector3.zero;
        switch (startEdge)
        {
            case StartEdge.Left:
                startPos = new Vector3(parentBounds.min.x + edgeOffset, 0f, 0f);
                break;
            case StartEdge.Center:
                startPos = new Vector3(parentBounds.center.x - totalWidth / 2f, 0f, 0f);
                break;
            case StartEdge.Right:
                startPos = new Vector3(parentBounds.max.x - totalWidth - edgeOffset, 0f, 0f);
                break;
        }

        Vector3 localTarget = startPos + Vector3.right * (futureIndex * currentSpacing);
        return inWorldSpace ? transform.TransformPoint(localTarget) : localTarget;
    }

    // -------------------- SLOT BY DISH --------------------
    public Vector3 GetSlot(Dish dish, bool inWorldSpace = true)
    {
        if (!values.ContainsKey(dish))
            return transform.position;

        List<Dish> ordered = new List<Dish>(values.Keys);
        int index = ordered.IndexOf(dish);

        if (index < 0) return transform.position;

        Bounds parentBounds = GetParentBounds();
        float currentSpacing = spacing;
        float totalWidth = (values.Count - 1) * currentSpacing;

        if (fitToParent && values.Count > 1)
        {
            float parentWidth = parentBounds.size.x - 2 * edgeOffset;
            if (totalWidth > parentWidth)
            {
                currentSpacing = parentWidth / (values.Count - 1);
                totalWidth = parentWidth;
            }
        }

        Vector3 startPos = Vector3.zero;
        switch (startEdge)
        {
            case StartEdge.Left:
                startPos = new Vector3(parentBounds.min.x + edgeOffset, 0f, 0f);
                break;
            case StartEdge.Center:
                startPos = new Vector3(parentBounds.center.x - totalWidth / 2f, 0f, 0f);
                break;
            case StartEdge.Right:
                startPos = new Vector3(parentBounds.max.x - totalWidth - edgeOffset, 0f, 0f);
                break;
        }

        Vector3 localTarget = startPos + Vector3.right * (index * currentSpacing);
        return inWorldSpace ? transform.TransformPoint(localTarget) : localTarget;
    }
}
