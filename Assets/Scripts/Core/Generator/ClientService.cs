using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private List<Client> _clients = new();
    private List<Enums.DishType> _orders = new();
    private PlayState.PlayStatus _status;
    private HashSet<Enums.DishType> _availableDishes;

    private bool _inTaken;

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

        var orders = _state.GetTilesInOrder();

        orders.Reverse();

        foreach (var order in orders)
        { 
            if (DishMapping.TryGetDish(order.First.TileType, out var dish))
            {
                _orders.Add(dish);
            }
        }
    }

    void OnPlayStatusChange(PlayState.PlayStatus status)
    {
        _status = status;
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
    public void Finish(Action callback)
    {
        StartCoroutine(FinishRoutine(callback));
    }

    IEnumerator FinishRoutine(Action callback)
    {
        int index = 0;

        while (index < _clients.Count && _orders.Count > 0)
        {
            var client = _clients[index];

            if (client != null)
            {
                // Ждём завершения анимации забора блюда
                yield return client.FinishTakeDish(_orders[0]);

                // После завершения убираем заказ из очереди
                _orders.RemoveAt(0);
            }

            index++; // Переходим к следующему клиенту
        }

        UpdateUI();

        callback?.Invoke();
    }


    private void Update()
    {
        if (_status != PlayState.PlayStatus.play) return;
        if (_availableDishes == null || _availableDishes.Count == 0) return;
        if (_inTaken) return;

        HandleSpawning();
        HandleTaking();
    }

    void HandleTaking()
    {
        if (_clients.Count == 0 || _orders.Count == 0) return;

        _takeTimer -= Time.deltaTime;
        if (_takeTimer <= 0f)
        {
            StartCoroutine(HandleTakenRoutine());
            _takeTimer = _clientDelayTake;
        }
    }
    IEnumerator HandleTakenRoutine()
    {
        _inTaken = true;

        if (_clients.Count > 0 && _orders.Count > 0)
        {
            var firstClient = _clients[0];

            if (firstClient != null)
            {
                // Ждём, пока корутина TryTakeDish завершит удаление из layout и анимации
                yield return firstClient.TryTakeDish(_orders[0]);

                // После завершения удаляем заказ и обновляем UI
                _orders.RemoveAt(0);
                UpdateUI();
            }
        }

        _inTaken = false;
    }

    public void ForceTakeDish(Action callback)
    {
        _inTaken = true;

        _spawnTimer = _spawnInterval;

        StartCoroutine(ForceTakeDishRoutine(callback));
    }

    IEnumerator ForceTakeDishRoutine(Action callback = null)
    {
        int index = 0;

        while (index < _clients.Count && _orders.Count > 0)
        {
            var client = _clients[index];

            if (client != null)
            {
                // Ждём, пока корутина TryTakeDish завершит удаление блюда и анимацию
                yield return client.TryTakeDish(_orders[0]);

                // После завершения удаления убираем заказ
                _orders.RemoveAt(0);

                index++;
            }
            else
            {
                index++;
            }
        }

        _takeTimer = _clientDelayTake;
        UpdateUI();

        callback?.Invoke();

        _inTaken = false;
    }

    private void SpawnClient()
    { 
        Vector3 spawnPos = _spawnPoint.position + Vector3.right * (_clients.Count * _queueSpacing);

        var client = Instantiate(clientPrefab, spawnPos, Quaternion.identity);
        client.Init(_state.ServingWindow, OnClientLeft);

        _clients.Add(client);

        UpdateUI(); 
    }

    private Enums.DishType GetRandomDish()
    {
        var tiles = _state.GetAvaiablesTiles();
         
        int index = UnityEngine.Random.Range(0, tiles.Count);
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
                // Берём первые 3 заказа из _orders
                var dishes = _orders
                    .Take(3)
                    .ToList();

                panel.GetWindow<TipWindow>().UpdateSlots(dishes);
            }
        }
    }
}