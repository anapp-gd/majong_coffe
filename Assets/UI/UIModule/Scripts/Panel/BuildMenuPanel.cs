using System;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenuPanel : SourcePanel
{
    [SerializeField] AudioClip _audioClick;
    private AudioSource _audioSource;
    [SerializeField] Button _btnClose;

    public override void Init(SourceCanvas canvasParent)
    {
        base.Init(canvasParent); 
        _audioSource = gameObject.AddComponent<AudioSource>(); 
        _btnClose.onClick.AddListener(OnClose);
    }

    public void PlaySound()
    { 
        if (PlayerEntity.Instance.IsSound)
        {
            _audioSource.PlayOneShot(_audioClick);
        }
    }
     
    void OnClose()
    {
        PlaySound();

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
