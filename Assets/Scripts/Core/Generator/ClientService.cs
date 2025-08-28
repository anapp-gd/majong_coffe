using System.Collections.Generic;
using UnityEngine;

public class ClientService : MonoBehaviour
{
    private Transform _spawnPoint;
    [SerializeField] private Client clientPrefab; 
    [SerializeField] private float _spawnInterval = 5f;
    [SerializeField] private int _maxClients = 5;

    private float _clientDelayTake = 5f;

    private PlayState _state;
    private float _timer;
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
        _state.BoardCleanChange += BoardClean;
    } 

    void BoardClean()
    {
        foreach (var client in _clients)
        {
            client.TakeDish();
        }

        _spawnInterval = 0f;
        _clientDelayTake = 0f; 
    }

    void OnPlayStatusChange(PlayState.PlayStatus status)
    {
        _status = status;
    }

    private void Update()
    {
        if (_status != PlayState.PlayStatus.play) return;

        if (_availableDishes == null || _availableDishes.Count == 0)
            return;

        // ���� ������� ������ � ���
        if (_clients.Count >= _maxClients)
            return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            SpawnClient();
            _timer = _spawnInterval;
        }
    }

    private void SpawnClient()
    {
        var dish = GetRandomDish();

        Vector3 spawnPos = _spawnPoint.position;

        spawnPos.y += 5f;
        spawnPos.x += Random.Range(-5, 5);

        var client = Instantiate(clientPrefab, spawnPos, Quaternion.identity); 

        client.Init(_clientDelayTake, dish, _state.ServingWindow, OnClientLeft);

        _clients.Add(client);

        Debug.Log($"����� ������ �������: {dish}");
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

        Debug.Log("������ ����, ������������ ����� � �������");
    }
}
