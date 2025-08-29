using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEntity : SourceEntity
{
    public static PlayerEntity Instance;
    public int GetResource
    {
        get
        {
            return _metaResouceValue;
        }

    }
    private int _metaResouceValue;
    public int GetCurrentLevel
    {
        get
        {
            return _currentLevel;
        }
    }
    private int _currentLevel; 

    public List<ItemData> Data; 

    private const int _maxBalance = 100000; 

    public PlayerEntity()
    {
        Instance = this;
    }

    public override void Init()
    {
        Load();
    }

    public void Load()
    { 
        Data = new();

        var saveData = SaveModule.Load<SaveData>();

        if (saveData.itemsData.Count > 0)
        {
            foreach (var item in saveData.itemsData)
            {
                Data.Add(new ItemData(item.Level, item.Type, item.Cost));
            }
        }

        _currentLevel = saveData.Level;
        _metaResouceValue = saveData.MetaResources;
    }

    public void Save()
    {
        List<ItemSaveData> itemsData = new List<ItemSaveData>();

        foreach (var item in Data)
        {
            itemsData.Add(new ItemSaveData()
            {
                Cost = item.Cost,
                Level = item.Level,
                Type = item.Type,
            });
        }

        var data = new SaveData()
        {
            Level = _currentLevel,
            MetaResources = _metaResouceValue,
            itemsData = itemsData
        };

        SaveModule.Save(data);
    }

    public void AddItem(ItemData item)
    {
        MenuState.Instance.BuyItem(item.Type);

        Data.Add(item);

        Save();
    } 

    public void SetNextLevel()
    {
        int nextLevel = _currentLevel + 1;

        var levelConfig = ConfigModule.GetConfig<LevelConfig>();

        if (levelConfig.TryGetLevelData(nextLevel, out LevelData data))
        {
            _currentLevel = nextLevel;

            Save();
        } 
    }

    public bool TrySubResourceValue(int value)
    {
        if (value <= 0)
        {
            Debug.LogWarning($"[{nameof(PlayerEntity)}] TrySubResourceValue: value must be > 0, got {value}");
            return false;
        }

        if (_metaResouceValue < value)
        {
            // недостаточно средств
            Debug.Log($"[{nameof(PlayerEntity)}] Not enough funds: need {value}, have {_metaResouceValue}");
            return false;
        }

        _metaResouceValue -= value;
        PlayerPrefs.SetInt("resource", _metaResouceValue);
        ObserverEntity.Instance.UpdatePlayerMetaResourceChanged(_metaResouceValue);
        Save();
        return true;
    }

    public bool TryAddResourceValue(int value)
    {
        if (value <= 0)
        {
            Debug.LogWarning($"[{nameof(PlayerEntity)}] TryAddResourceValue: value must be > 0, got {value}");
            return false;
        }

        if (_metaResouceValue >= _maxBalance)
        { 
            Debug.Log($"[{nameof(PlayerEntity)}] Balance already at cap {_maxBalance}");
            return false;
        }

        // защищаемся от переполнения и капаем по _maxBalance
        long sum = (long)_metaResouceValue + value;
        int newBalance = sum >= _maxBalance ? _maxBalance : (int)sum;

        if (newBalance == _metaResouceValue)
            return false; // ничего не изменилось

        _metaResouceValue = newBalance;
        PlayerPrefs.SetInt("resource", _metaResouceValue);
        ObserverEntity.Instance.UpdatePlayerMetaResourceChanged(_metaResouceValue);
        Save();
        return true;
    }
}