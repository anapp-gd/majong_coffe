using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseWindow : SourceWindow
{
    [SerializeField] Button _btnRestart;

    public override SourceWindow Init(SourcePanel panel)
    {
        base.Init(panel);

        _btnRestart.onClick.AddListener(OnRestart);
         
        return this;
    }

    void OnRestart()
    {
        SceneManager.LoadScene(2);
    }

    public override void Dispose()
    {
        _btnRestart.onClick.RemoveAllListeners();
    }
}
