using UnityEngine;

public class UpgradeLayout : SourceLayout
{

    public override void OnOpen()
    {
        var itemConfig = ConfigModule.GetConfig<UpgradeConfig>();

        int index = 0;

        foreach (var slot in _slots)
        {
            if (slot is ShopBuildSlot shopSlot)
            {
                shopSlot.Data = itemConfig.Items[index];
                index++;
            }

            slot.UpdateView();

            if (index >= itemConfig.Items.Count) break;
        }
        base.OnOpen();
    } 

    public override void CloseIt()
    {
        gameObject.SetActive(false);
    }  
}
