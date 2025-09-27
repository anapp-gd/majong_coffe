using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DishSlot : MonoBehaviour
{
    public Dish Dish { get; private set; }
    public bool IsFree => Dish == null;

    private Transform child;

    public void Assign(Dish dish, Transform obj)
    {
        Dish = dish;
        obj.SetParent(transform, false);
        obj.localPosition = Vector3.zero;
        obj.localScale = Vector3.zero;
        child = obj;
    }

    public Transform GetDishObject()
    {
        return child;
    }

    public void Release()
    {
        Dish = null; 
    }

    public void Clear()
    {
        Dish = null;
        foreach (Transform child in transform)
            GameObject.Destroy(child.gameObject);
    }
}
