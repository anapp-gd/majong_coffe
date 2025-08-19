using System;
using System.Collections.Generic;
using UnityEngine;

public class ServingWindow : MonoBehaviour
{
    private PlayState _state;
    public event Action<List<Dish>> OnServingUpdate;
    private List<Dish> _readyDishes = new();

    public void Init(PlayState state)
    {
        _state = state;
    }

    public void AddDish(Dish dish)
    {
        _readyDishes.Add(dish);
        Debug.Log($"{dish.Type} добавлено в окно выдачи");
        OnServingUpdate?.Invoke(_readyDishes);
    }

    public bool TryTakeDish(Enums.DishType dishType, out Dish dish)
    {
        dish = _readyDishes.Find(d => d.Type == dishType);

        if (dish != null)
        {
            _readyDishes.Remove(dish);
            Debug.Log($"{dish.Type} забрано из окна выдачи");
            OnServingUpdate?.Invoke(_readyDishes);
            return true;
        }
        return false;
    }

    public bool IsFull()
    {
        return _readyDishes.Count >= 5;
    }
}