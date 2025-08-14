using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Размер сетки (x, y) и слои (z)")]
    public Vector2Int gridSize = new Vector2Int(10, 8); // x,y
    [Range(1, 10)] public int layers = 3;               // z (0..layers-1)

    [Header("Контент")]
    [Min(1)] public int pairsCount = 24;                // общее число пар
    [Min(1)] public int tileTypes = 12;                 // количество типов
    public Sprite[] tileSpritesPool;                    // общий пул визуалов (>= tileTypes)

    [Header("Разрешённая зона слоя Z=0")]
    // Белое/прозрачное = разрешено, чёрное = запрещено; размер подгоняется под gridSize
    public Texture2D allowedMask;

    [Header("Сложность/стиль раскладки")]
    [Tooltip("0 — всё свободно, 1 — много боковых блокировок (плотная кладка)")]
    [Range(0f, 1f)] public float blockiness = 0.55f;

    [Tooltip("Вероятность, что второй тайл пары будет поставлен рядом (усиливает читаемость в начале)")]
    [Range(0f, 1f)] public float pairNearness = 0.75f;

    [Tooltip("Максимум попыток при поиске позиции/валидации")]
    public int maxTries = 2000;

    // Формирует bool[,] из allowedMask. Если mask нет — вся плоскость разрешена.
    public bool[,] BuildAllowedZone()
    {
        var zone = new bool[gridSize.x, gridSize.y];
        if (allowedMask == null)
        {
            for (int x = 0; x < gridSize.x; x++)
                for (int y = 0; y < gridSize.y; y++)
                    zone[x, y] = true;
            return zone;
        }

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                // семплим маску в нормированных координатах
                float u = (x + 0.5f) / gridSize.x;
                float v = (y + 0.5f) / gridSize.y;
                var c = allowedMask.GetPixelBilinear(u, v);
                zone[x, y] = c.grayscale > 0.5f || c.a < 0.5f;
            }
        }
        return zone;
    }
}
