using System.Collections.Generic;
using UnityEngine;

public class ClientService : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Client clientPrefab;
    [SerializeField] private float _spawnInterval = 1f;
    [SerializeField] private int _maxClients = 5;
    [SerializeField] private float _queueSpacing = 2f; // расстояние между клиентами

    private float _takeTimer = 1f;
    private float _clientDelayTake = 1f;

    private PlayState _state;
    private float _spawnTimer = 1f;
    private List<Client> _clients = new List<Client>();
    private PlayState.PlayStatus _status;
    private HashSet<Enums.DishType> _availableDishes;

    public void Init(PlayState state, HashSet<Enums.DishType> availableDishes)
    {
        _state = state;
        _state.PlayStatusChanged += OnPlayStatusChange;
        _availableDishes = availableDishes;
        _spawnPoint = GameObject.FindWithTag("SpawnPoint").transform;

        var gameConfig = ConfigModule.GetConfig<GameConfig>();

        _spawnInterval = gameConfig.ClientSpawnDelay;
        _maxClients = gameConfig.MaxClientCount;
        _clientDelayTake = gameConfig.ClientTakeDelay; 
    }

    public void Finish()
    {
        foreach (var client in _clients)
        {
            client.FinishTakeDish();
        }
    }
    void OnPlayStatusChange(PlayState.PlayStatus status)
    {
        _status = status;
    }

    private void Update()
    {
        if (_status != PlayState.PlayStatus.play) return;
        if (_availableDishes == null || _availableDishes.Count == 0) return;

        HandleSpawning();
        HandleTaking();
    }

    private void HandleSpawning()
    {
        if (_clients.Count >= _maxClients) return;

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            SpawnClient();
            _spawnTimer = _spawnInterval;
        }
    }

    private void HandleTaking()
    {
        if (_clients.Count == 0) return;

        _takeTimer -= Time.deltaTime;
        if (_takeTimer <= 0f)
        {
            // всегда первый в очереди пытается взять
            var firstClient = _clients[0];
            if (firstClient != null)
            {
                firstClient.TryTakeDish(); // у клиента метод, который решает "получилось/нет"
            }

            _takeTimer = _clientDelayTake;
        }
    }

    private void SpawnClient()
    {
        var dish = GetRandomDish();

        // позиция в очереди: SpawnPoint - первый, дальше с отступами
        Vector3 spawnPos = _spawnPoint.position + Vector3.right * (_clients.Count * _queueSpacing);

        var client = Instantiate(clientPrefab, spawnPos, Quaternion.identity);
        client.Init(dish, _state.ServingWindow, OnClientLeft); // убираем локальный таймер

        _clients.Add(client);

        UpdateUI();
        Debug.Log($"Новый клиент заказал: {dish}");
    }

    private Enums.DishType GetRandomDish()
    {
        var tiles = _state.GetAvaiablesTiles();
         
        int index = Random.Range(0, tiles.Count);
        int i = 0;

        Enums.DishType dish = Enums.DishType.FriedEgg;

        foreach (var tile in tiles)
        {
            if (i == index)
            {
                if (DishMapping.TryGetDish(tile, out dish))
                {
                    break;
                } 
            }

            i++;
        }


        return dish;
    }

    private void OnClientLeft(Client client)
    {
        if (_clients.Contains(client))
            _clients.Remove(client);

        // после ухода смещаем всех вперёд
        for (int i = 0; i < _clients.Count; i++)
        {
            Vector3 targetPos = _spawnPoint.position + Vector3.right * (i * _queueSpacing);
            _clients[i].MoveToQueuePosition(targetPos);
        }

        UpdateUI();
        Debug.Log("Клиент ушёл, очередь сдвинулась");
    }

    private void UpdateUI()
    {
        if (UIModule.TryGetCanvas<PlayCanvas>(out var playCanvas))
        {
            if (playCanvas.TryGetPanel<PlayPanel>(out var panel))
            {
                var dishes = new List<Enums.DishType>();
                foreach (var c in _clients)
                    dishes.Add(c.WantedType);

                panel.GetWindow<TipWindow>().UpdateSlots(dishes);
            }
        }
    }
}
