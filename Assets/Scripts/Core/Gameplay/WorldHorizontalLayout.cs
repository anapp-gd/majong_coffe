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

    private List<Transform> children = new List<Transform>();

    // --- Callbacks ---
    public event Action<Transform> OnAddStart;
    public event Action<Transform> OnAddComplete;
    public event Action<Transform> OnRemoveStart;
    public event Action<Transform> OnRemoveComplete;
    public event Action OnRearrangeComplete;

    private void Update()
    {
        if (!Application.isPlaying)
            UpdateLayoutEditor();
    }

    private void GatherChildren()
    {
        children.Clear();
        foreach (Transform t in transform)
            children.Add(t);
    }

    private Bounds GetParentBounds()
    {
        var parentRenderer = GetComponent<SpriteRenderer>();
        return parentRenderer != null ? parentRenderer.bounds : new Bounds(Vector3.zero, Vector3.zero);
    }

    // -------------------- ADD --------------------
    public bool AddObject(Transform obj)
    {
        if (obj == null) return false;

        GatherChildren();
        if (children.Count >= maxCount)
            return false; // очередь полная

        obj.SetParent(transform);
        obj.localScale = Vector3.zero;

        Bounds parentBounds = GetParentBounds();

        // Начальная позиция для анимации (от края)
        Vector3 startPos = Vector3.zero;
        switch (startEdge)
        {
            case StartEdge.Left:
                startPos = new Vector3(parentBounds.min.x - spacing, 0f, 0f);
                break;
            case StartEdge.Right:
                startPos = new Vector3(parentBounds.max.x + spacing, 0f, 0f);
                break;
            case StartEdge.Center:
                startPos = Vector3.zero;
                break;
        }
        obj.localPosition = startPos;

        OnAddStart?.Invoke(obj);

        // Появление и перестановка очереди
        Sequence seq = DOTween.Sequence();
        seq.Append(obj.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
        seq.AppendCallback(() => UpdateLayoutAnimated());
        seq.OnComplete(() => OnAddComplete?.Invoke(obj));

        return true;
    }

    // -------------------- REMOVE --------------------
    public void RemoveObject(Transform obj)
    {
        if (obj == null) return;

        GatherChildren();
        if (!children.Contains(obj)) return;

        OnRemoveStart?.Invoke(obj);

        // Анимация исчезновения
        obj.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
        {
            children.Remove(obj);
            Destroy(obj.gameObject);
            UpdateLayoutAnimated();
            OnRemoveComplete?.Invoke(obj);
        });
    }

    // -------------------- UPDATE LAYOUT --------------------
    private void UpdateLayoutAnimated()
    {
        GatherChildren();
        if (children.Count == 0)
        {
            OnRearrangeComplete?.Invoke();
            return;
        }

        Bounds parentBounds = GetParentBounds();
        float currentSpacing = spacing;
        float totalWidth = (children.Count - 1) * currentSpacing;

        if (fitToParent && children.Count > 1)
        {
            float parentWidth = parentBounds.size.x - 2 * edgeOffset;
            if (totalWidth > parentWidth)
            {
                currentSpacing = parentWidth / (children.Count - 1);
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
        for (int i = 0; i < children.Count; i++)
        {
            Vector3 target = startPos + Vector3.right * (i * currentSpacing);
            seq.Join(children[i].DOLocalMove(target, 0.5f).SetEase(Ease.OutCubic));
        }
        seq.OnComplete(() => OnRearrangeComplete?.Invoke());
    }

    // -------------------- EDITOR --------------------
    private void UpdateLayoutEditor()
    {
        GatherChildren();
        if (children.Count == 0) return;

        Bounds parentBounds = GetParentBounds();
        float currentSpacing = spacing;
        float totalWidth = (children.Count - 1) * currentSpacing;

        if (fitToParent && children.Count > 1)
        {
            float parentWidth = parentBounds.size.x - 2 * edgeOffset;
            if (totalWidth > parentWidth)
            {
                currentSpacing = parentWidth / (children.Count - 1);
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

        for (int i = 0; i < children.Count; i++)
        {
            children[i].localPosition = startPos + Vector3.right * (i * currentSpacing);
            children[i].localScale = Vector3.one;
        }
    }

    // -------------------- CLEAR --------------------
    public void ClearQueue()
    {
        GatherChildren();
        for (int i = children.Count - 1; i >= 0; i--)
        {
            RemoveObject(children[i]);
        }
    }
}
