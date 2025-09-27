using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private List<DishSlot> _slots = new();
    private Dictionary<Dish, DishSlot> _occupied = new();

    // --- Callbacks ---
    public event Action<Dish, DishSlot> OnAddStart;
    public event Action<Dish, DishSlot> OnAddComplete;
    public event Action<Dish, DishSlot> OnRemoveStart;
    public event Action<Dish, DishSlot> OnRemoveComplete;
    //public event Action OnRearrangeComplete;

    public bool IsBusy;

    private void Awake()
    {
        InitSlots();
    }

    private void Update()
    {
        if (!Application.isPlaying)
            UpdateLayoutEditor();
    }

    private void InitSlots()
    { 
        foreach (var slot in _slots)
        {
            slot.Clear(); 
        }
    }

    /*public void AddObject(Dish dish, Transform obj, Action addCallback)
    {
        if (dish == null || obj == null) return;
        if (_occupied.Count >= maxCount) return;
        if (_occupied.ContainsKey(dish)) return;

        // Берём первый свободный слот из очереди
        DishSlot freeSlot = _slots.FirstOrDefault(x => x.IsFree);
         
        if (freeSlot == null) return;

        freeSlot.Assign(dish, obj);
        _occupied.Add(dish, freeSlot);

        OnAddStart?.Invoke(dish, freeSlot);

        // Перестройка и анимация
        Sequence seq = DOTween.Sequence();
        seq.Append(obj.DOScale(Vector3.one, 0.12f).SetEase(Ease.OutBack));

        seq.AppendCallback(UpdateLayoutAnimated);

        seq.OnComplete(() =>
        {
            addCallback?.Invoke();
            OnAddComplete?.Invoke(dish, freeSlot);
        }); 
    }*/

    /*public IEnumerator AddObjectRoutine(Dish dish, Transform obj, Action addCallback)
    {
        if (dish == null || obj == null) yield break;
        if (_occupied.Count >= maxCount) yield break;
        if (_occupied.ContainsKey(dish)) yield break;

        DishSlot freeSlot = _slots.FirstOrDefault(x => x.IsFree);
        if (freeSlot == null) yield break;

        freeSlot.Assign(dish, obj);
        _occupied.Add(dish, freeSlot);

        OnAddStart?.Invoke(dish, freeSlot);

        // масштабируем объект
        yield return obj.DOScale(Vector3.one, 0.12f).SetEase(Ease.OutBack).WaitForCompletion();

        // запускаем перестройку
        Sequence layoutSeq = UpdateLayoutAnimated();

        if (layoutSeq != null) yield return layoutSeq.WaitForCompletion();

        // всё закончилось — можно вызывать коллбэки
        addCallback?.Invoke();
        OnAddComplete?.Invoke(dish, freeSlot);
    }

    public IEnumerator RemoveObjectRoutine(Dish dish)
    {
        if (dish == null || !_occupied.ContainsKey(dish)) yield break;
         
        var slot = _occupied[dish];

        var obj = slot.GetDishObject();

        OnRemoveStart?.Invoke(dish, slot);

        Sequence seq = DOTween.Sequence();

        if (obj != null)
        {
            seq.Append(obj.DOScale(Vector3.zero, 0.12f).SetEase(Ease.InBack));
            seq.AppendCallback(() =>
            {
                Destroy(obj.gameObject);
                _occupied.Remove(dish);
            });
        }
        else
        {
            seq.AppendCallback(() => _occupied.Remove(dish));
        }

        // Добавляем анимацию смещения прямо в цепочку
        seq.Append(UpdateLayoutAnimated());

        yield return seq.WaitForCompletion();

        slot.Release();
        OnRemoveComplete?.Invoke(dish, slot);
    }*/

    public IEnumerator AddObjectRoutine(Dish dish, Transform obj, Action addCallback)
    {
        if (dish == null || obj == null) yield break;
        if (_occupied.Count >= maxCount) yield break;
        if (_occupied.ContainsKey(dish)) yield break;

        DishSlot freeSlot = _slots.FirstOrDefault(x => x.IsFree);

        if (freeSlot == null) yield break;

        IsBusy = true;

        freeSlot.Assign(dish, obj);
        _occupied.Add(dish, freeSlot);

        OnAddStart?.Invoke(dish, freeSlot);
         
        // масштабируем объект
        yield return obj.DOScale(Vector3.one, 0.12f).SetEase(Ease.OutBack).WaitForCompletion();

        /*// запускаем перестройку с ожиданием
        yield return StartCoroutine(UpdateLayoutAnimated());
        // всё закончилось — вызываем коллбэки*/
        addCallback?.Invoke();

        IsBusy = false;

        OnAddComplete?.Invoke(dish, freeSlot);
    }
    /*
        public IEnumerator RemoveObjectRoutine(Dish dish)
        {
            if (dish == null || !_occupied.ContainsKey(dish)) yield break;

            var slot = _occupied[dish];
            var obj = slot.GetDishObject();

            OnRemoveStart?.Invoke(dish, slot);

            // анимация удаления объекта
            if (obj != null)
            {
                yield return obj.DOScale(Vector3.zero, 0.12f).SetEase(Ease.InBack).WaitForCompletion();
                Destroy(obj.gameObject);
            }

            _occupied.Remove(dish);

            // перестройка слотов
            yield return StartCoroutine(UpdateLayoutAnimatedRoutine());

            slot.Release();
            OnRemoveComplete?.Invoke(dish, slot);
        }
    */

    public IEnumerator RemoveObjectRoutine(Dish dish)
    {
        if (dish == null || !_occupied.ContainsKey(dish)) yield break;

        var slot = _occupied[dish];
        var obj = slot.GetDishObject();

        OnRemoveStart?.Invoke(dish, slot);

        IsBusy = false;

        // Шаг 1: анимация уменьшения объекта
        if (obj != null)
        {
            yield return obj.DOScale(Vector3.zero, 0.12f).SetEase(Ease.InBack).WaitForCompletion();

            Destroy(obj.gameObject);
        }

        _occupied.Remove(dish);
        slot.Release();

        _slots.Remove(slot);
        _slots.Add(slot);

        yield return StartCoroutine(UpdateLayoutAnimated());

        IsBusy = false;

        OnRemoveComplete?.Invoke(dish, slot);
    }
    IEnumerator UpdateLayoutAnimated()
    {
        if (_slots.Count == 0) yield break;

        float currentSpacing = spacing;
        float totalWidth = (_slots.Count - 1) * currentSpacing;

        // Получаем границы родителя через BoxCollider2D
        Bounds parentBounds = GetParentBounds();
        if (fitToParent && _slots.Count > 1)
        {
            float parentWidth = parentBounds.size.x - 2 * edgeOffset;
            if (totalWidth > parentWidth)
            {
                currentSpacing = parentWidth / (_slots.Count - 1);
                totalWidth = parentWidth;
            }
        }

        float yPos = _slots.Count > 0 ? _slots[0].transform.localPosition.y : 0f;

        // Стартовая позиция
        Vector3 startPos = Vector3.zero;
        switch (startEdge)
        {
            case StartEdge.Left:
                startPos = new Vector3(parentBounds.min.x + edgeOffset, yPos, 0f);
                break;
            case StartEdge.Center:
                startPos = new Vector3(parentBounds.center.x - totalWidth / 2f, yPos, 0f);
                break;
            case StartEdge.Right:
                startPos = new Vector3(parentBounds.max.x - totalWidth - edgeOffset, yPos, 0f);
                break;
        }

        // Сначала заполненные слоты, потом пустые
        List<DishSlot> orderedSlots = new List<DishSlot>();
        orderedSlots.AddRange(_slots.FindAll(s => !s.IsFree));
        orderedSlots.AddRange(_slots.FindAll(s => s.IsFree));
        orderedSlots.Reverse(); // Визуальный реверс

        // Анимация перемещения всех слотов одновременно
        Sequence seq = DOTween.Sequence();

        foreach (var slot in orderedSlots)
        {
            Vector3 target = startPos + Vector3.right * (orderedSlots.IndexOf(slot) * currentSpacing);
            seq.Join(slot.transform.DOLocalMove(target, 0.12f).SetEase(Ease.OutCubic));
        }
         
        // Ждём завершения Sequence в корутине
        yield return seq.WaitForCompletion();
    }

    /*public IEnumerator UpdateLayoutAnimatedRoutine()
    {
        if (_slots.Count == 0) yield break;

        float currentSpacing = spacing;
        float totalWidth = (_slots.Count - 1) * currentSpacing;

        // Получаем границы родителя через BoxCollider2D
        Bounds parentBounds = GetParentBounds();
        if (fitToParent && _slots.Count > 1)
        {
            float parentWidth = parentBounds.size.x - 2 * edgeOffset;
            if (totalWidth > parentWidth)
            {
                currentSpacing = parentWidth / (_slots.Count - 1);
                totalWidth = parentWidth;
            }
        }

        float yPos = _slots.Count > 0 ? _slots[0].transform.localPosition.y : 0f;

        // Стартовая позиция
        Vector3 startPos = Vector3.zero;
        switch (startEdge)
        {
            case StartEdge.Left:
                startPos = new Vector3(parentBounds.min.x + edgeOffset, yPos, 0f);
                break;
            case StartEdge.Center:
                startPos = new Vector3(parentBounds.center.x - totalWidth / 2f, yPos, 0f);
                break;
            case StartEdge.Right:
                startPos = new Vector3(parentBounds.max.x - totalWidth - edgeOffset, yPos, 0f);
                break;
        }

        // Сначала заполненные слоты, потом пустые
        List<DishSlot> orderedSlots = new List<DishSlot>();
        orderedSlots.AddRange(_slots.FindAll(s => !s.IsFree));
        orderedSlots.AddRange(_slots.FindAll(s => s.IsFree));
        orderedSlots.Reverse(); // Визуальный реверс

        // Анимация перемещения всех слотов одновременно
        Sequence seq = DOTween.Sequence();
        foreach (var slot in orderedSlots)
        {
            Vector3 target = startPos + Vector3.right * (orderedSlots.IndexOf(slot) * currentSpacing);
            seq.Join(slot.transform.DOLocalMove(target, 0.12f).SetEase(Ease.OutCubic));
        }

        // Вызываем событие по завершении анимации
        seq.OnComplete(() => OnRearrangeComplete?.Invoke());

        // Ждём завершения Sequence в корутине
        yield return seq.WaitForCompletion();
    }
    
    private Sequence UpdateLayoutAnimated()
    {
        if (_slots.Count == 0) return null;

        float currentSpacing = spacing;
        float totalWidth = (_slots.Count - 1) * currentSpacing;

        Bounds parentBounds = GetParentBounds();
        if (fitToParent && _slots.Count > 1)
        {
            float parentWidth = parentBounds.size.x - 2 * edgeOffset;
            if (totalWidth > parentWidth)
            {
                currentSpacing = parentWidth / (_slots.Count - 1);
                totalWidth = parentWidth;
            }
        }

        float yPos = _slots.Count > 0 ? _slots[0].transform.localPosition.y : 0f;

        Vector3 startPos = Vector3.zero;
        switch (startEdge)
        {
            case StartEdge.Left:
                startPos = new Vector3(parentBounds.min.x + edgeOffset, yPos, 0f);
                break;
            case StartEdge.Center:
                startPos = new Vector3(parentBounds.center.x - totalWidth / 2f, yPos, 0f);
                break;
            case StartEdge.Right:
                startPos = new Vector3(parentBounds.max.x - totalWidth - edgeOffset, yPos, 0f);
                break;
        }

        List<DishSlot> orderedSlots = new List<DishSlot>();
        orderedSlots.AddRange(_slots.FindAll(s => !s.IsFree));
        orderedSlots.AddRange(_slots.FindAll(s => s.IsFree));
        orderedSlots.Reverse();

        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < orderedSlots.Count; i++)
        {
            Vector3 target = startPos + Vector3.right * (i * currentSpacing);
            seq.Join(orderedSlots[i].transform.DOLocalMove(target, 0.12f).SetEase(Ease.OutCubic));
        }

        seq.OnComplete(() => OnRearrangeComplete?.Invoke());
        return seq;
    }
    */

    private Bounds GetParentBounds()
    {
        var collider = GetComponent<BoxCollider2D>();
        if (collider != null)
            return collider.bounds;

        return new Bounds(Vector3.zero, Vector3.zero);
    }

    public DishSlot GetNextSlot()
    {
        return _slots.FirstOrDefault(s => s.IsFree);
    }

    public Vector3 GetNextSlotPosition(bool inWorldSpace = true)
    {
        var slot = GetNextSlot();
        if (slot == null) return transform.position;
        return inWorldSpace ? slot.transform.position : slot.transform.localPosition;
    }

    public Vector3 GetSlot(Dish dish, bool inWorldSpace = true)
    {
        if (!_occupied.ContainsKey(dish))
            return transform.position;

        var slot = _occupied[dish];
        return inWorldSpace ? slot.transform.position : slot.transform.localPosition;
    }
    /*private void UpdateLayoutAnimated()
    {
        if (_slots.Count == 0) return;

        float currentSpacing = spacing;
        float totalWidth = (_slots.Count - 1) * currentSpacing;

        // Получаем границы родителя через BoxCollider2D
        Bounds parentBounds = GetParentBounds();
        if (fitToParent && _slots.Count > 1)
        {
            float parentWidth = parentBounds.size.x - 2 * edgeOffset;
            if (totalWidth > parentWidth)
            {
                currentSpacing = parentWidth / (_slots.Count - 1);
                totalWidth = parentWidth;
            }
        }

        // Сохраняем текущую высоту слотов
        float yPos = _slots.Count > 0 ? _slots[0].transform.localPosition.y : 0f;

        // Стартовая позиция
        Vector3 startPos = Vector3.zero;
        switch (startEdge)
        {
            case StartEdge.Left:
                startPos = new Vector3(parentBounds.min.x + edgeOffset, yPos, 0f);
                break;
            case StartEdge.Center:
                startPos = new Vector3(parentBounds.center.x - totalWidth / 2f, yPos, 0f);
                break;
            case StartEdge.Right:
                startPos = new Vector3(parentBounds.max.x - totalWidth - edgeOffset, yPos, 0f);
                break;
        }

        // Сначала заполненные слоты, потом пустые
        List<DishSlot> orderedSlots = new List<DishSlot>();
        orderedSlots.AddRange(_slots.FindAll(s => !s.IsFree));
        orderedSlots.AddRange(_slots.FindAll(s => s.IsFree));

        // Визуальный реверс, чтобы первый элемент оказался слева
        orderedSlots.Reverse();

        // Анимация перемещения
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < orderedSlots.Count; i++)
        {
            Vector3 target = startPos + Vector3.right * (i * currentSpacing);
            seq.Join(orderedSlots[i].transform.DOLocalMove(target, 0.12f).SetEase(Ease.OutCubic));
        }

        seq.OnComplete(() => OnRearrangeComplete?.Invoke());
    }*/

    // Редакторская версия для визуализации в сцене
    private void UpdateLayoutEditor()
    {
        float currentSpacing = spacing;
        float totalWidth = (maxCount - 1) * currentSpacing;

        Bounds parentBounds = GetParentBounds();
        if (fitToParent && maxCount > 1)
        {
            float parentWidth = parentBounds.size.x - 2 * edgeOffset;
            if (totalWidth > parentWidth)
            {
                currentSpacing = parentWidth / (maxCount - 1);
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

        // Сначала заполненные слоты, потом пустые
        List<DishSlot> orderedSlots = new List<DishSlot>();
        orderedSlots.AddRange(_slots.FindAll(s => !s.IsFree));
        orderedSlots.AddRange(_slots.FindAll(s => s.IsFree));

        orderedSlots.Reverse();

        for (int i = 0; i < orderedSlots.Count; i++)
        {
            orderedSlots[i].transform.localPosition = startPos + Vector3.right * (i * currentSpacing);
        }
    }
}
