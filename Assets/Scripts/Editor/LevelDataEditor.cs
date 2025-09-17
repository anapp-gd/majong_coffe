using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    private LevelData data;
    private string[] tileOptions;

    private void OnEnable()
    {
        data = (LevelData)target;
        BuildTileOptions();
    }

    // Построение списка для Popup:
    // 0 = пусто, 1 = Random, 2..N = тайлы
    private void BuildTileOptions()
    {
        var enums = Enum.GetNames(typeof(Enums.TileType));
        tileOptions = new string[enums.Length + 2];
        tileOptions[0] = "Empty";   // 0
        tileOptions[1] = "Random";  // 1

        for (int i = 0; i < enums.Length; i++)
            tileOptions[i + 2] = enums[i]; // 2..N
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        DrawAvailableTilesSection();

        GUILayout.Space(10);
        GUILayout.Label("Редактор базового слоя", EditorStyles.boldLabel);

        DrawBaseLayerEditor();
    }

    void DrawAvailableTilesSection()
    {
        GUILayout.Label("Доступные тайлы (уникальные)", EditorStyles.boldLabel);

        if (data.availableTileTypes.Count == 0)
            EditorGUILayout.HelpBox("Список пуст. Добавьте типы тайлов.", MessageType.Info);

        for (int i = 0; i < data.availableTileTypes.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("• " + data.availableTileTypes[i], GUILayout.MaxWidth(200));

            if (GUILayout.Button("Удалить", GUILayout.Width(60)))
            {
                data.availableTileTypes.RemoveAt(i);
                EditorUtility.SetDirty(data);
                BuildTileOptions();
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Добавить тип"))
            ShowAddTileMenu();

        if (GUILayout.Button("Выбрать все"))
            SelectAllTiles();

        if (GUILayout.Button("Очистить"))
        {
            data.availableTileTypes.Clear();
            EditorUtility.SetDirty(data);
            BuildTileOptions();
        }

        EditorGUILayout.EndHorizontal();
    }

    void ShowAddTileMenu()
    {
        var menu = new GenericMenu();
        var used = new HashSet<Enums.TileType>(data.availableTileTypes);

        foreach (Enums.TileType val in Enum.GetValues(typeof(Enums.TileType)))
        {
            if (used.Contains(val))
                menu.AddDisabledItem(new GUIContent(val.ToString()));
            else
                menu.AddItem(new GUIContent(val.ToString()), false, OnAddTileMenu, val);
        }

        menu.ShowAsContext();
    }

    void OnAddTileMenu(object tile)
    {
        data.availableTileTypes.Add((Enums.TileType)tile);
        EditorUtility.SetDirty(data);
        BuildTileOptions();
    }

    void SelectAllTiles()
    {
        data.availableTileTypes.Clear();
        foreach (Enums.TileType val in Enum.GetValues(typeof(Enums.TileType)))
            data.availableTileTypes.Add(val);

        EditorUtility.SetDirty(data);
        BuildTileOptions();
    }

    void DrawBaseLayerEditor()
    {
        int width = data.width;
        int height = data.height;

        int targetSize = width * height;
        while (data.baseLayer.Count < targetSize)
            data.baseLayer.Add(0); // пусто
        while (data.baseLayer.Count > targetSize)
            data.baseLayer.RemoveAt(data.baseLayer.Count - 1);

        for (int y = 0; y < height; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                int current = data.baseLayer[index];

                // Защита от выхода за пределы массива
                if (current < 0 || current >= tileOptions.Length)
                    current = 0;

                int newValue = EditorGUILayout.Popup(current, tileOptions, GUILayout.Width(80));
                if (newValue != current)
                {
                    data.baseLayer[index] = newValue;
                    EditorUtility.SetDirty(data);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
