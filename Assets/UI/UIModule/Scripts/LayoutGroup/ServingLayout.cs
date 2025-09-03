using System.Collections.Generic;
using UnityEngine;

public class ServingLayout : SourceLayout
{
    [UIInject] ServingWindow _servingWindow;

    public override SourceLayout Init(SourcePanel panel)
    {
        base.Init(panel);

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] is OrderSlot orderSlot)
            {
                orderSlot.Data = null;
                orderSlot.UpdateView();
            }
        }

        return this;
    }

    public OrderSlot GetNextSlot(Dish dish)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] is OrderSlot orderSlot && !orderSlot.IsBusy)
            {
                orderSlot.SetNextPos(dish);

                return orderSlot;
            }
        }

        return null; // fallback: сам layout
    }

    public void AddDish(OrderSlot slot)
    { 
        slot.UpdateView(); 
    }

    public void Remove(Dish dish)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] is OrderSlot orderSlot && ReferenceEquals(orderSlot.Data, dish))
            {
                orderSlot.Remove();
                break;
            }
        }
    }

    public override void OnInject()
    {
        base.OnInject();

        _servingWindow.OnServingUpdate += UpdateLayout;

        ClearLayout();
    }

    void ClearLayout()
    { 
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] is OrderSlot orderSlot)
            {
                orderSlot.Data = null;
                orderSlot.UpdateView();
            }
        }
    }

    void UpdateLayout(List<Dish> dishes)
    {
        if (dishes == null || _slots == null)
            return;

        int count = Mathf.Min(dishes.Count, _slots.Length);

        for (int i = 0; i < count; i++)
        {
            if (_slots[i] is OrderSlot orderSlot && dishes[i] != null)
            {
                orderSlot.Data = dishes[i];
                orderSlot.UpdateView();
            }
        } 

        for (int i = count; i < _slots.Length; i++)
        {
            if (_slots[i] is OrderSlot orderSlot)
            {
                orderSlot.Data = null;
                orderSlot.UpdateView();
            }
        }
    }

    public override void CloseIt()
    {
        gameObject.SetActive(false);
    }
}
