using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "TextureConfig", menuName = "Config/TextureConfig")]
public class TextureConfig : Config, ISerializationCallbackReceiver
{
    [Header("Папки для автозаполнения")]
    [FolderPath] public string TileFolder;
    [FolderPath] public string DishFolder;

    [Header("Ингредиенты (TileType)")]
    public List<TileTextureData> TileTextures;

    [Header("Блюда (DishType)")]
    public List<DishTextureData> DishTextures;

    private Dictionary<Enums.TileType, TileTextureData> _tileDictionary;
    private Dictionary<Enums.DishType, DishTextureData> _dishDictionary;

    public override IEnumerator Init()
    {
        _tileDictionary = new Dictionary<Enums.TileType, TileTextureData>();
        _dishDictionary = new Dictionary<Enums.DishType, DishTextureData>();

        foreach (var data in TileTextures)
            if (!_tileDictionary.ContainsKey(data.TileType))
                _tileDictionary.Add(data.TileType, data);

        foreach (var data in DishTextures)
            if (!_dishDictionary.ContainsKey(data.DishType))
                _dishDictionary.Add(data.DishType, data);

        yield return null;
    }

    #region Get Data
    public bool TryGetTextureData(Enums.TileType type, out TileTextureData data)
        => _tileDictionary.TryGetValue(type, out data);

    public bool TryGetTextureData(Enums.DishType type, out DishTextureData data)
        => _dishDictionary.TryGetValue(type, out data);

    public bool TryGetTexture(Enums.TileType type, out Sprite texture)
    {
        texture = null;
        if (_tileDictionary.TryGetValue(type, out var data))
        {
            texture = data.TextureTile;
            return texture != null;
        }
        return false;
    }

    public bool TryGetTexture(Enums.DishType type, out Sprite texture)
    {
        texture = null;
        if (_dishDictionary.TryGetValue(type, out var data))
        {
            texture = data.TextureDish;
            return texture != null;
        }
        return false;
    }
    #endregion

    #region Serialization
    public void OnAfterDeserialize() { }

    public void OnBeforeSerialize()
    {
        SyncTileList();
        SyncDishList();
    }

    public void SyncTileList()
    {
        var enumValues = (Enums.TileType[])Enum.GetValues(typeof(Enums.TileType));

        if (TileTextures == null)
            TileTextures = new List<TileTextureData>();

        // делаем словарь по TileType (чтобы не потерять ссылки на Sprite)
        var dict = new Dictionary<Enums.TileType, TileTextureData>();
        foreach (var data in TileTextures)
            if (!dict.ContainsKey(data.TileType))
                dict[data.TileType] = data;

        // пересобираем список ровно по enum'ам
        var newList = new List<TileTextureData>();
        foreach (var type in enumValues)
        {
            if (dict.TryGetValue(type, out var existing))
            {
                existing.TileType = type;
                existing.Name = type.ToString();
                newList.Add(existing);
            }
            else
            {
                newList.Add(new TileTextureData
                {
                    TileType = type,
                    Name = type.ToString()
                });
            }
        }

        TileTextures = newList;
    }

    public void SyncDishList()
    {
        var enumValues = (Enums.DishType[])Enum.GetValues(typeof(Enums.DishType));

        if (DishTextures == null)
            DishTextures = new List<DishTextureData>();

        var dict = new Dictionary<Enums.DishType, DishTextureData>();
        foreach (var data in DishTextures)
            if (!dict.ContainsKey(data.DishType))
                dict[data.DishType] = data;

        var newList = new List<DishTextureData>();
        foreach (var type in enumValues)
        {
            if (dict.TryGetValue(type, out var existing))
            {
                existing.DishType = type;
                existing.Name = type.ToString();
                newList.Add(existing);
            }
            else
            {
                newList.Add(new DishTextureData
                {
                    DishType = type,
                    Name = type.ToString()
                });
            }
        }

        DishTextures = newList;
    }
    #endregion

#if UNITY_EDITOR
    public void AutoAssignSprites<TEnum, TData>(
    List<TData> list,
    Func<TData, TEnum> getType,
    Action<TData, Sprite> setSprite,
    string folderPath
)
    {
        if (string.IsNullOrEmpty(folderPath)) return;
        if (!AssetDatabase.IsValidFolder(folderPath)) return;

        var guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
        var spriteList = new List<Sprite>(); 

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) spriteList.Add(sprite);
        }

        // Для каждого элемента списка
        foreach (var item in list)
        {
            string key = getType(item).ToString().ToLower();
            Sprite assigned = null;

            // Начинаем проверку от полной длины к 3 символам
            for (int len = key.Length; len >= 3; len--)
            {
                string subKey = key.Substring(0, len);

                foreach (var sprite in spriteList)
                {
                    if (sprite.name.ToLower().Contains(subKey))
                    {
                        assigned = sprite;
                        break;
                    }
                }

                if (assigned != null)
                    break; // нашли спрайт — выходим
            }

            if (assigned != null)
                setSprite(item, assigned);
        }
    }
#endif
}


[System.Serializable]
public class TileTextureData
{
    public Enums.TileType TileType;
    public string Name;
    public Sprite TextureTile;
}

[System.Serializable]
public class DishTextureData
{
    public Enums.DishType DishType;
    public string Name;
    public Sprite TextureDish;
}