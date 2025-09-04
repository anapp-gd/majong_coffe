using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FlyIcon : MonoBehaviour
{
    private Image _image;
    private Sequence _flySequence; // активная анимация полёта

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    /// <summary>
    /// Устанавливает спрайт для иконки
    /// </summary>
    public void InvokeFly(Sprite sprite)
    {
        _image.sprite = sprite;
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero; // чтобы "вырастало"
    }

    /// <summary>
    /// Запуск анимации полёта
    /// </summary>
    public void PlayFly(RectTransform flyRect, Vector2 endLocalPos, float duration, System.Action onArrive)
    {
        // если уже что-то летит → убиваем
        _flySequence?.Kill();

        _flySequence = DOTween.Sequence();

        // Этап 1: поп-ап
        _flySequence.Append(flyRect.DOScale(1.5f, 0.25f).SetEase(Ease.OutBack));

        // Этап 2: полёт
        _flySequence.Append(flyRect.DOAnchorPos(endLocalPos, duration).SetEase(Ease.InOutCubic));

        // Этап 3: прибытие
        _flySequence.OnComplete(() =>
        {
            _flySequence = null;
            onArrive?.Invoke();
        });
    }

    /// <summary>
    /// Завершение с красивым punch-эффектом
    /// </summary>
    public void Finish()
    {
        _flySequence?.Kill(); // если летело — стопаем
        _flySequence = null;

        transform.DOPunchScale(Vector3.one * 0.1f, 0.25f, 6, 1f)
            .OnComplete(() => gameObject.SetActive(false));
    }
}
