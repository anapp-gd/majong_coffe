using System.Linq;
using UnityEngine;

public class UpgradeLayout : SourceLayout
{

    public override void OnOpen()
    {
        var itemConfig = ConfigModule.GetConfig<UpgradeConfig>();

        int index = 0;

        var items = itemConfig.Items
            .OrderBy(x => x.ItemData.Level)
            .ToList();

        foreach (var slot in _slots)
        {
            if (slot is ShopBuildSlot shopSlot)
            {
                shopSlot.Data = items[index];
                index++;
            }

            slot.UpdateView();

            if (index >= items.Count)
                break;
        }

        base.OnOpen();
    }


    public override void CloseIt()
    {
        gameObject.SetActive(false);
    }  
}
