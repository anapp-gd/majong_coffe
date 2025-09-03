using UnityEngine;
using UnityEngine.UI;

public class MenuInfoLayout : SourceLayout
{
    [SerializeField] AudioClip _audioClick;
    private AudioSource _audioSource;
    [SerializeField] Button _btnSettings;

    public override SourceLayout Init(SourcePanel panel)
    {
        _btnSettings.onClick.AddListener(OnSettings); 
        _audioSource = gameObject.AddComponent<AudioSource>();
        return base.Init(panel);
    }

    void OnSettings()
    {
        if (PlayerEntity.Instance.IsSound)
        {
            _audioSource.PlayOneShot(_audioClick);
        }
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
