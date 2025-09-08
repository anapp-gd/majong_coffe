using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FlyIcon : MonoBehaviour
{
    private Image _image;
    private SpriteRenderer _renderer;
    private Sequence _flySequence; // активная анимация полёта

    private void Awake()
    {
        _image = GetComponent<Image>();
        _renderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Устанавливает спрайт для иконки
    /// </summary>
    public void InvokeFly(Sprite sprite)
    {
        _renderer.sprite = sprite;
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero; // появляемся маленькими
    }

    /// <summary>
    /// Запуск анимации полёта (в мировых координатах)
    /// </summary>
    public void PlayFlyWorld(Vector3 endWorldPos, float duration, System.Action onArrive)
    {
        // если уже что-то летит → убиваем
        _flySequence?.Kill();

        _flySequence = DOTween.Sequence();

        // Этап 1: поп-ап
        _flySequence.Append(transform.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack));

        // Этап 2: полёт
        _flySequence.Append(transform.DOMove(endWorldPos, duration).SetEase(Ease.InOutCubic));

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
        _flySequence?.Kill();
        _flySequence = null;
    }

    /// <summary>
    /// Прервать полёт (например, слот занят)
    /// </summary>
    public void CancelFly()
    {
        _flySequence?.Kill();
        _flySequence = null;
        gameObject.SetActive(false);
    }
}
