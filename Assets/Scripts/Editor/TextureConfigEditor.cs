#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TextureConfig))]
public class TextureConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Auto Assign Sprites"))
        {
            var config = (TextureConfig)target;
             
            // Автозаполнение спрайтов
            config.AutoAssignSprites(config.TileTextures, t => t.TileType, (t, s) => t.TextureTile = s, config.TileFolder);
            config.AutoAssignSprites(config.DishTextures, d => d.DishType, (d, s) => d.TextureDish = s, config.DishFolder);

            EditorUtility.SetDirty(config);
        }
    }
}
#endif
