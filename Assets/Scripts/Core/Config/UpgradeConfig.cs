using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeConfig", menuName = "Config/UpgradeConfig")]
public class UpgradeConfig : Config, ISerializationCallbackReceiver
{
    public List<ItemBase> Items;

    public override IEnumerator Init()
    {
        yield return null;
    }

    public void OnAfterDeserialize()
    { 
    }

    public void OnBeforeSerialize()
    {
        if (Items != null && Items.Count > 2)
        {
            Items.Sort((a, b) => a.ItemData.Level.CompareTo(b.ItemData.Level));
        }
    }
}
 