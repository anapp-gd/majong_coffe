public class OrderSlot : SourceSlot
{
    public Dish Data;
    public bool IsEmpty => Data == null;
    public bool IsBusy;



    public override void OnActive()
    {
        
    }

    public override void OnClick()
    { 
    }

    public void SetNextPos(Dish dish)
    {
        Data = dish;
        IsBusy = true;
        _icon.enabled = false;
        _background.enabled = true;
        gameObject.SetActive(true);
    }

    public override void UpdateView()
    {
        if (Data != null)
        {
            _background.enabled = true;
            _icon.enabled = true;
            _icon.sprite = Data.Icon;
        }
        else
        {
            _background.enabled = false;
            _icon.enabled = false;
            IsBusy = false; 
        }
    }

    public void Remove()
    {
        Data = null; 
        _background.enabled = false;
        _icon.enabled = false;
        IsBusy = false;
    }
}
