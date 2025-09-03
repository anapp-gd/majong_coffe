using System; 
using System.Linq; 
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : SourcePanel
{
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

        _btnPlay.onClick.AddListener(OnPlay);
        _btnBuild.onClick.AddListener(OnUpgrade); 
    }

    public override void OnOpen(params Action[] onComplete)
    {
        base.OnOpen(onComplete);

        _titlePlay.text = $"{PlayerEntity.Instance.GetCurrentLevel + 1}";

        var upgradeConfig = ConfigModule.GetConfig<UpgradeConfig>();

        int max = upgradeConfig.Items.Max(x => x.ItemData.Level);
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

    /*if (_animateCoroutine != null)
    {
        StopCoroutine(_animateCoroutine);
        _animateCoroutine = null;
    }

    _animateCoroutine = StartCoroutine(AnimateRoutine(value, current, max));
    
    private IEnumerator AnimateRoutine(float targetFill, int startValue, int targetValue)
    { 
        float startFill = _progressSlider.value; 

        float time = 0f;
        while (time < _duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / _duration);

            float value = Mathf.Lerp(startFill, targetFill, t);
             
            _progressFill.fillAmount = value;
            _progressSlider.value = value;

            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, t));
            _progressTitle.text = $"{currentValue}/{targetValue}";

            yield return null;
        }

        _progressFill.fillAmount = targetFill;
        _progressSlider.value = targetFill;
        _progressTitle.text = $"${targetValue}/{targetValue}";

        _animateCoroutine = null;
    }
    */

    void OnPlay()
    {
        MenuState.Instance.Play();
    }

    void OnUpgrade()
    {
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
