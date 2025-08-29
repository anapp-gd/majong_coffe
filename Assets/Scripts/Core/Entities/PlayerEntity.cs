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

    public int GetCurrentUpgrade
    {
        get
        {
            return _currentUpgrade;
        }
    }

    public List<ItemData> Data;

    private int _currentUpgrade;

    private const int _maxBalance = 100000; 

    public PlayerEntity()
    {
        Instance = this;
    }

    public override void Init()
    {
        if (PlayerPrefs.HasKey("level"))
        {
            _currentLevel = PlayerPrefs.GetInt("level");
        }
        else
        {
            _currentLevel = 0;
        }

        if (PlayerPrefs.HasKey("resource"))
        {
            _metaResouceValue = PlayerPrefs.GetInt("resource");
        }
        else
        {
            _metaResouceValue = 0;
        }

        if (PlayerPrefs.HasKey("upgrade"))
        {
            _currentUpgrade = PlayerPrefs.GetInt("upgrade");
        }
        else
        {
            _currentUpgrade = -1;
        }

        Data = new ();

        var saveData = SaveModule.Load<SaveData>();

        if (saveData.itemsData.Count > 0)
        {

            foreach (var item in saveData.itemsData)
            {
                Data.Add(new ItemData(item.Level, item.Type, item.Cost));
            }
        }
    }

    public void AddItem(ItemData item)
    {
        MenuState.Instance.BuyItem(item.Type);

        Data.Add(item);
    } 

    public void SetNextLevel()
    {
        int nextLevel = _currentLevel + 1;

        var levelConfig = ConfigModule.GetConfig<LevelConfig>();

        if (levelConfig.TryGetLevelData(nextLevel, out LevelData data))
        {
            _currentLevel = nextLevel;

            PlayerPrefs.SetInt("level", _currentLevel);
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
        return true;
    }
}