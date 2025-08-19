using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

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
        // Стандартные поля
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
    }

    void SelectAllTiles()
    {
        data.availableTileTypes.Clear();
        foreach (Enums.TileType val in Enum.GetValues(typeof(Enums.TileType)))
            data.availableTileTypes.Add(val);

        EditorUtility.SetDirty(data);
    }

    void DrawBaseLayerEditor()
    {
        int width = data.width;
        int height = data.height;

        // Синхронизируем размер списка с width*height
        int targetSize = width * height;
        while (data.baseLayer.Count < targetSize)
            data.baseLayer.Add(0);
        while (data.baseLayer.Count > targetSize)
            data.baseLayer.RemoveAt(data.baseLayer.Count - 1);

        for (int y = 0; y < height; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                int value = data.baseLayer[index];

                Color prevColor = GUI.backgroundColor;
                GUI.backgroundColor = value == 0 ? Color.gray : Color.green;

                if (GUILayout.Button("", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    data.baseLayer[index] = (value + 1) % 2;
                    EditorUtility.SetDirty(data);
                }

                GUI.backgroundColor = prevColor;
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
