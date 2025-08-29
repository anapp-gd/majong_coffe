using UnityEngine;
using UnityEngine.UI;

public class MenuInfoLayout : SourceLayout
{
    [SerializeField] Button _btnSettings;

    public override SourceLayout Init(SourcePanel panel)
    {
        _btnSettings.onClick.AddListener(OnSettings);

        return base.Init(panel);
    }

    void OnSettings()
    {
        if (UIModule.TryGetCanvas<MainMenuCanvas>(out var mainMenuCanvas))
        {
            mainMenuCanvas.OpenPanel<SettingsPanel>();
        }
    }

    public override void Dispose()
    {
        _btnSettings.onClick.RemoveAllListeners();
        base.Dispose();
    }

    public override void CloseIt()
    { 
        gameObject.SetActive(false);
    }
}
