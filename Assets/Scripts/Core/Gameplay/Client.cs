using UnityEngine;
using DG.Tweening;

public class Client : MonoBehaviour
{
    [SerializeField] private Sprite[] _views;
    [SerializeField] private Transform _happyReaction;
    [SerializeField] private Transform _angryReaction;
    [SerializeField] private Transform _sadReaction;
    public Enums.DishType WantedType => _wantedDish;

    private Enums.DishType _wantedDish;
    private ServingWindow _window;
    private System.Action<Client> _onLeave;

    public void Init(Enums.DishType dish, ServingWindow window, System.Action<Client> onLeave)
    {
        _wantedDish = dish;
        _window = window;
        _onLeave = onLeave;

        if (_views != null && TryGetComponent<SpriteRenderer>(out var renderer) && _views.Length > 0)
        {
            renderer.sprite = _views[Random.Range(0, _views.Length)];
        }

        // сразу выключаем все реакции
        _happyReaction?.gameObject.SetActive(false);
        _angryReaction?.gameObject.SetActive(false);
        _sadReaction?.gameObject.SetActive(false);
    }

    public void TryTakeDish()
    {
        if (_window.TryTakeDish(_wantedDish, out Dish dish))
        {
            if (_wantedDish == dish.Type)
                Leave(dish, success: true);
            else
                Leave(dish, success: false);
        }
        else
        {
            Leave(null, success: false);
        }
    }

    public void MoveToQueuePosition(Vector3 target, float duration = 0.4f, System.Action onArrived = null)
    {
        DOTween.Kill(transform);

        transform.DOMove(target, duration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => onArrived?.Invoke());
    }

    private void Leave(Dish dish, bool success)
    {
        int value = 0;
        Transform reaction = null;

        if (!success)
        {
            if (dish == null)
            {
                Debug.Log("Клиент ушёл недовольным! (не осталось блюд)");
                reaction = _angryReaction;
            }
            else
            {
                if (PlayerEntity.Instance.TryAddResourceValue(5))
                {
                    value = 5;
                    Debug.Log($"Клиент ушёл недовольным! Хотел {_wantedDish}, а получил {dish.Type}");
                }
                reaction = _sadReaction;
            }
        }
        else
        {
            if (PlayerEntity.Instance.TryAddResourceValue(10))
            {
                value = 10;
                Debug.Log($"Клиент ушёл довольный! Получил {_wantedDish}");
            }
            reaction = _happyReaction;
        }

        PlayState.Instance.AddValue(value);

        // --- Анимация реакции перед уходом ---
        if (reaction != null)
        {
            reaction.gameObject.SetActive(true);
            reaction.localPosition = Vector3.zero;
            reaction.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            seq.Append(reaction.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
            seq.Join(reaction.DOLocalMoveY(1.5f, 0.6f).SetEase(Ease.OutCubic));
            seq.AppendInterval(0.2f); 
            seq.OnComplete(() =>
            {
                DOTween.Kill(transform); // убиваем твины объекта
                _onLeave?.Invoke(this);  // теперь сигналим сервису → очередь сдвинется
                Destroy(gameObject);
            });
        }
        else
        {
            // fallback: если реакции нет
            DOTween.Kill(transform);
            _onLeave?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
