using System;
using System.Collections.Generic;
using UnityEngine;

public class ServingWindow : MonoBehaviour
{
    private PlayState _state;
    public event Action<List<Dish>> OnServingUpdate;
    private List<Dish> _readyDishes = new();
    private PlayState.PlayStatus _status;

    public void Init(PlayState state)
    {
        _state = state;
        _state.PlayStatusChanged += OnPlayStatusChange;
    }

    void OnPlayStatusChange(PlayState.PlayStatus playStatus)
    {
        _status = playStatus;
    }

    public void AddDish(Dish dish)
    {
        if (_status != PlayState.PlayStatus.play) return;

        _readyDishes.Add(dish);
        Debug.Log($"{dish.Type} добавлено в окно выдачи");
        OnServingUpdate?.Invoke(_readyDishes);
    }

    public bool TryTakeDish(Enums.DishType dishType, out Dish dish)
    {
        dish = null;

        if (_status != PlayState.PlayStatus.play) return false;

        if (_readyDishes == null || _readyDishes.Count == 0)
            return false; // нет блюд вообще

        // пробуем найти нужное
        dish = _readyDishes.Find(d => d.Type == dishType);

        if (dish != null)
        {
            _readyDishes.Remove(dish);
            Debug.Log($"{dish.Type} забрано из окна выдачи");
        }
        else
        {
            dish = _readyDishes[UnityEngine.Random.Range(0, _readyDishes.Count)];
            _readyDishes.Remove(dish);
            Debug.Log($"Нужного блюда ({dishType}) не было, выдано случайное: {dish.Type}");
        }

        if (_readyDishes.Count == 0)
        {
            _state.SetTableClear();
        }

        OnServingUpdate?.Invoke(_readyDishes);
        return true;
    }

    public bool IsFull()
    {
        return _readyDishes.Count >= 5;
    }
}