using UnityEngine;
using UnityEngine.UI;

public class ShopBuildSlot : SourceSlot
{
    [HideInInspector] public ItemBase Data;
    [SerializeField] private Image _iconItem;
    [SerializeField] private Text _titleName;
    [SerializeField] private Text _titleCost;
    [SerializeField] private Text _titleLevel;

    public override void OnActive()
    { 
    }

    public override void OnClick()
    { 
    }

    public override void UpdateView()
    { 
        if (Data)
        {
            gameObject.SetActive(true);

            _iconItem.sprite = Data.ItemData.Icon;
            _titleCost.text = $"{Data.ItemData.Cost}";
            _titleName.text = $"{Data.ItemData.Name}";
            _titleLevel.text = $"LEVEL {Data.ItemData.Level}";
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    public void TryBuild()
    {
        Data.Buy();
    }
}
