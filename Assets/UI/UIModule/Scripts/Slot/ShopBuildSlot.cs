using UnityEngine;
using UnityEngine.UI;

public class ShopBuildSlot : SourceSlot
{
    [HideInInspector] public ItemBase Data;
    private bool isBuyed;
    [SerializeField] private Button _buyBtn;
    [SerializeField] private Image _buyImage;
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
            var interfaceConfig = ConfigModule.GetConfig<InterfaceConfig>();

            var list = PlayerEntity.Instance.Data;

            isBuyed = list.Exists(_ => _.Type == Data.ItemData.Type);

            gameObject.SetActive(true);

            _iconItem.sprite = Data.ItemData.Icon;
            _titleCost.text = $"{Data.ItemData.Cost}";
            _titleName.text = $"{Data.ItemData.Name}";
            _titleLevel.text = $"LEVEL {Data.ItemData.Level}";
              
            if (isBuyed)
            {
                _buyBtn.interactable = false;
                _buyImage.sprite = interfaceConfig.BuyedButton;
            }
            else
            {
                _buyBtn.interactable = true;
                _buyImage.sprite = interfaceConfig.BuyButton;

                if (PlayerEntity.Instance.GetResource < Data.ItemData.Cost)
                {
                    _buyBtn.image.sprite = interfaceConfig.CantBuyButton;
                }
            }

        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    public void TryBuild()
    {
        if (Data.Buy())
        {
            _buyBtn.interactable = false;
        } 
        else
        { 
            var interfaceConfig = ConfigModule.GetConfig<InterfaceConfig>();
            _buyBtn.image.sprite = interfaceConfig.CantBuyButton;
        }
    }
}
