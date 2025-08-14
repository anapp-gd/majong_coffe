using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    public int width = 10;
    public int height = 6;
    public int layers = 3;
    public int symbolTypes = 12; // сколько разных картинок
    public float fillPercent = 0.8f; // 0..1
    public LayoutStyle layoutStyle = LayoutStyle.Cluster;

    public enum LayoutStyle
    {
        Cluster,    // кучкой
        Grid,       // сетка
        RandomSpread // разбросано
    }
}
