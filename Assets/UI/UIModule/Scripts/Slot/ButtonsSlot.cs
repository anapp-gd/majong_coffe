using UnityEngine;
using UnityEngine.UI;

public class ButtonsSlot : SourceSlot
{
    [SerializeField] private Button _btnExit;
    [SerializeField] private Button _btnSettings;

    public override SourceSlot Init(SourceLayout layout)
    {
        base.Init(layout);
        _btnClick.interactable = false;
        _btnExit.onClick.AddListener(OnExit);
        _btnSettings.onClick.AddListener(OnSettings);
        return this;
    }

    public override void Dispose()
    {
        base.Dispose();
        _btnExit.onClick.RemoveAllListeners();
        _btnSettings.onClick.RemoveAllListeners();
    }

    public override void OnInject()
    {
        base.OnInject();

        
    }

    void OnExit()
    {

    }

    void OnSettings()
    {

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
