using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : SourcePanel
{
    [SerializeField] Button _btnPlay;
    [SerializeField] Button _btnBuild;

    [SerializeField] TextMeshProUGUI _titlePlay;
    [SerializeField] TextMeshProUGUI _titleBuild;

    [SerializeField] Text _progressTitle;
    [SerializeField] Image _progressFill;

    [SerializeField] private float _duration = 1.5f;

    Coroutine _animateCoroutine;

    public override void Init(SourceCanvas canvasParent)
    {
        base.Init(canvasParent);

        _btnPlay.onClick.AddListener(OnPlay);
        _btnBuild.onClick.AddListener(OnUpgrade);

        ObserverEntity.Instance.UpgradeProgreesChanged += OnProgressUpdate;
    }

    public override void OnOpen(params Action[] onComplete)
    {
        base.OnOpen(onComplete);

        _titlePlay.text = $"Start ({PlayerEntity.Instance.GetCurrentLevel + 1})";
        _titleBuild.text = $"Build ({10})";
    }

    void OnProgressUpdate(float targetFill, int startValue, int targetValue)
    {
        if (_animateCoroutine != null)
        {
            StopCoroutine(_animateCoroutine);
            _animateCoroutine = null;
        }

        _animateCoroutine = StartCoroutine(AnimateRoutine(targetFill, startValue, targetValue));
    }

    private IEnumerator AnimateRoutine(float targetFill, int startValue, int targetValue)
    {
        float startFill = _progressFill.fillAmount; 

        float time = 0f;
        while (time < _duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / _duration);

            _progressFill.fillAmount = Mathf.Lerp(startFill, targetFill, t);
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, t));
            _progressTitle.text = $"${currentValue}/{targetValue}";

            yield return null;
        }

        _progressFill.fillAmount = targetFill;
        _progressTitle.text = $"${targetValue}/{targetValue}";

        _animateCoroutine = null;
    }

    void OnPlay()
    {
        MenuState.Instance.Play();
    }

    void OnUpgrade()
    {
        MenuState.Instance.Upgrade();
    }
    
    public override void OnDispose()
    { 
        base.OnDispose();

        ObserverEntity.Instance.UpgradeProgreesChanged -= OnProgressUpdate;
        _btnPlay.onClick.RemoveAllListeners();
        _btnBuild.onClick.RemoveAllListeners();
    }
}
