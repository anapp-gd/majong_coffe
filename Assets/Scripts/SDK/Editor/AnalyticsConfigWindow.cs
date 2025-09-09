using UnityEditor;
using UnityEngine;

public class AnalyticsConfigWindow : EditorWindow
{
    private AnalyticsConfig config;

    [MenuItem("Tools/Analytics Config")]
    public static void ShowWindow()
    {
        GetWindow<AnalyticsConfigWindow>("Analytics Config");
    }

    private void OnEnable()
    {
        config = Resources.Load<AnalyticsConfig>("AnalyticsConfig");
        if (config == null)
        {
            // если нет Ч создаЄм
            config = CreateInstance<AnalyticsConfig>();
            if (!System.IO.Directory.Exists("Assets/Resources"))
                System.IO.Directory.CreateDirectory("Assets/Resources");

            AssetDatabase.CreateAsset(config, "Assets/Resources/AnalyticsConfig.asset");
            AssetDatabase.SaveAssets();
        }
    }

    private void OnGUI()
    {
        if (config == null) return;

        SerializedObject so = new SerializedObject(config);
        SerializedProperty prop = so.GetIterator();
        prop.NextVisible(true);

        EditorGUILayout.LabelField("Analytics Keys", EditorStyles.boldLabel);
        while (prop.NextVisible(false))
        {
            EditorGUILayout.PropertyField(prop, true);
        }

        so.ApplyModifiedProperties();
    }
}
