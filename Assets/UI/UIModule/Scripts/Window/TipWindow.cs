using System.Collections.Generic;

public class TipWindow : SourceWindow
{
    private TipSlot[] slots;

    public override SourceWindow Init(SourcePanel panel)
    {
        slots = GetComponentsInChildren<TipSlot>();

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].gameObject.SetActive(false);
        }

        return base.Init(panel);
    }

    public void UpdateSlots(List<Enums.DishType> wantedDishes)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (i >= wantedDishes.Count) break;

            var wantedDish = wantedDishes[i];

            var slot = slots[i];

            slot.Data = wantedDish;

            slot.UpdateView();
        }

        OnOpen();
    }

    public override void Dispose()
    {

    }
}
