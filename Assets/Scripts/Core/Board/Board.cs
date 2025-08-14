using UnityEngine;

public class Board : MonoBehaviour
{
    public TileView tilePrefab;
    public int pairsCount = 10;
    public float tileSize = 1.1f;

    public void Init()
    {
        var tiles = MadjongGenerator.Generate(pairsCount);

        // 1. Определяем размеры сетки
        int maxX = 0;
        int maxY = 0;
        foreach (var t in tiles)
        {
            if (t.position.x > maxX) maxX = t.position.x;
            if (t.position.y > maxY) maxY = t.position.y;
        }

        // 2. Вычисляем смещение (центрируем)
        float offsetX = -(maxX * tileSize) / 2f;
        float offsetY = -(maxY * tileSize) / 2f;

        // 3. Спавн с учётом смещения
        foreach (var t in tiles)
        {
            Vector3 worldPos = new Vector3(
                t.position.x * tileSize + offsetX,
                t.position.y * tileSize + offsetY,
                0 // для 2D
            );

            TileView tile = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);

            Renderer rend = tile.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = Color.HSVToRGB(t.type / 5f, 0.8f, 1f);
        }
    }
}
