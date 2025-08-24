using System;

public class ObserverEntity : SourceEntity
{
    public static ObserverEntity Instance;
    public event Action<int> PlayerMetaResourceChanged; 
    public event Action<float, int, int> UpgradeProgreesChanged; 

    public ObserverEntity()
    {
        Instance = this;
    }
      
    public override void Init()
    {

    }

    public void UpdatePlayerMetaResourceChanged(int value)
    {
        PlayerMetaResourceChanged?.Invoke(value);
    } 
} 