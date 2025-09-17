using UnityEngine;
[System.Serializable]
public class ItemData
{
    public int Level;
    public Sprite Icon;
    public Enums.ItemType Type;
    public int Cost;
    public string Name; 

    public ItemData(int level, Enums.ItemType type, int cost)
    {
        Level = level;
        Type = type;
        Cost = cost;
    }

    public bool TryBuy(out ItemData data)
    {
        data = null;

        if (PlayerEntity.Instance.TrySubResourceValue(Cost))
        {
            data = new ItemData(this.Level, this.Type, this.Cost);
            return true;
        }

        return false;
    }
}