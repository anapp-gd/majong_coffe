using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[ExecuteAlways]
public class HorizontalQueue2D : MonoBehaviour
{
    public enum StartEdge { Left, Center, Right }
    public StartEdge startEdge = StartEdge.Center;

    [SerializeField] private float spacing = 1f;

    private List<Transform> children = new List<Transform>();

    private void Update()
    {
        // Только в Editor для визуализации
        if (!Application.isPlaying)
        {
            UpdateLayoutEditor();
        }
    }

    private void GatherChildren()
    {
        children.Clear();
        foreach (Transform t in transform)
        {
            children.Add(t);
        }
    }

    private Bounds GetParentBounds()
    {
        GatherChildren();
        if (children.Count == 0)
            return new Bounds(Vector3.zero, Vector3.zero);

        Bounds bounds = new Bounds(children[0].localPosition, Vector3.zero);
        foreach (var child in children)
        {
            bounds.Encapsulate(child.localPosition);
        }
        return bounds;
    }

    public void AddObject(Transform obj)
    {
        obj.SetParent(transform);
        obj.localScale = Vector3.zero;

        GatherChildren();

        Bounds parentBounds = GetParentBounds();

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

        // Анимация появления и выравнивания
        Sequence seq = DOTween.Sequence();
        seq.Append(obj.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
        seq.AppendCallback(() =>
        {
            UpdateLayoutAnimated();
        });
    }

    private void UpdateLayoutAnimated()
    {
        GatherChildren();
        float totalWidth = (children.Count - 1) * spacing;
        Vector3 leftStart = Vector3.zero;

        switch (startEdge)
        {
            case StartEdge.Left:
                leftStart = Vector3.zero;
                break;
            case StartEdge.Center:
                leftStart = new Vector3(-totalWidth / 2f, 0f, 0f);
                break;
            case StartEdge.Right:
                leftStart = new Vector3(-totalWidth, 0f, 0f);
                break;
        }

        for (int i = 0; i < children.Count; i++)
        {
            Vector3 target = leftStart + Vector3.right * (i * spacing);
            children[i].DOLocalMove(target, 0.5f).SetEase(Ease.OutCubic);
        }
    }

    [SerializeField] private float edgeOffset = 0.5f; // смещение от края 
    private void UpdateLayoutEditor()
    {
        GatherChildren();

        if (children.Count == 0) return;

        // Получаем bounds родителя
        var parentRenderer = GetComponent<SpriteRenderer>();
        Bounds parentBounds = parentRenderer != null ? parentRenderer.bounds : new Bounds(Vector3.zero, Vector3.zero);

        float totalWidth = (children.Count - 1) * spacing;
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
            children[i].localPosition = startPos + Vector3.right * (i * spacing);
            children[i].localScale = Vector3.one;
        }
    }
}
