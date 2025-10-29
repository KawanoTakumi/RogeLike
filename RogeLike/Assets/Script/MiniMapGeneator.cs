using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MiniMapGenerator : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Tile tileScript;     // Tile.cs（ダンジョン生成スクリプト）
    [SerializeField] private RawImage miniMapImage; // CanvasのRawImage
    [SerializeField] private RectTransform playerIcon; // プレイヤーアイコン（UIのImage）
    [SerializeField] public Transform player;// プレイヤー

    [Header("色設定")]
    [SerializeField] private Color floorColor = new(0.8f, 0.8f, 0.8f);
    [SerializeField] private Color wallColor = new(0.2f, 0.2f, 0.2f);
    [SerializeField] private Color stairsColor = new(0.2f, 0.8f, 1.0f); // ← 水色
    [SerializeField] private Color emptyColor = new(0, 0, 0, 0);

    private Texture2D mapTexture;
    private Vector3Int min;
    private Vector3Int max;
    private RectTransform mapRect;
    private Tilemap tilemap;

    void Start()
    {
        mapRect = miniMapImage.rectTransform;
        tilemap = tileScript.tilemap;
    }

    void Update()
    {
        if (player != null)
            UpdatePlayerIcon();
    }

    /// <summary>
    /// ミニマップ画像を生成
    /// </summary>
    public void GenerateMiniMap()
    {
        if (!tilemap) return; 
        min = tilemap.cellBounds.min;
        max = tilemap.cellBounds.max;

        int width = max.x - min.x;
        int height = max.y - min.y;

        mapTexture = new Texture2D(width, height);
        mapTexture.filterMode = FilterMode.Point;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new(min.x + x, min.y + y, 0);
                TileBase tile = tilemap.GetTile(pos);
                Color color = emptyColor;

                if (tile == null)
                    color = emptyColor;
                else if (tile == tileScript.stairsTile)
                    color = stairsColor;
                else if (tile == Tile.FTILENAME)
                    color = floorColor;
                else
                    color = wallColor;

                mapTexture.SetPixel(x, height - 1 - y, color);
            }
        }

        mapTexture.Apply();
        miniMapImage.texture = mapTexture;

        miniMapImage.uvRect = new Rect(0, 1, 1, -1);
    }

    void UpdatePlayerIcon()
    {
        if (!mapTexture) GenerateMiniMap();

        Vector3Int cell = tilemap.WorldToCell(player.position);
        float mapX = cell.x - min.x;
        float mapY = cell.y - min.y;
        mapX += 0.5f;
        mapY += 0.5f;

        float normalizedX = mapX / mapTexture.width;
        float normalizedY = mapY / mapTexture.height;

        float px = (normalizedX * mapRect.rect.width) - (mapRect.rect.width / 2f);
        float py = (normalizedY * mapRect.rect.height) - (mapRect.rect.height / 2f);

        playerIcon.anchoredPosition = new Vector2(px, py);
    }
}
