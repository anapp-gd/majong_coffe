using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeConfig", menuName = "Config/UpgradeConfig")]
public class UpgradeConfig : Config
{
    public List<ItemBase> Items;

    public override IEnumerator Init()
    {
        yield return null;
    }
}


public class LevelInfo
{

}