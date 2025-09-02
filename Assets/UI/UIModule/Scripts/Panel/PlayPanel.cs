using DG.Tweening;
using System;
using UnityEngine;

public class PlayPanel : SourcePanel
{
    [SerializeField] FlyIcon _flyIcon;

    private float duration = 1f;

    public override void OnOpen(params Action[] onComplete)
    {
        base.OnOpen(onComplete);

        OpenLayout<ServingLayout>(); 
    }

    public void RemoveTile(Dish dish)
    { 
        var layout = GetLayout<ServingLayout>();

        layout.Remove(dish);
    }

    public void InvokeMoveTile(Vector3 mergeWorldPos, Dish dish)
    {
        var canvas = _sourceCanvas;
        var canvasRect = canvas.transform as RectTransform;

        var layout = GetLayout<ServingLayout>();
        var layoutRect = layout.GetComponent<RectTransform>();

        // 1. World → Screen
        Vector3 screenPos = Camera.main.WorldToScreenPoint(mergeWorldPos);

        // 2. Screen → Local
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,
            out Vector2 startLocalPos);

        // 3. Создаём иконку
        var flyIcon = Instantiate(_flyIcon, canvas.transform);
        flyIcon.InvokeFly(dish.Icon);

        var flyRect = flyIcon.GetComponent<RectTransform>();
        flyRect.anchoredPosition = startLocalPos;

        // Спавним с нулевым scale
        flyRect.localScale = Vector3.zero;

        // 4. Конечный слот
        var targetSlot = GetLayout<ServingLayout>().GetNextSlot();
        var rectTargetSlot = targetSlot.GetComponent<RectTransform>();

        // Конвертируем позицию слота в local canvas coords
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(null, rectTargetSlot.position),
            null,
            out Vector2 endLocalPos);

        // 5. Анимация (последовательность)
        float duration = 0.6f;

        Sequence seq = DOTween.Sequence();

        // Этап 1: "поп-ап" (scale 0 → 1)
        seq.Append(flyRect.DOScale(1f, 0.25f).SetEase(Ease.OutBack));

        // Этап 2: полёт
        seq.Append(flyRect.DOAnchorPos(endLocalPos, duration).SetEase(Ease.InOutCubic));

        // Этап 3: добавляем в layout
        seq.OnComplete(() =>
        {
            layout.AddDish(targetSlot, dish);
            flyIcon.Finish();
        });
    }
}
