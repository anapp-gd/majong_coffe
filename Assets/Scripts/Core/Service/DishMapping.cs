using System.Collections.Generic;
using UnityEngine;

public static class DishMapping
{
    private static Dictionary<Enums.TileType, Enums.DishType> map = new()
    {
        { Enums.TileType.Eggs, Enums.DishType.FriedEgg },
        { Enums.TileType.CoffeeBeans, Enums.DishType.CoffeeCup },
        { Enums.TileType.Bread, Enums.DishType.Toast },
        { Enums.TileType.Potato, Enums.DishType.FrenchFries },
        { Enums.TileType.Dough, Enums.DishType.Pizza },
        { Enums.TileType.Salmon, Enums.DishType.FishSteak },
        { Enums.TileType.Chiken, Enums.DishType.Nuggets },
        { Enums.TileType.Limon, Enums.DishType.Lemonade },
        { Enums.TileType.Cheese, Enums.DishType.CheeseStick },
        { Enums.TileType.Blueberries, Enums.DishType.BlueberriesCheesecake },
        { Enums.TileType.TeaLeaves, Enums.DishType.Tea },
        { Enums.TileType.Tomato, Enums.DishType.Bruschetta },
        { Enums.TileType.Cream, Enums.DishType.IceCream },
        { Enums.TileType.Pasta, Enums.DishType.WOK }, 
        { Enums.TileType.Onion, Enums.DishType.OnionRing }, 
        { Enums.TileType.Olive, Enums.DishType.GreekSalad }, 
        { Enums.TileType.Sausages, Enums.DishType.Hotdog }, 
        { Enums.TileType.Beef, Enums.DishType.Steak }, 
        { Enums.TileType.Apple, Enums.DishType.ApplePie }, 
        { Enums.TileType.Kvass, Enums.DishType.Okroshka }, 
        { Enums.TileType.Pumpkin, Enums.DishType.CreamSoup }, 
        { Enums.TileType.Beet, Enums.DishType.Borsch }, 
        { Enums.TileType.MincedMeat, Enums.DishType.Khinkali }, 
        { Enums.TileType.Cabbage, Enums.DishType.Sauerkraut }, 
        { Enums.TileType.Cucumbers, Enums.DishType.SaltedCucumbers }, 
        { Enums.TileType.Jam, Enums.DishType.PancakeJam }, 
        { Enums.TileType.Mushrooms, Enums.DishType.Julien }, 
        { Enums.TileType.Rice, Enums.DishType.Pilaf }, 
        { Enums.TileType.Caviar, Enums.DishType.CaviarSandwich }, 
        { Enums.TileType.CottageCheese, Enums.DishType.Cheesecakes }, 
        { Enums.TileType.Garlic, Enums.DishType.Croutons }, 
        { Enums.TileType.Shrimp, Enums.DishType.GrilledShrimp }, 
        { Enums.TileType.LettuceLeaf, Enums.DishType.Sandwich }, 
    };
     
    private static readonly Dictionary<Enums.DishType, Enums.TileType> reverseMap; 

    static DishMapping()
    {
        reverseMap = new Dictionary<Enums.DishType, Enums.TileType>();
        foreach (var kvp in map)
            reverseMap[kvp.Value] = kvp.Key;
    }

    public static bool TryGetDish(Enums.TileType tile, out Enums.DishType dish)
    {
        return map.TryGetValue(tile, out dish);
    }

    public static bool TryGetTile(Enums.DishType dish, out Enums.TileType tile)
    {
        return reverseMap.TryGetValue(dish, out tile);
    }
}