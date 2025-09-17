#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(SourceSlot), true)]
public class SourceSlotEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // ������������ ��������� ���������
        DrawDefaultInspector();

        var slot = (SourceSlot)target;
        var button = slot.GetComponent<Button>();

        if (slot.IsButton)
        {
            if (button == null)
            {
                // ����� �������, ���� �������� �������
                slot.gameObject.AddComponent<Button>();
            }
        }
        else
        {
            if (button != null)
            {
                // ������� ���������
                DestroyImmediate(button, false);
            }
        }
    }
}
#endif