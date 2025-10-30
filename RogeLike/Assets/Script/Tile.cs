using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
public class Tile : MonoBehaviour
{
    public Tilemap tilemap;//�^�C���}�b�v
    //���^�C��
    public TileBase[] floorTile;
    private int ftile_number;
    public static TileBase FTILENAME;
    //�ǃ^�C��
    public TileBase[] wallTile;
    private int wtile_number;

    public TileBase stairsTile;//�K�i�^�C��
    public GameObject playerPrefab;//�v���C���[
    public GameObject[] enemyPrefab;//�G
    public GameObject[] bossPrefab;//�{�X
    public GameObject[] itemPrefab;//�A�C�e��
    public GameObject Exit_Obj;//�K�i
    public int mapSize = 40;
    public static int MAP_SIZE = 0;//�}�b�v�̑傫��(�c�����䗦)
    public int roomCount = 8;//�����̐�
    public int roomMinSize = 6;//�����̍ŏ��̑傫��
    public int roomMaxSize = 12;//�����̍ő�̑傫��

    private List<Room> rooms = new();
    public static List<Enemys> allEnemys = new();
    public static Tilemap Save_maps;
    public static int MAX_FLOOR = 5;
    public static bool Boss_Flag = false;
    Room selectRoom;
    public MiniMapGenerator maping;
    void Start()
    {
        //�}�b�v�^�C���ݒ�(���[���h�}�b�v����_���W�����ɓ�������ݒ�)
        SelectTileMaps();

        //�}�b�v�̑傫����ݒ�
        MAP_SIZE = mapSize;
        Save_maps = tilemap;
        //�}�b�v�𐶐�
        if(Exit.Now_Floor < MAX_FLOOR)
        {
            //��������
            GenerateRooms();
            //�L������
            ConnectRooms();
            //�J�����𕔉��̒��S�ɐ���
            CenterCameraOnFirstRoom();
            //�J�����̈ʒu�Ƀv���C���[����
            CameraToPlayer();
            //�e�����ɓG�𐶐�
            SpawnEnemies(rooms);
            //�e�����ɃA�C�e������
            SpawnItems(rooms);
        }
        else
        {
            //�{�X��������
            GenerateBossRoom();
            //�J�����𕔉��̒��S�ɐ���
            CenterCameraOnFirstRoom();
            //�J�����̈ʒu�Ƀv���C���[����
            CameraToPlayer();
            Boss_Flag = true;
        }

        DrawWalls();
        //�K�i�^�C������
        ExitTileGenerate();
        //�~�j�}�b�v����
        maping = GameObject.Find("Grid").GetComponent<MiniMapGenerator>();
        maping.player = GameObject.Find("Player(Clone)").transform;
        maping.GenerateMiniMap();
    }
    //�J�����̈ʒu�Ƀv���C���[�𐶐�
    public void CameraToPlayer()
    {
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3Int gridPos = tilemap.WorldToCell(cameraPos);
        Vector3 spawnWorldPos = tilemap.CellToWorld(gridPos) + new Vector3(0.5f, 0.5f, 0); // �����ɕ␳
        Instantiate(playerPrefab, spawnWorldPos, Quaternion.identity);
    }

    //�^�C���摜�ݒ�X�N���v�g+�K�w��ݒ�
    void SelectTileMaps()
    {
        //���񎞂̂ݐݒ�
        if(Exit.Now_Floor == 1)
        {
            ftile_number = UnityEngine.Random.Range(0, floorTile.Count()-1);
            wtile_number = UnityEngine.Random.Range(0, 1);
            MAX_FLOOR = UnityEngine.Random.Range(3, 4 + Exit.Clear_Dungeon);//�N���A�񐔂�������ΊK�w�𑝂₷
            //8�ȏ�͖���
            if (MAX_FLOOR > 6)
                MAX_FLOOR = 6;
        }
        FTILENAME = floorTile[ftile_number];
    }

    //���������X�N���v�g
    void GenerateRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
            int w = UnityEngine.Random.Range(roomMinSize, roomMaxSize);
            int h = UnityEngine.Random.Range(roomMinSize, roomMaxSize);
            int x = UnityEngine.Random.Range(0, MAP_SIZE - w);
            int y = UnityEngine.Random.Range(0, MAP_SIZE - h);
            RectInt rect = new RectInt(x, y, w, h);
            Room room = new Room(rect);
            rooms.Add(room);
            DrawRoom(room);
        }
    }
    //�����̕`��֐�
    void DrawRoom(Room room)
    {
        for (int x = room.rect.xMin; x < room.rect.xMax; x++)
        {
            for (int y = room.rect.yMin; y < room.rect.yMax; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), floorTile[ftile_number]);
            }
        }
    }

    //�ǂ̕`��
    void DrawWalls()
    {
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (tilemap.GetTile(pos) == floorTile[ftile_number])
                {
                    // ����8�������`�F�b�N
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
                            tilemap.SetTile(neighborPos, wallTile[wtile_number]);
                        }
                    }
                }
            }
        }
    }
    //�����ڑ��֐�
    void ConnectRooms()
    {
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Vector2Int from = rooms[i].Center;
            Vector2Int to = rooms[i + 1].Center;
            CreateCorridor(from, to);
        }
    }
    //�e�^�C���̃R���C�_�[�쐬
    void CreateCorridor(Vector2Int from, Vector2Int to)
    {
        Vector2Int current = from;
        while (current.x != to.x)
        {
            current.x += (to.x > current.x) ? 1 : -1;
            tilemap.SetTile(new Vector3Int(current.x, current.y, 0), floorTile[ftile_number]);
        }
        while (current.y != to.y)
        {
            current.y += (to.y > current.y) ? 1 : -1;
            tilemap.SetTile(new Vector3Int(current.x, current.y, 0), floorTile[ftile_number]);
        }
    }

    //�J�����̈ʒu���ŏ��ɕ`�悵�������ɐݒ�
    void CenterCameraOnFirstRoom()
    {
        if (rooms.Count > 0)
        {
            Vector2Int center = rooms[0].Center;
            Camera.main.transform.position = new Vector3(center.x, center.y, Camera.main.transform.position.z);
        }
    }
    //�G��z�u����֐�
    void SpawnEnemies(List<Room> rooms)
    {
        foreach (Room room in rooms)
        {
            int max_rooms_enemy = UnityEngine.Random.Range(0,5);
            for (int i = 0; i < max_rooms_enemy; i++) // �e������3�̏o��
            {
                // �����_���ȓG��ނ�I��
                int enemyIndex = UnityEngine.Random.Range(0, enemyPrefab.Length);
                // �����_���Ȉʒu�i�����͈͓̔��j���擾
                Vector2Int randomPos = new Vector2Int(
                    UnityEngine.Random.Range(room.rect.x + 1, room.rect.x + room.rect.width - 1),
                    UnityEngine.Random.Range(room.rect.y + 1, room.rect.y + room.rect.height - 1)
                );
                //���W���^�C�����W�ɐݒ�A����
                Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(randomPos.x, randomPos.y, 0)) + new Vector3(0.5f, 0.5f, 0);
                GameObject Enemy = Instantiate(enemyPrefab[enemyIndex], worldPos, Quaternion.identity);
                allEnemys.Add(Enemy.GetComponent<Enemys>());
                Enemys Enemy_status = Enemy.GetComponent<Enemys>();
            }
        }
    }

    //�����̃N���X��ݒ�
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
    //�{�X�����𐶐�
    void GenerateBossRoom()
    {
        int bossRoomSize = 10;

        // �}�b�v���Ɏ��܂�悤�Ƀ����_���Ȉʒu������
        int x = UnityEngine.Random.Range(1, MAP_SIZE - bossRoomSize - 1);
        int y = UnityEngine.Random.Range(1, MAP_SIZE - bossRoomSize - 1);

        // ���������쐬
        RectInt bossRect = new RectInt(x, y, bossRoomSize, bossRoomSize);
        Room bossRoom = new Room(bossRect);
        rooms.Add(bossRoom);

        // �����`��i���^�C���j
        for (int i = bossRect.xMin; i < bossRect.xMax; i++)
        {
            for (int j = bossRect.yMin; j < bossRect.yMax; j++)
            {
                tilemap.SetTile(new Vector3Int(i, j, 0), floorTile[ftile_number]);
            }
        }

        // �Ǖ`��i����8�����j
        for (int i = bossRect.xMin - 1; i <= bossRect.xMax; i++)
        {
            for (int j = bossRect.yMin - 1; j <= bossRect.yMax; j++)
            {
                Vector3Int pos = new Vector3Int(i, j, 0);
                if (tilemap.GetTile(pos) == null)
                {
                    tilemap.SetTile(pos, wallTile[wtile_number]);
                }
            }
        }

        // �{�X�z�u�i�����j
        Vector2Int center = bossRoom.Center;
        Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(center.x, center.y+3, 0)) + new Vector3(0.5f, 0.5f, 0);
        GameObject boss = Instantiate(bossPrefab[UnityEngine.Random.Range(0,bossPrefab.Count() -1)], worldPos, Quaternion.identity);
        allEnemys.Add(boss.GetComponent<Enemys>());
    }

    //�K�i�^�C������
    public void ExitTileGenerate()
    {
        //�����̂ǂ����Ɏ��̊K�w�ւ̃^�C����z�u(�v���C���[��0�Ԗڂ̕����ɕK������)
        if (!Boss_Flag)
        {
            selectRoom = rooms[UnityEngine.Random.Range(1, rooms.Count)];//���̊K�w�ɍs���镔�����쐬
        }
        else
            selectRoom = rooms[0];//�{�X�̕����Ȃ�0�Ԗ�
        //���͂W�}�X�ɂ����U����悤�ɐݒ�
        Vector2Int center = selectRoom.Center;
        center.x += UnityEngine.Random.Range(-1, 1);
        center.y += UnityEngine.Random.Range(-1, 1);
        Vector3Int cellPos = new Vector3Int(center.x, center.y, 0);
        tilemap.SetTile(cellPos, stairsTile);
        Vector3 ExitWorldPos = tilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0); // �����ɕ␳
        Instantiate(Exit_Obj, ExitWorldPos, Quaternion.identity);
        Exit.exitCell = ExitWorldPos;

    }

    //�A�C�e�������֐�
    void SpawnItems(List<Room> rooms)
    {
        foreach (Room room in rooms)
        {
            for (int x = room.rect.xMin + 1; x < room.rect.xMax - 1; x++)
            {
                for (int y = room.rect.yMin + 1; y < room.rect.yMax - 1; y++)
                {
                    if (UnityEngine.Random.Range(0, 20) == 0)
                    {
                        Vector3Int cellPos = new Vector3Int(x, y, 0);
                        if (tilemap.GetTile(cellPos) == floorTile[ftile_number])
                        {
                            GameObject item = itemPrefab[UnityEngine.Random.Range(0, itemPrefab.Length)];
                            Vector3 worldPos = tilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
                            Instantiate(item, worldPos, Quaternion.identity);
                        }
                    }
                }
            }
        }
    }
}