using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : SourcePanel
{
    [SerializeField] AudioClip _audioClick;

    [SerializeField] Button _btnSound;
    [SerializeField] Button _btnMusic;
    [SerializeField] Button _btnVibro;
    [SerializeField] Button _btnClose;

    const string SOUND_KEY = "SoundEnabled";
    const string MUSIC_KEY = "MusicEnabled";
    const string VIBRO_KEY = "VibroEnabled";

    private AudioSource _audioSource;

    public override void Init(SourceCanvas canvasParent)
    {
        base.Init(canvasParent);
        _audioSource = gameObject.AddComponent<AudioSource>();
        _btnClose.onClick.AddListener(OnClose);
        _btnSound.onClick.AddListener(OnSoundChange);
        _btnMusic.onClick.AddListener(OnMusicChange);
        _btnVibro.onClick.AddListener(OnVibroChange); 
    }

    public override void OnOpen(params Action[] onComplete)
    {
        UpdateButtons(); 
        base.OnOpen(onComplete);
    }

    void OnClose()
    {
        if(PlayerEntity.Instance.IsSound) _audioSource.PlayOneShot(_audioClick);

        PlayerEntity.Instance.Save();

        State.Instance.Close();
    }

    void OnSoundChange()
    {
        if (PlayerEntity.Instance.IsSound) _audioSource.PlayOneShot(_audioClick);

        bool result = !PlayerEntity.Instance.IsSound;

        SetButtonState(SOUND_KEY, _btnSound, result);

        PlayerEntity.Instance.IsSound = result; 
    }

    void OnMusicChange()
    {
        if (PlayerEntity.Instance.IsSound) _audioSource.PlayOneShot(_audioClick);

        bool result = !PlayerEntity.Instance.IsMusic;

        SetButtonState(MUSIC_KEY, _btnMusic, result);

        PlayerEntity.Instance.IsMusic = result;

        UIModule.Play();
    }

    void OnVibroChange()
    {
        if (PlayerEntity.Instance.IsSound) _audioSource.PlayOneShot(_audioClick);

        bool result = !PlayerEntity.Instance.IsVibro;

        SetButtonState(VIBRO_KEY, _btnVibro, result);

        PlayerEntity.Instance.IsVibro = result;
    } 

    void UpdateButtons()
    { 
        SetButtonState(SOUND_KEY, _btnSound, PlayerEntity.Instance.IsSound);
        SetButtonState(MUSIC_KEY, _btnMusic, PlayerEntity.Instance.IsMusic);
        SetButtonState(VIBRO_KEY, _btnVibro, PlayerEntity.Instance.IsVibro);
    }

    void SetButtonState(string key, Button btn, bool enabled)
    {
        var interfaceConfig = ConfigModule.GetConfig<InterfaceConfig>();

        var buttonIcon = btn.transform.GetChild(0).GetComponent<Image>();

        switch (key)
        {
            case SOUND_KEY: 
                if (enabled)
                    buttonIcon.sprite = interfaceConfig.SoundOn; 
                else
                    buttonIcon.sprite = interfaceConfig.SoundOff; 
                break;
            case MUSIC_KEY:
                if (enabled)
                    buttonIcon.sprite = interfaceConfig.MusicOn;
                else
                    buttonIcon.sprite = interfaceConfig.MusicOff;
                break;
            case VIBRO_KEY:
                if (enabled)
                    buttonIcon.sprite = interfaceConfig.VibroOn;
                else
                    buttonIcon.sprite = interfaceConfig.VibroOff;
                break;
        } 

        if (enabled)
        {
            btn.image.sprite = interfaceConfig.ButtonOn;
        }
        else
        {
            btn.image.sprite = interfaceConfig.ButtonOff;
        }
    }

    public override void OnDispose()
    {
        base.OnDispose();

        _btnClose.onClick.RemoveAllListeners();
        _btnMusic.onClick.RemoveAllListeners();
        _btnSound.onClick.RemoveAllListeners();
        _btnVibro.onClick.RemoveAllListeners();
    }
}
