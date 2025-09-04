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


    public void AddDish(Vector3 worldMergePos, Dish dish)
    {
        if (_status != PlayState.PlayStatus.play) return;

        if (UIModule.TryGetCanvas<PlayCanvas>(out var playCanvas))
        {
            var playPanel = playCanvas.GetPanel<PlayPanel>();

            playPanel.InvokeMoveTile(worldMergePos, dish);

            Debug.Log($"{dish.Type} добавлено в окно выдачи");
        }

        _readyDishes.Add(dish); 
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

        if (UIModule.TryGetCanvas<PlayCanvas>(out var playCanvas))
        {
            var playPanel = playCanvas.GetPanel<PlayPanel>();
            playPanel.RemoveTile(dish);
            Debug.Log($"{dish.Type} убрано из окна выдачи");
        }

        _readyDishes.Remove(dish);

        if (_readyDishes.Count == 0)
            _state.SetTableClear();

        return true;
    }


    public bool IsFull()
    {
        return _readyDishes.Count >= 5;
    }
}