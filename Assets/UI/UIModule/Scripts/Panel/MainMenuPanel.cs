using System; 
using System.Linq; 
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : SourcePanel
{
    [SerializeField] AudioClip _audioClick;
    private AudioSource _audioSource;

    [SerializeField] Button _btnPlay;
    [SerializeField] Button _btnBuild;

    [SerializeField] Text _titlePlay;
    [SerializeField] Text _titleBuild;

    [SerializeField] Text _progressTitle;
    [SerializeField] Image _progressFill;
    [SerializeField] Slider _progressSlider;

    [SerializeField] private float _duration = 1.5f;

    Coroutine _animateCoroutine;

    public override void Init(SourceCanvas canvasParent)
    {
        base.Init(canvasParent);
        _audioSource = gameObject.AddComponent<AudioSource>();
        _btnPlay.onClick.AddListener(OnPlay);
        _btnBuild.onClick.AddListener(OnUpgrade); 
    }

    public override void OnOpen(params Action[] onComplete)
    {
        base.OnOpen(onComplete);

        _titlePlay.text = $"{PlayerEntity.Instance.GetCurrentLevel + 1}";

        var upgradeConfig = ConfigModule.GetConfig<UpgradeConfig>();

        int max = upgradeConfig.Items.Max(x => x.ItemData.Level);

        if (PlayerEntity.Instance.Data.Count > 0)
        { 
            int current = PlayerEntity.Instance.Data.Max(x => x.Level);

            float value = Mathf.Clamp01((float)current / max);

            var nextLevelInfo = upgradeConfig.Items.Find(x => x.ItemData.Level == current + 1);

            if (nextLevelInfo != null)
            {
                _titleBuild.text = $"{nextLevelInfo.ItemData.Cost}";
                _progressTitle.text = $"{current}/{max}";
            }
            else
            {
                _titleBuild.text = "Max level reached";
                _progressTitle.text = $"{current}/{max}";
            }

            _progressFill.fillAmount = value;
            _progressSlider.value = value;
        }
        else
        { 
            _titleBuild.text = $"{upgradeConfig.Items[0].ItemData.Cost}";
            _progressTitle.text = $"{0}/{max}";
        }
    } 

    void OnPlay()
    {
        if (PlayerEntity.Instance.IsVibro) Vibration.VibratePop();
        if (PlayerEntity.Instance.IsSound)
        {
            _audioSource.PlayOneShot(_audioClick);
        }
        MenuState.Instance.Play();
    }

    void OnUpgrade()
    {
        if (PlayerEntity.Instance.IsVibro) Vibration.VibratePop();
        if (PlayerEntity.Instance.IsSound)
        {
            _audioSource.PlayOneShot(_audioClick);
        }

        if (UIModule.TryGetCanvas<MainMenuCanvas>(out var mainMenuCanvas))
        {
            mainMenuCanvas.OpenPanel<BuildMenuPanel>();
        }
    }
    
    public override void OnDispose()
    { 
        base.OnDispose();
         
        _btnPlay.onClick.RemoveAllListeners();
        _btnBuild.onClick.RemoveAllListeners();
    }
}
