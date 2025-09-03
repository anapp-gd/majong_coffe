using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

public static class SaveModule
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save<T>(T data)
    {
        try
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(SavePath, json);
#if UNITY_EDITOR
            Debug.Log($"[SaveModule] Saved to: {SavePath}");
#endif
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveModule] Save error: {ex.Message}");
        }
    }

    public static T Load<T>() where T : new()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
#if UNITY_EDITOR
                Debug.Log("[SaveModule] No save file found. Creating new save.");
#endif
                return new T();
            }

            string json = File.ReadAllText(SavePath);
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveModule] Load error: {ex.Message}. Returning new instance.");
            return new T();
        }
    }

    public static void Delete()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
#if UNITY_EDITOR
                Debug.Log("[SaveModule] Save deleted.");
#endif
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveModule] Delete error: {ex.Message}");
        }
    }
}


[System.Serializable]
public class SaveData
{
    public int Level;
    public int MetaResources;
    public bool IsSound;
    public bool IsMusic;
    public bool IsVibro;
    public List<ItemSaveData> itemsData;
    public SaveData()
    {
        Level = 0;
        MetaResources = 0; 
        itemsData = new List<ItemSaveData>();
    }
}

[System.Serializable]
public class ItemSaveData
{
    public int Level;
    public Enums.ItemType Type;
    public int Cost;
}