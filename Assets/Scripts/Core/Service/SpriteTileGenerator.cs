using UnityEngine;

public class SpriteTileGenerator : MonoBehaviour
{
    [Header("Настройки сетки")]
    public Sprite sprite;
    [Range(1, 50)] public int rows = 5;
    [Range(1, 50)] public int cols = 5;
    public float cellSize = 1f;

    [HideInInspector] public Transform container;

    public void RebuildGrid()
    {
        if (sprite == null) return;

        // ищем или создаём контейнер
        if (container == null)
        {
            var existing = transform.Find("GridContainer");
            container = existing != null ? existing : new GameObject("GridContainer").transform;
            container.SetParent(transform);
            container.localPosition = Vector3.zero;
        }

#if UNITY_EDITOR
        // очищаем контейнер
        while (container.childCount > 0)
            DestroyImmediate(container.GetChild(0).gameObject);
#else
        foreach (Transform child in container)
            Destroy(child.gameObject);
#endif

        // создаём сетку
        float offsetX = (cols - 1) * cellSize / 2f;
        float offsetY = (rows - 1) * cellSize / 2f;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                GameObject go = new GameObject($"Tile_{x}_{y}");
                go.transform.SetParent(container);
                go.transform.localPosition = new Vector3(x * cellSize - offsetX, y * cellSize - offsetY, 0);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
            }
        }
    }
}
