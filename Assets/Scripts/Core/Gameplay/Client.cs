using UnityEngine;

public class Client : MonoBehaviour
{
    public Enums.DishType WantedType => _wantedDish;
    private Enums.DishType _wantedDish;
    private ServingWindow _window;
    private float _timer;
    private System.Action<Client> _onLeave;

    public void Init(float wait, Enums.DishType dish, ServingWindow window, System.Action<Client> onLeave)
    {
        _wantedDish = dish;
        _window = window;
        _onLeave = onLeave; 
        _timer = wait;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            TakeDish();
        }
    }
    public void TakeDish()
    {
        if (_window.TryTakeDish(_wantedDish, out Dish dish))
        {
            if (_wantedDish == dish.Type)
            {
                // ������ ������� ������ ��, ��� �����
                Leave(dish, success: true);
            }
            else
            {
                // ������ ������� ������ �����
                Leave(dish, success: false);
            }
        }
        else
        {
            // ������ �� ���������� ����� �����
            Leave(null, success: false);
        }
    }

    void Leave(Dish dish, bool success)
    {
        // TODO: �������� ����� + ������� �����

        int value = 0;

        if (!success)
        {
            if (dish == null)
                Debug.Log("������ ���� �����������! (�� �������� ����)");
            else
            {
                if (PlayerEntity.Instance.TryAddResourceValue(5))
                { 
                    value = 5;
                    Debug.Log($"������ ���� �����������! ����� {_wantedDish}, � ������� {dish.Type}");
                }
            }
        }
        else
        {
            if (PlayerEntity.Instance.TryAddResourceValue(10))
            {
                value = 10;
                Debug.Log($"������ ���� ���������! ������� {_wantedDish}"); 
            }
        }

        PlayState.Instance.AddValue(value);
        _onLeave?.Invoke(this);
        Destroy(gameObject);
    }
}
