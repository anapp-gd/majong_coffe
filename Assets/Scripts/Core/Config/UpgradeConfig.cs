using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeConfig", menuName = "Config/UpgradeConfig")]
public class UpgradeConfig : Config
{
    public List<LevelInfo> Items;

    public override IEnumerator Init()
    {
        yield return null;
    }

    public LevelInfo GetStartLevelUpgrade()
    {
        return Items[0];
    }

    public bool TryGetUpgrade(int level, out LevelInfo levelInfo)
    {
        levelInfo = null;

        if (level < Items.Count)
        {
            levelInfo = Items[level];
        }

        return levelInfo != null;
    }

    public int GetMaxLevelUpgrade()
    {
        return Items.Last().Level;
    }
}

[Serializable]
public class LevelInfo
{
    public int Cost;
    public int Level;
}