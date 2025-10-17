using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
public class Tile : MonoBehaviour
{
    public Tilemap tilemap;//タイルマップ
    public TileBase floorTile;//床タイル
    public TileBase wallTile;//壁タイル
    public TileBase stairsTile;//階段タイル
    public GameObject playerPrefab;//プレイヤー
    public GameObject[] enemyPrefab;//敵
    public GameObject[] bossPrefab;//ボス
    public GameObject[] itemPrefab;//アイテム
    public GameObject Exit_Obj;//階段
    public int mapSize = 40;
    public static int MAP_SIZE = 0;//マップの大きさ(縦横同比率)
    public int roomCount = 4;//部屋の数
    public int roomMinSize = 6;//部屋の最小の大きさ
    public int roomMaxSize = 12;//部屋の最大の大きさ

    private List<Room> rooms = new();
    public static List<Enemys> allEnemys = new();
    public static Tilemap Save_maps;
    const int MAX_FLOOR = 5;
    void Start()
    {
        //マップの大きさを設定
        MAP_SIZE = mapSize;
        Save_maps = tilemap;
        //マップを生成
        if(Exit.Now_Floor < MAX_FLOOR)
        {
            GenerateRooms();
            ConnectRooms();

            //部屋のどこかに次の階層へのタイルを配置
            Room selectedRoom = rooms[Random.Range(0, rooms.Count)];//次の階層に行ける部屋を作成
            Vector2Int center = selectedRoom.Center;
            Vector3Int cellPos = new Vector3Int(center.x, center.y, 0);
            tilemap.SetTile(cellPos, stairsTile);
            Vector3 ExitWorldPos = tilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0); // 中央に補正
            Instantiate(Exit_Obj, ExitWorldPos, Quaternion.identity);
            Exit.exitCell = ExitWorldPos;

            //各部屋に敵を生成
            SpawnEnemies(rooms);
            //各部屋にアイテム生成
            SpawnItems(rooms);
        }
        else
        {
            GenerateBossRoom();
        }
        //壁を生成
        DrawWalls();
        //カメラを部屋の中心に生成
        CenterCameraOnFirstRoom();

        //カメラの位置にプレイヤーを生成
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3Int gridPos = tilemap.WorldToCell(cameraPos);
        Vector3 spawnWorldPos = tilemap.CellToWorld(gridPos) + new Vector3(0.5f, 0.5f, 0); // 中央に補正
        Instantiate(playerPrefab, spawnWorldPos, Quaternion.identity);
    }

    //部屋生成スクリプト
    void GenerateRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
            int w = Random.Range(roomMinSize, roomMaxSize);
            int h = Random.Range(roomMinSize, roomMaxSize);
            int x = Random.Range(0, MAP_SIZE - w);
            int y = Random.Range(0, MAP_SIZE - h);
            RectInt rect = new RectInt(x, y, w, h);
            Room room = new Room(rect);
            rooms.Add(room);
            DrawRoom(room);
        }
    }
    //部屋の描画関数
    void DrawRoom(Room room)
    {
        for (int x = room.rect.xMin; x < room.rect.xMax; x++)
        {
            for (int y = room.rect.yMin; y < room.rect.yMax; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
            }
        }
    }

    //壁の描画
    void DrawWalls()
    {
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (tilemap.GetTile(pos) == floorTile)
                {
                    // 周囲8方向をチェック
                    foreach (Vector3Int dir in new Vector3Int[] {
                    new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
                    new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
                    new Vector3Int(1, 1, 0), new Vector3Int(-1, -1, 0),
                    new Vector3Int(-1, 1, 0), new Vector3Int(1, -1, 0)
                })
                    {
                        Vector3Int neighborPos = pos + dir;
                        if (tilemap.GetTile(neighborPos) == null)
                        {
                            tilemap.SetTile(neighborPos, wallTile);
                        }
                    }
                }
            }
        }
    }
    //部屋接続関数
    void ConnectRooms()
    {
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Vector2Int from = rooms[i].Center;
            Vector2Int to = rooms[i + 1].Center;
            CreateCorridor(from, to);
        }
    }
    //各タイルのコライダー作成
    void CreateCorridor(Vector2Int from, Vector2Int to)
    {
        Vector2Int current = from;
        while (current.x != to.x)
        {
            current.x += (to.x > current.x) ? 1 : -1;
            tilemap.SetTile(new Vector3Int(current.x, current.y, 0), floorTile);
        }
        while (current.y != to.y)
        {
            current.y += (to.y > current.y) ? 1 : -1;
            tilemap.SetTile(new Vector3Int(current.x, current.y, 0), floorTile);
        }
    }

    //カメラの位置を最初に描画した部屋に設定
    void CenterCameraOnFirstRoom()
    {
        if (rooms.Count > 0)
        {
            Vector2Int center = rooms[0].Center;
            Camera.main.transform.position = new Vector3(center.x, center.y, Camera.main.transform.position.z);
        }
    }
    //敵を配置する関数
    void SpawnEnemies(List<Room> rooms)
    {
        foreach (Room room in rooms)
        {
            for (int i = 0; i < 3; i++) // 各部屋に3体出現
            {
                // ランダムな敵種類を選択
                int enemyIndex = Random.Range(0, enemyPrefab.Length);
                // ランダムな位置（部屋の範囲内）を取得
                Vector2Int randomPos = new Vector2Int(
                    Random.Range(room.rect.x + 1, room.rect.x + room.rect.width - 1),
                    Random.Range(room.rect.y + 1, room.rect.y + room.rect.height - 1)
                );
                //座標をタイル座標に設定、生成
                Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(randomPos.x, randomPos.y, 0)) + new Vector3(0.5f, 0.5f, 0);
                GameObject Enmey = Instantiate(enemyPrefab[enemyIndex], worldPos, Quaternion.identity);
                allEnemys.Add(Enmey.GetComponent<Enemys>());
            }
        }

    }

    //部屋のクラスを設定
    public class Room
    {
        public RectInt rect;
        public Vector2Int Center => new Vector2Int(rect.x + rect.width / 2, rect.y + rect.height / 2);
        public Room(RectInt rect)
        {
            this.rect = rect;
        }
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public bool Contains(Vector2Int pos)
        {
            return pos.x >= X && pos.x < X + Width &&
                   pos.y >= Y && pos.y < Y + Height;
        }
    }
    //ボス部屋を生成
    void GenerateBossRoom()
    {
        int bossRoomSize = 10;

        // マップ内に収まるようにランダムな位置を決定
        int x = Random.Range(1, MAP_SIZE - bossRoomSize - 1);
        int y = Random.Range(1, MAP_SIZE - bossRoomSize - 1);

        // 部屋情報を作成
        RectInt bossRect = new RectInt(x, y, bossRoomSize, bossRoomSize);
        Room bossRoom = new Room(bossRect);
        rooms.Add(bossRoom); // 必要なら別リストにしてもOK

        // 部屋描画（床タイル）
        for (int i = bossRect.xMin; i < bossRect.xMax; i++)
        {
            for (int j = bossRect.yMin; j < bossRect.yMax; j++)
            {
                tilemap.SetTile(new Vector3Int(i, j, 0), floorTile);
            }
        }

        // 壁描画（周囲8方向）
        for (int i = bossRect.xMin - 1; i <= bossRect.xMax; i++)
        {
            for (int j = bossRect.yMin - 1; j <= bossRect.yMax; j++)
            {
                Vector3Int pos = new Vector3Int(i, j, 0);
                if (tilemap.GetTile(pos) == null)
                {
                    tilemap.SetTile(pos, wallTile);
                }
            }
        }

        // ボス配置（中央）
        Vector2Int center = bossRoom.Center;
        Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(center.x, center.y, 0)) + new Vector3(0.5f, 0.5f, 0);
        GameObject boss = Instantiate(bossPrefab[0], worldPos, Quaternion.identity);
        allEnemys.Add(boss.GetComponent<Enemys>());
    }

    //アイテム生成関数
    void SpawnItems(List<Room> rooms)
    {
        foreach (Room room in rooms)
        {
            for (int x = room.rect.xMin + 1; x < room.rect.xMax - 1; x++)
            {
                for (int y = room.rect.yMin + 1; y < room.rect.yMax - 1; y++)
                {
                    // 1/30の確率でアイテムを生成
                    if (Random.Range(0, 30) == 0)
                    {
                        Vector3Int cellPos = new Vector3Int(x, y, 0);
                        if (tilemap.GetTile(cellPos) == floorTile)
                        {
                            GameObject item = itemPrefab[Random.Range(0, itemPrefab.Length)];
                            Vector3 worldPos = tilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
                            Instantiate(item, worldPos, Quaternion.identity);
                        }
                    }
                }
            }
        }
    }

}