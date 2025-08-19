public class OrderSlot : SourceSlot
{
    public Dish Data;

    public override void OnActive()
    {
        
    }

    public override void OnClick()
    { 
    }

    public override void UpdateView()
    {
        if (Data != null)
        {
            gameObject.SetActive(true);
            _icon.sprite = Data.Icon;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
