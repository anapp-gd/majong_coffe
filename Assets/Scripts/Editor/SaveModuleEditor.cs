#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class SaveModuleEditor
{
    [MenuItem("Tools/Open Save File %#o")] // Ctrl+Shift+O (Win) / Cmd+Shift+O (Mac)
    private static void OpenSaveFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "save.json");

        if (File.Exists(path))
        {
            EditorUtility.RevealInFinder(path); // откроет папку и выделит файл
        }
        else
        {
            Debug.LogWarning("[SaveModuleEditor] Save file not found.");
        }
    }
}
#endif
