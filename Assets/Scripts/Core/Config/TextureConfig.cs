using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TextureConfig", menuName = "Config/TextureConfig")]
public class TextureConfig : Config, ISerializationCallbackReceiver
{
    public List<TextureData> TextureData;
    private Dictionary<Enums.TileType, TextureData> _dictionary; 

    public override IEnumerator Init()
    {
        _dictionary = new Dictionary<Enums.TileType, TextureData>();

        foreach (var data in TextureData)
        {
            if (!_dictionary.ContainsKey(data.TileType))
            {
                _dictionary.Add(data.TileType, data);
            }
        }

        yield return null;
    }

    public bool TryGetTextureData(Enums.TileType type, out TextureData data)
    {
        data = null;

        if (_dictionary.TryGetValue(type, out data))
        {
            return true;
        }

        return false;
    }

    public void OnAfterDeserialize()
    {

    }

    public void OnBeforeSerialize()
    {
        var enumValues = (Enums.TileType[])Enum.GetValues(typeof(Enums.TileType));

        // Если список пуст или размер не совпадает — пересобираем
        if (TextureData == null || TextureData.Count < enumValues.Length)
            TextureData = new List<TextureData>();

        for (int i = 0; i < enumValues.Length; i++)
        {
            // Если элемента нет — добавляем новый
            if (i >= TextureData.Count)
            {
                TextureData.Add(new TextureData
                {
                    TileType = enumValues[i],
                    Name = enumValues[i].ToString()
                });
            }
            else
            {
                // Синхронизируем значение TileType в случае изменения порядка enum
                TextureData[i].TileType = enumValues[i];
                TextureData[i].Name = enumValues[i].ToString();
            }
        }

        // Если enum уменьшился — удаляем лишние элементы
        if (TextureData.Count > enumValues.Length)
            TextureData.RemoveRange(enumValues.Length, TextureData.Count - enumValues.Length);
    }

    public bool TryGetTexture(Enums.TileType type, out Sprite texture)
    {
        texture = null;

        foreach (var data in TextureData)
        {
            if (data.TileType == type)
            {
                texture = data.Texture;
                break;
            }
        }

        return texture != null;
    }
}

[System.Serializable]
public class TextureData
{
    public Enums.TileType TileType;
    public string Name;
    public Sprite Texture;
}