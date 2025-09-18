using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseWindow : SourceWindow
{
    [SerializeField] AudioClip _audioClick;
    private AudioSource _audioSource;
    [SerializeField] Button _btnRestart;

    public override SourceWindow Init(SourcePanel panel)
    {
        base.Init(panel);

        _audioSource = gameObject.AddComponent<AudioSource>();
        _btnRestart.onClick.AddListener(OnRestart);
         
        return this;
    }

    void OnRestart()
    {
        if (PlayerEntity.Instance.IsVibro) Vibration.VibratePop();
        if (PlayerEntity.Instance.IsSound)
        {
            _audioSource.PlayOneShot(_audioClick);
        }
        
        if (UIModule.OpenCanvas<LoadingCanvas>(out var loadingCanvas))
        {
            loadingCanvas.OpenPanel<LoadingPanel>(false, () =>
            {
                SceneManager.LoadScene(2);
            });
        } 
    }

    public override void Dispose()
    {
        _btnRestart.onClick.RemoveAllListeners();
    }
}
