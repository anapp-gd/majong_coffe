using UnityEngine;

public class Client : MonoBehaviour
{
    public Enums.DishType WantedDish { get; private set; }

    [SerializeField] private float waitTime = 10f; // сколько ждёт клиент

    private float _timer;
    private System.Action<Client> _onLeave;

    public void Init(Enums.DishType dish, System.Action<Client> onLeave)
    {
        _timer = waitTime;
        WantedDish = dish;
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
        Leave(dish == WantedDish);
    }

    private void Leave(bool success)
    {
        // TODO Animation leave and score
        if (!success)
            Debug.Log("Клиент ушёл недовольным!");
        else
            Debug.Log("Клиент ушел довольный!");

        _onLeave?.Invoke(this);
        Destroy(gameObject);
    }
}
