using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Client : MonoBehaviour
{
    [SerializeField] private Sprite[] _views;
    [SerializeField] private Transform _happyReaction;
    [SerializeField] private Transform _angryReaction;
    [SerializeField] private Transform _sadReaction; 
    private ServingWindow _window;
    private System.Action<Client> _onLeave;

    public void Init(ServingWindow window, System.Action<Client> onLeave)
    { 
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

    public IEnumerator FinishTakeDish(Enums.DishType targetDish)
    {
        yield return _window.TryTakeDish(targetDish, takeDish); 
    }

    public IEnumerator TryTakeDish(Enums.DishType targetDish)
    {
        yield return _window.TryTakeDish(targetDish, takeDish); 
    }

    void takeDish(Enums.DishType targetDish, Dish dish)
    {
        if (dish == null)
        {
            Leave(null, false); 
        }
        else
        {
            if (targetDish == dish.Type)
            {
                Leave(dish, success: true);
            }
            else
            {
                Leave(dish, success: false);
            }
        } 
    }
     
    public void MoveToQueuePosition(Vector3 target, float duration = 0.4f, System.Action onArrived = null)
    {
        DOTween.Kill(transform);

        transform.DOMove(target, duration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                onArrived?.Invoke();
            });
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
                    Debug.Log($"Клиент ушёл недовольным!");
                }
                reaction = _sadReaction;
            }
        }
        else
        {
            if (PlayerEntity.Instance.TryAddResourceValue(10))
            {
                value = 10;
                Debug.Log($"Клиент ушёл довольный!");
            }
            reaction = _happyReaction;
        }

        PlayState.Instance.AddValue(value); 

        if (reaction != null)
        {
            reaction.gameObject.SetActive(true);
            reaction.localPosition = Vector3.zero;
            reaction.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            seq.Append(reaction.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
            seq.Join(reaction.DOLocalMoveY(1.5f, 0.6f).SetEase(Ease.OutCubic));
            seq.AppendInterval(0.2f);
            seq.OnComplete(Cleanup);
        }
        else
        {
            Cleanup();
        }

        void Cleanup()
        {
            if (this != null && transform != null) DOTween.Kill(transform); 
            gameObject.SetActive(false);
            _onLeave?.Invoke(this);  
        }
    } 
}