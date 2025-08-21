using UnityEngine;
using UnityEngine.UI;

public class LevelInfoSlot : SourceSlot
{
    private Text _title;

    public override SourceSlot Init(SourceLayout layout)
    {
        base.Init(layout);
        _title = GetComponentInChildren<Text>(true);
        _btnClick.interactable = false;
        return this;
    }

    public override void OnInject()
    {
        base.OnInject();

        _title.text = $"LEVEL {PlayerEntity.Instance.GetCurrentLevel + 1}";
    }

    public override void OnActive()
    {

    }

    public override void OnClick()
    {

    }

    public override void UpdateView()
    {

    }
}
