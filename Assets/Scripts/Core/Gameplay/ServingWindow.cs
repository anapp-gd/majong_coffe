using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ServingWindow : MonoBehaviour
{
    [SerializeField] FlyIcon _flyIcon;

    private PlayState _state;
    public event Action<List<Dish>> OnServingUpdate;
    private List<Dish> _readyDishes = new();
    private PlayState.PlayStatus _status;
    private WorldHorizontalLayout _layout;

    public bool IsFree => _currentInGameCountDishes < 5;

    private int _currentInGameCountDishes;
    private List<FlyIcon> _currentIconsFly;

    public void Init(PlayState state)
    {
        _state = state;
        _state.PlayStatusChanged += OnPlayStatusChange;
        _layout = GetComponent<WorldHorizontalLayout>();
        _currentIconsFly = new List<FlyIcon>();
    }

    void OnPlayStatusChange(PlayState.PlayStatus playStatus)
    {
        _status = playStatus;
    }


    public void AddDish(Vector3 worldMergePos, Dish dish)
    {
        if (_status != PlayState.PlayStatus.play) return;

        InvokeMoveTile(worldMergePos, dish);
    }

    void InvokeMoveTile(Vector3 mergeWorldPos, Dish dish)
    {
        if (_layout == null)
        {
            Debug.LogError("Layout не установлен!");
            return;
        }

        // 1. Создаём иконку в мире
        var flyIcon = Instantiate(_flyIcon);
        flyIcon.InvokeFly(dish.Icon);
        flyIcon.transform.position = mergeWorldPos;
        flyIcon.transform.localScale = Vector3.zero;

        // 2. Получаем целевой слот
        Vector3 targetPos = _layout.GetNextSlot(); 

        // 3. Настройки анимации
        float duration = 0.2f;

        _currentIconsFly.Add(flyIcon);

        _currentInGameCountDishes++;

        // 4. Запускаем анимацию
        flyIcon.PlayFlyWorld(targetPos, duration, () =>
        {
            // Проверяем, свободен ли слот к моменту завершения
            if (!_layout.AddObject(dish, flyIcon.transform, () =>  OnFlyComplete(dish, flyIcon)))
            {
                flyIcon.CancelFly();
            } 
        });
    }

    void OnFlyComplete(Dish dish, FlyIcon icon)
    {
        _readyDishes.Add(dish);
        _currentIconsFly.Remove(icon);

        if (_currentInGameCountDishes >= 5) _state.ForceTakeDish(); 
    }

    public void Finish()
    {
        foreach (var icon in _currentIconsFly)
        {
            icon.CancelFly();
        }
    }

    public bool TryTakeDish(Enums.DishType dishType, out Dish dish)
    {
        dish = null;

        if (_status != PlayState.PlayStatus.play)
            return false;

        if (_readyDishes == null || _readyDishes.Count == 0)
            return false; // нет блюд вообще

        // пробуем найти нужное
        dish = _readyDishes.Find(d => d.Type == dishType);

        // если нужного нет → берём первый
        if (dish == null)
            dish = _readyDishes[0];

        if (dish == null)
            return false;

        _layout.RemoveObject(dish);

        _readyDishes.Remove(dish);

        _currentInGameCountDishes--;

        if (_readyDishes.Count == 0)
            _state.SetTableClear();

        return true;
    } 
}