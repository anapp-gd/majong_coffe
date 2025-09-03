using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InfoLayout : SourceLayout
{
    [SerializeField] AudioClip _audioClick;
    private AudioSource _audioSource;
    [SerializeField] Button _btnSettings;
    [SerializeField] Button _btnMenu;

    public override SourceLayout Init(SourcePanel panel)
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _btnSettings.onClick.AddListener(OnSettings);
        _btnMenu.onClick.AddListener(OnMenu);
        return base.Init(panel);
    }

    void OnSettings()
    {
        if (PlayerEntity.Instance.IsSound)
        {
            _audioSource.PlayOneShot(_audioClick);
        }
        if (UIModule.TryGetCanvas<PlayCanvas>(out var playCanvas))
        {
            playCanvas.OpenPanel<SettingsPanel>();
        }
    }

    void OnMenu()
    {
        if (PlayerEntity.Instance.IsSound)
        {
            _audioSource.PlayOneShot(_audioClick);
        }
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
