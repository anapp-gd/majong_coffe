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

        // загрузка текущего состояния
        UpdateButtons();
    }

    void OnClose()
    { 

    }

    void OnSoundChange()
    {
        TogglePref(SOUND_KEY);
        UpdateButtons();
    }

    void OnMusicChange()
    {
        TogglePref(MUSIC_KEY);
        UpdateButtons();
    }

    void OnVibroChange()
    {
        TogglePref(VIBRO_KEY);
        UpdateButtons();
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
        SetButtonState(_btnSound, PlayerPrefs.GetInt(SOUND_KEY, 1) == 1);
        SetButtonState(_btnMusic, PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1);
        SetButtonState(_btnVibro, PlayerPrefs.GetInt(VIBRO_KEY, 1) == 1);
    }

    void SetButtonState(Button btn, bool enabled)
    {
        var colors = btn.colors;
        colors.normalColor = enabled ? Color.green : Color.gray;
        btn.colors = colors;
    }
}
