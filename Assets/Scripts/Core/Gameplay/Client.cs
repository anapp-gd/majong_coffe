using UnityEngine;

public class Client : MonoBehaviour
{
    public Enums.DishType WantedDish { get; private set; }

    [SerializeField] private float waitTime = 10f; // сколько ждёт клиент

    private float _timer;
    private System.Action<Client> _onLeave;

    public void Init(Enums.DishType dish, System.Action<Client> onLeave)
    {
        WantedDish = dish;
        _timer = waitTime;
        _onLeave = onLeave;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            Leave(false);
        }
    }

    public void TakeDish(Enums.DishType dish)
    {
        if (dish == WantedDish)
        {
            Debug.Log($"Клиент доволен, получил {dish}!");
            Leave(true);
        }
        else
        {
            Debug.Log($"Клиент недоволен, ожидал {WantedDish}, а получил {dish}");
            Leave(false);
        }
    }

    private void Leave(bool success)
    {
        // Тут можно триггерить анимацию ухода, начисление очков и т.д.
        if (!success)
            Debug.Log("Клиент ушёл недовольным!");

        _onLeave?.Invoke(this);
        Destroy(gameObject);
    }
}
