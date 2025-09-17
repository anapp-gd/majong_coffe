using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(FolderPathAttribute))]
public class FolderPathDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var path = property.stringValue;

        EditorGUI.BeginChangeCheck();
        var newObj = EditorGUI.ObjectField(position, label,
            AssetDatabase.LoadAssetAtPath<DefaultAsset>(path), typeof(DefaultAsset), false);

        if (EditorGUI.EndChangeCheck())
        {
            if (newObj != null)
            {
                var newPath = AssetDatabase.GetAssetPath(newObj);
                if (AssetDatabase.IsValidFolder(newPath))
                    property.stringValue = newPath;
            }
        }

        EditorGUI.EndProperty();
    }
}
#endif