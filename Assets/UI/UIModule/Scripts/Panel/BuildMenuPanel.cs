using System;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenuPanel : SourcePanel
{
    [SerializeField] Button _btnClose;

    public override void Init(SourceCanvas canvasParent)
    {
        base.Init(canvasParent);

        _btnClose.onClick.AddListener(OnClose);
    }
     
    void OnClose()
    {
        if (UIModule.TryGetCanvas<MainMenuCanvas>(out var mainMenuCanvas))
        {
            mainMenuCanvas.OpenPanel<MainMenuPanel>();
        }
    }

    public override void OnOpen(params Action[] onComplete)
    {
        OpenLayout<UpgradeLayout>();

        base.OnOpen(onComplete);
    }

    public override void OnDispose()
    {
        base.OnDispose();

        _btnClose.onClick.RemoveAllListeners();
    }
}
