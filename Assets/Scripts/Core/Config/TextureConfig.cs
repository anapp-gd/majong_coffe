using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TextureConfig", menuName = "Config/TextureConfig")]
public class TextureConfig : Config, ISerializationCallbackReceiver
{
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

    private void SyncTileList()
    {
        var enumValues = (Enums.TileType[])Enum.GetValues(typeof(Enums.TileType));
        if (TileTextures == null || TileTextures.Count < enumValues.Length)
            TileTextures = new List<TileTextureData>();

        for (int i = 0; i < enumValues.Length; i++)
        {
            if (i >= TileTextures.Count)
            {
                TileTextures.Add(new TileTextureData
                {
                    TileType = enumValues[i],
                    Name = enumValues[i].ToString()
                });
            }
            else
            {
                TileTextures[i].TileType = enumValues[i];
                TileTextures[i].Name = enumValues[i].ToString();
            }
        }

        if (TileTextures.Count > enumValues.Length)
            TileTextures.RemoveRange(enumValues.Length, TileTextures.Count - enumValues.Length);
    }

    private void SyncDishList()
    {
        var enumValues = (Enums.DishType[])Enum.GetValues(typeof(Enums.DishType));
        if (DishTextures == null || DishTextures.Count < enumValues.Length)
            DishTextures = new List<DishTextureData>();

        for (int i = 0; i < enumValues.Length; i++)
        {
            if (i >= DishTextures.Count)
            {
                DishTextures.Add(new DishTextureData
                {
                    DishType = enumValues[i],
                    Name = enumValues[i].ToString()
                });
            }
            else
            {
                DishTextures[i].DishType = enumValues[i];
                DishTextures[i].Name = enumValues[i].ToString();
            }
        }

        if (DishTextures.Count > enumValues.Length)
            DishTextures.RemoveRange(enumValues.Length, DishTextures.Count - enumValues.Length);
    }
    #endregion
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