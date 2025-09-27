using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

public class ServingWindow : MonoBehaviour
{
    [SerializeField] FlyIcon _flyIcon;

    private PlayState _state;
    public event Action<List<Dish>> OnServingUpdate;
    private List<Dish> _readyDishes = new();
    private PlayState.PlayStatus _status;
    private WorldHorizontalLayout _layout;
    private bool _isForceTaken;

    public bool IsFree => _currentInGameCountDishes < 5;

    private int _currentInGameCountDishes => _readyDishes.Count + _currentIconsFly.Count;
    private List<FlyIcon> _currentIconsFly;

    private Queue<(Dish dish, Vector3 pos, Action onComplete)> _dishQueue = new();
    private bool _processingQueue = false;

    public void Init(PlayState state)
    {
        _state = state;
        _state.PlayStatusChanged += OnPlayStatusChange;
        _layout = GetComponent<WorldHorizontalLayout>();
        _currentIconsFly = new List<FlyIcon>();
    }

    private void OnPlayStatusChange(PlayState.PlayStatus status)
    {
        _status = status;
    }

    public void EnqueueDish(Dish dish, Vector3 mergePos, Action onComplete = null)
    {
        _dishQueue.Enqueue((dish, mergePos, onComplete));

        if (!_processingQueue)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue()
    {
        _processingQueue = true;

        _isForceTaken = false;

        while (_dishQueue.Count > 0)
        {
            var (dish, spawnPos, onComplete) = _dishQueue.Peek();

            // Ждём, пока появится свободный слот
            DishSlot slot = null;
            yield return new WaitUntil(() =>
            { 
                return !_isForceTaken && !_layout.IsBusy;
            });

            yield return new WaitUntil(() =>
            {
                slot = _layout.GetNextSlot();
                return slot != null;
            }); 

            // Создаём FlyIcon
            var flyIcon = Instantiate(_flyIcon);
            flyIcon.InvokeFly(dish.Icon);
            flyIcon.transform.position = spawnPos;
            flyIcon.transform.localScale = Vector3.zero;
            _currentIconsFly.Add(flyIcon);

            // Ждём завершения полёта
            yield return flyIcon.PlayFlyWorld(slot.transform.position, 0.15f).WaitForCompletion();

            // Добавляем в layout → ждём анимацию scale + перестройку
            yield return StartCoroutine(_layout.AddObjectRoutine(dish, flyIcon.transform, () =>
            {
                OnFlyComplete(dish, flyIcon, onComplete);
            }));

            // Убираем из очереди
            _dishQueue.Dequeue();
        }

        _processingQueue = false;
    }
     
    private void OnFlyComplete(Dish dish, FlyIcon icon, Action onComplete)
    {
        _readyDishes.Add(dish);
        _currentIconsFly.Remove(icon);
        onComplete?.Invoke();

        // Если превысили лимит, можно форсировать сброс
        if (_currentInGameCountDishes >= 5)
        {
            _isForceTaken = true;

            _state.ForceTakeDish(CompleteForceTake);
        }
    }

    void CompleteForceTake() => _isForceTaken = false; 

    public IEnumerator TryTakeDish(Enums.DishType dishType, Action<Enums.DishType, Dish> onComplete)
    {
        if (_status != PlayState.PlayStatus.play || _readyDishes.Count == 0)
        {
            onComplete?.Invoke(dishType, null);
            yield break;
        }

        // FIFO
        var dish = _readyDishes[0];
        _readyDishes.RemoveAt(0);

        yield return _layout.RemoveObjectRoutine(dish);

        if (_readyDishes.Count == 0)
            _state.SetTableValue(true);

        onComplete?.Invoke(dishType, dish);
    }


    public void Finish()
    {
        foreach (var icon in _currentIconsFly)
        {
            icon.CancelFly();
        }
    }

}