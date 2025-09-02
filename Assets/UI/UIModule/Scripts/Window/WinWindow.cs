using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinWindow : SourceWindow
{
    [SerializeField] ParticleSystem[] _winParticles;

    [SerializeField] Text _resultTitle;
    [SerializeField] Button _next;
    [SerializeField] Transform[] _winStars;

    [UIInject] PlayState _state;

    [SerializeField] float _animationDuration = 1.5f; // ������������ �������� � ��������

    public override SourceWindow Init(SourcePanel panel)
    {
        base.Init(panel);
        _next.onClick.AddListener(OnNext);
        return this;
    }

    public override void OnOpen()
    {
        base.OnOpen();

        PlayParticles();

        StartCoroutine(ResultTitleUpdate());
    }

    public override void OnClose()
    {
        base.OnClose();

        DisposeParticles();
    }

    void DisposeParticles()
    {
        for (int i = 0; i < _winParticles.Length; i++)
        {
            _winParticles[i].gameObject.SetActive(false);
        } 
    }

    void PlayParticles()
    {
        for (int i = 0; i < _winParticles.Length; i++)
        {
            _winParticles[i].gameObject.SetActive(true);
        }
    }

    IEnumerator ResultTitleUpdate()
    {
        _next.interactable = false;

        int result = _state.GetResaultValue;

        float elapsed = 0f;
        int startValue = 0;

        while (elapsed < _animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _animationDuration);

            // ������������ �����
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, result, t));
            _resultTitle.text = $"${currentValue}";

            yield return null;
        }

        // � ����� ����� ������ ��������� ��������
        _resultTitle.text = $"${result}";

        _next.interactable = true;
    }

    void OnNext()
    {
        if (UIModule.OpenCanvas<LoadingCanvas>(out var loadingCanvas))
        {
            loadingCanvas.OpenPanel<LoadingPanel>();
        }

        SceneManager.LoadScene(1);
    }

    public override void Dispose()
    {
        _next.onClick.RemoveAllListeners();
    }
}
