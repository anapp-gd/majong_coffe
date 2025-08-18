using UnityEngine;

public class Dish
{
    public Enums.DishType Type { get; private set; }
    public Sprite Icon { get; private set; } // можно визуализировать еду

    public Dish(Enums.DishType type, Sprite icon = null)
    {
        Type = type;
        Icon = icon;
    }
}
