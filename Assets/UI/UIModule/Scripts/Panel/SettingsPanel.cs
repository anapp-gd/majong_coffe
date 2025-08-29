using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : SourcePanel
{
    [SerializeField] Button _btnSound;
    [SerializeField] Button _btnMusic;
    [SerializeField] Button _btnVibro;
    [SerializeField] Button _btnClose;

    const string SOUND_KEY = "SoundEnabled";
    const string MUSIC_KEY = "MusicEnabled";
    const string VIBRO_KEY = "VibroEnabled";

    public override void Init(SourceCanvas canvasParent)
    {
        base.Init(canvasParent);

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
        State.Instance.Close();
    }

    void OnSoundChange()
    {
        TogglePref(SOUND_KEY);
        SetButtonState(SOUND_KEY, _btnSound, PlayerPrefs.GetInt(SOUND_KEY, 1) == 1);
    }

    void OnMusicChange()
    {
        TogglePref(MUSIC_KEY);
        SetButtonState(MUSIC_KEY, _btnMusic, PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1);
    }

    void OnVibroChange()
    {
        TogglePref(VIBRO_KEY);
        SetButtonState(VIBRO_KEY, _btnVibro, PlayerPrefs.GetInt(VIBRO_KEY, 1) == 1);
    }

    void TogglePref(string key)
    {
        int value = PlayerPrefs.GetInt(key, 1); // по умолчанию включено
        value = value == 1 ? 0 : 1;
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    void UpdateButtons()
    { 
        SetButtonState(SOUND_KEY, _btnSound, PlayerPrefs.GetInt(SOUND_KEY, 1) == 1);
        SetButtonState(MUSIC_KEY, _btnMusic, PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1);
        SetButtonState(VIBRO_KEY, _btnVibro, PlayerPrefs.GetInt(VIBRO_KEY, 1) == 1);
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
}
