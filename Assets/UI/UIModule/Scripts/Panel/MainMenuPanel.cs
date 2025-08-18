using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : SourcePanel
{
    [SerializeField] Button _btnPlay;
    [SerializeField] Button _btnBuild;

    public override void Init(SourceCanvas canvasParent)
    {
        base.Init(canvasParent);

        _btnPlay.onClick.AddListener(OnPlay);
        _btnBuild.onClick.AddListener(OnUpgrade);
    }

    void OnPlay()
    {
        MenuState.Instance.Play();
    }

    void OnUpgrade()
    {
        MenuState.Instance.Upgrade();
    }
    
    public override void OnDipose()
    { 
        base.OnDipose();

        _btnPlay.onClick.RemoveAllListeners();
        _btnBuild.onClick.RemoveAllListeners();
    }
}
