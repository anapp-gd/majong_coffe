using UnityEngine;
public abstract class ItemBase : ScriptableObject, ISerializationCallbackReceiver
{
    [ReadOnlyInspector] public string KEY_ID;
    public abstract void Buy();

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
