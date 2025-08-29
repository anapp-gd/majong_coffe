using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InfoLayout : SourceLayout
{
    [SerializeField] Button _btnSettings;
    [SerializeField] Button _btnMenu;

    public override SourceLayout Init(SourcePanel panel)
    {
        _btnSettings.onClick.AddListener(OnSettings);
        _btnMenu.onClick.AddListener(OnMenu);
        return base.Init(panel);
    }

    void OnSettings()
    {
        if (UIModule.TryGetCanvas<PlayCanvas>(out var playCanvas))
        {
            playCanvas.OpenPanel<SettingsPanel>();
        }
    }

    void OnMenu()
    {
        if (UIModule.OpenCanvas<LoadingCanvas>(out var loadingCanvas))
        { 
            SceneManager.LoadScene(1);
        }
    } 

    public override void Dispose()
    {

        base.Dispose();
    }

    public override void CloseIt()
    {
        gameObject.SetActive(false);
    }
}
