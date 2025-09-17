using UnityEngine.UI;

public class ResourceSlot : SourceSlot
{
    private Text _title; 

    public override SourceSlot Init(SourceLayout layout)
    {
        base.Init(layout);
        _title = GetComponentInChildren<Text>();
        if(_btnClick) _btnClick.interactable = false;
        ObserverEntity.Instance.PlayerMetaResourceChanged += OnUpdateResourceValue;
        OnUpdateResourceValue(PlayerEntity.Instance.GetResource);
        return this;
    }

    public override void Dispose()
    {
        ObserverEntity.Instance.PlayerMetaResourceChanged -= OnUpdateResourceValue;
        base.Dispose();
    }

    public override void OnInject()
    {

    }

    public override void OnActive()
    {

    }

    public override void OnClick()
    {

    }

    void OnUpdateResourceValue(int value)
    {
        _title.text = $"${value}";
    }

    public override void UpdateView()
    {

    }
}
