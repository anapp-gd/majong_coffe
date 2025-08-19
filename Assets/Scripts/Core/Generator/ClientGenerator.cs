using System.Collections.Generic;
using UnityEngine;

public class ClientGenerator : MonoBehaviour
{
    [SerializeField] private GameObject clientPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int maxClients = 5;

    private PlayState _state;
    private float _timer;
    private List<Client> _clients = new List<Client>();

    private HashSet<Enums.DishType> _availableDishes;

    public void Init(PlayState state, HashSet<Enums.DishType> availableDishes)
    {
        _state = state;
        _availableDishes = availableDishes;
    }

    private void Update()
    {
        if (_availableDishes == null || _availableDishes.Count == 0)
            return;

        // если очередь полная — ждём
        if (_clients.Count >= maxClients)
            return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            SpawnClient();
            _timer = spawnInterval;
        }
    }

    private void SpawnClient()
    {
        var dish = GetRandomDish();

        var go = Instantiate(clientPrefab, spawnPoint.position, Quaternion.identity);
        var client = go.GetComponent<Client>();
        if (client == null)
            client = go.AddComponent<Client>();

        client.Init(dish, OnClientLeft);

        _clients.Add(client);

        Debug.Log($"Новый клиент заказал: {dish}");
    }

    private Enums.DishType GetRandomDish()
    {
        int index = Random.Range(0, _availableDishes.Count);
        int i = 0;
        foreach (var dish in _availableDishes)
        {
            if (i == index) return dish;
            i++;
        }
        return Enums.DishType.FriedEgg; // fallback
    }

    private void OnClientLeft(Client client)
    {
        if (_clients.Contains(client))
            _clients.Remove(client);

        Debug.Log("Клиент ушёл, освободилось место в очереди");
    }
}
