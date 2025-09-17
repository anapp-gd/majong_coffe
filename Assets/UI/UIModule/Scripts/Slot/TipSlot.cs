using UnityEngine;
using UnityEngine.UI;

public class TipSlot : SourceSlot
{
    public Enums.DishType Data;

    [SerializeField] Image _dishIcon;
    [SerializeField] Image _tileIcon;

    public override void OnActive()
    { 
    }

    public override void OnClick()
    { 
    }

    public override void UpdateView()
    {
        var textureConfig = ConfigModule.GetConfig<TextureConfig>();

        if (textureConfig.TryGetTexture(Data, out Sprite sprite))
        {
            _dishIcon.sprite = sprite;
        }
         
        if (DishMapping.TryGetTile(Data, out Enums.TileType tile) && textureConfig.TryGetTexture(tile, out Sprite texture))
        {
            _tileIcon.sprite = texture;
        }

        gameObject.SetActive(true);
    }
}
