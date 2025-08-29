 using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteTileGenerator))]
public class SpriteTileGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpriteTileGenerator generator = (SpriteTileGenerator)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Rebuild Grid"))
        {
            generator.RebuildGrid();
        }
    }
}
