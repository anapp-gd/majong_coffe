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

    private ButtonHoldListener _listner;

    public override SourceSlot Init(SourceLayout layout)
    {
        _listner = GetComponentInChildren<ButtonHoldListener>();
        return base.Init(layout);
    }

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

            isBuyed = list.Exists(_ => _.Level == Data.ItemData.Level);

            gameObject.SetActive(true);

            _iconItem.sprite = Data.ItemData.Icon;
            _titleCost.text = $"{Data.ItemData.Cost}";
            _titleName.text = $"{Data.ItemData.Name}";
            _titleLevel.text = $"LEVEL {Data.ItemData.Level}";

            bool hasPrevious = true;
            if (Data.ItemData.Level > 1) // если не первый уровень
            {
                hasPrevious = list.Exists(_ => _.Level == Data.ItemData.Level - 1);
            }

            if (isBuyed)
            {
                _listner.SetInteractable(false);
                _buyBtn.interactable = false;
                _buyImage.sprite = interfaceConfig.BuyedButton;
            }
            else
            {
                // можно покупать только если предыдущий уровень куплен
                bool canBuy = hasPrevious && PlayerEntity.Instance.GetResource >= Data.ItemData.Cost;

                _listner.SetInteractable(canBuy);
                _buyBtn.interactable = canBuy;

                if (!hasPrevious || PlayerEntity.Instance.GetResource < Data.ItemData.Cost)
                {
                    _buyImage.sprite = interfaceConfig.CantBuyButton;
                }
                else
                {
                    _buyImage.sprite = interfaceConfig.BuyButton;
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
        var interfaceConfig = ConfigModule.GetConfig<InterfaceConfig>();

        var panel = _layout.GetSourcePanel<BuildMenuPanel>();

        if (Data.Buy(()=> { panel.OnOpen(); }))
        {
            panel.OnCLose();

            _listner.SetInteractable(false);  
            _buyBtn.interactable = false;
            _buyImage.sprite = interfaceConfig.BuyedButton;
        } 
        else
        { 
            _buyBtn.image.sprite = interfaceConfig.CantBuyButton;
        }
    }
}
