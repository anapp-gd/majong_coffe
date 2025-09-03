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

        // 1. Получаем слот
        var targetSlot = GetLayout<ServingLayout>().GetNextSlot(dish);
        var rectTargetSlot = targetSlot.GetComponent<RectTransform>();

        // 2. Конвертируем позицию слота в local canvas coords
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(null, rectTargetSlot.position),
            null,
            out Vector2 endLocalPos);

        // 3. Настройки анимации
        float duration = 0.6f;

        // 4. Запускаем анимацию через FlyIcon 
        flyIcon.PlayFly(flyRect, endLocalPos, duration, () =>
        {
            layout.AddDish(targetSlot); // добавляем блюдо
            flyIcon.Finish();           // добиваем punch-эффектом
        });
    }
}
