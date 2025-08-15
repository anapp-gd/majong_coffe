using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] TileView tileView;
    [SerializeField] int layersCount = 3;          // Кол-во слоёв
    [SerializeField] int pairsCount = 10;          // Кол-во пар плиток
    [SerializeField] int tileTypesCount = 5;       // Кол-во типов плиток
    [SerializeField] float tileSize = 1f;          // Размер одной плитки
    [SerializeField, Range(0f, 1f)] float lowerLayerDarken = 0.5f; // Затемнение нижних слоёв
    private int currentLayer;

    public void Init(PlayState state)
    {
        currentLayer = layersCount;

        var tiles = MadjongGenerator.Generate(layersCount, pairsCount, tileTypesCount);

        // Чтобы всё было по центру
        int maxX = 0, maxY = 0;
        foreach (var t in tiles)
        {
            if (t.position.x > maxX) maxX = t.position.x;
            if (t.position.y > maxY) maxY = t.position.y;
        }

        float offsetX = -(maxX * tileSize) / 2f;
        float offsetY = -(maxY * tileSize) / 2f;

        foreach (var data in tiles)
        {
            Vector3 worldPos = new Vector3(
                data.position.x * tileSize + offsetX,
                data.position.y * tileSize + offsetY,
                0f // Z всегда 0, всё контролируем через sortingOrder
            );

            var tile = Instantiate(tileView, worldPos, Quaternion.identity, transform); 
            tile.GridPos = data.position;
            tile.LayerIndex = data.layer; // Запоминаем слой
            tile.Init(state);

            var spriteRenderer = tile.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                // Чем меньше индекс слоя (верхние слои), тем больше порядок
                spriteRenderer.sortingOrder = layersCount - data.layer;// (layersCount - t.position.z) * orderPerLayer + (int)(-t.position.y);

                // Красим для теста
                spriteRenderer.color = Color.HSVToRGB((data.type % 12) / 12f, 0.8f, 1f);

                // Затемняем нижние слои
                if (spriteRenderer.sortingOrder < currentLayer)
                {
                    spriteRenderer.color *= lowerLayerDarken;
                }
            }
        }
    }
}
