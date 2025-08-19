using System.Collections.Generic;
using UnityEngine;

public class ServingWindow : MonoBehaviour
{
    private PlayState _state;
    private List<Dish> readyDishes = new();

    public void Init(PlayState state)
    {
        _state = state;
    }

    public void AddDish(Dish dish)
    {
        readyDishes.Add(dish);
        Debug.Log($"{dish.Type} добавлено в окно выдачи");
    }

    public bool TryTakeDish(Enums.DishType dishType, out Dish dish)
    {
        dish = readyDishes.Find(d => d.Type == dishType);

        if (dish != null)
        {
            readyDishes.Remove(dish);
            Debug.Log($"{dish.Type} забрано из окна выдачи");
            return true;
        }
        return false;
    }
}