using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemBase", menuName = "Scriptable Objects/Newitem")]
public class ItemBase : ScriptableObject, ISerializationCallbackReceiver
{
    [ReadOnlyInspector] public string KEY_ID;
    public ItemData ItemData;

    public bool Buy(Action onFinish)
    {
        if (ItemData.TryBuy(out var item))
        {
            PlayerEntity.Instance.AddItem(item, onFinish);

            return true;
        }

        return false;
    }

    public void OnAfterDeserialize()
    {

    }

    public void OnBeforeSerialize()
    {
        if (this != null)
        {
            if (!string.IsNullOrEmpty(name))
            {
                KEY_ID = name;
            }
        }
    }
}
