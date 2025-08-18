using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    private LevelData data;

    private void OnEnable()
    {
        data = (LevelData)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("Редактор базового слоя", EditorStyles.boldLabel);

        int width = data.width;
        int height = data.height;

        for (int y = 0; y < height; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                int value = data.baseLayer[index];

                // Цвет квадратика
                Color prevColor = GUI.backgroundColor;
                GUI.backgroundColor = value == 0 ? Color.gray : Color.green;

                if (GUILayout.Button("", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    data.baseLayer[index] = (value + 1) % 2; // переключение 0 ↔ 1
                    EditorUtility.SetDirty(data);
                }

                GUI.backgroundColor = prevColor; // вернуть цвет
            }
            GUILayout.EndHorizontal();
        }
    }
}
