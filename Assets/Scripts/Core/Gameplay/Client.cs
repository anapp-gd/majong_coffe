using UnityEngine;

public class Client : MonoBehaviour
{
    [SerializeField] private float waitTime = 10f; // сколько ждёт клиент

    private Enums.DishType _wantedDish;
    private ServingWindow _window;
    private float _timer;
    private System.Action<Client> _onLeave;

    public void Init(Enums.DishType dish, ServingWindow window, System.Action<Client> onLeave)
    {
        _timer = waitTime;
        _wantedDish = dish;
        _window = window;
        _onLeave = onLeave;

    }

    private void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            TakeDish();
        }
    }
    void TakeDish()
    {
        if (_window.TryTakeDish(_wantedDish, out Dish dish))
        {
            if (_wantedDish == dish.Type)
            {
                // Клиент получил именно то, что хотел
                Leave(dish, success: true);
            }
            else
            {
                // Клиент получил другое блюдо
                Leave(dish, success: false);
            }
        }
        else
        {
            // Вообще не получилось взять блюдо
            Leave(null, success: false);
        }
    }

    void Leave(Dish dish, bool success)
    {
        // TODO: анимация ухода + подсчет очков
        if (!success)
        {
            if (dish == null)
                Debug.Log("Клиент ушёл недовольным! (не осталось блюд)");
            else
            {
                if (PlayerEntity.Instance.TryAddResourceValue(5))
                { 
                    Debug.Log($"Клиент ушёл недовольным! Хотел {_wantedDish}, а получил {dish.Type}");
                }
            }
        }
        else
        {
            if (PlayerEntity.Instance.TryAddResourceValue(10))
            {
                Debug.Log($"Клиент ушёл довольный! Получил {_wantedDish}"); 
            }
        }

        _onLeave?.Invoke(this);
        Destroy(gameObject);
    }
}
