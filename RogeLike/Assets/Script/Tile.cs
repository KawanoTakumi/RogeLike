using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class Tile : MonoBehaviour
{
    public Tilemap tilemap;//�^�C���}�b�v
    public TileBase floorTile;//���^�C��
    public TileBase wallTile;//�ǃ^�C��
    public TileBase stairsTile;//�K�i�^�C��
    public GameObject playerPrefab;//�v���C���[
    public GameObject[] enemyPrefab;//�G
    public GameObject Exit_Obj;//�K�i
    public static int MAP_SIZE = 40;//�}�b�v�̑傫��(�c�����䗦)
    public int roomCount = 4;//�����̐�
    public int roomMinSize = 6;//�����̍ŏ��̑傫��
    public int roomMaxSize = 12;//�����̍ő�̑傫��

    private List<Room> rooms = new();
    public static Tilemap Save_maps;


    void Start()
    {
        Save_maps = tilemap;
        //�}�b�v����
        GenerateRooms();
        ConnectRooms();
        DrawWalls();
        CenterCameraOnFirstRoom();
        //�J�����̈ʒu�Ƀv���C���[�𐶐�
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3Int gridPos = tilemap.WorldToCell(cameraPos);
        Vector3 spawnWorldPos = tilemap.CellToWorld(gridPos) + new Vector3(0.5f, 0.5f, 0); // �����ɕ␳

        Instantiate(playerPrefab, spawnWorldPos, Quaternion.identity);
        Room selectedRoom = rooms[Random.Range(0, rooms.Count)];//���̊K�w�ɍs���镔�����쐬
        //�����̂ǂ����Ɏ��̊K�w�ւ̃^�C����z�u
        Vector2Int center = selectedRoom.Center;
        Vector3Int cellPos = new Vector3Int(center.x, center.y,0);
        tilemap.SetTile(cellPos, stairsTile);
        Vector3 ExitWorldPos = tilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0); // �����ɕ␳
        Instantiate(Exit_Obj, ExitWorldPos, Quaternion.identity);
        Exit.exitCell = ExitWorldPos;
        SpawnEnemies(rooms);

    }


    //���������X�N���v�g
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
    //�����̕`��֐�
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
                if (tilemap.GetTile(pos) == floorTile)
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
                            tilemap.SetTile(neighborPos, wallTile);
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
            tilemap.SetTile(new Vector3Int(current.x, current.y, 0), floorTile);
        }
        while (current.y != to.y)
        {
            current.y += (to.y > current.y) ? 1 : -1;
            tilemap.SetTile(new Vector3Int(current.x, current.y, 0), floorTile);
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
            for (int i = 0; i < 3; i++) // �e������3�̏o��
            {
                // �����_���ȓG��ނ�I��
                int enemyIndex = Random.Range(0, enemyPrefab.Length);
                // �����_���Ȉʒu�i�����͈͓̔��j���擾
                Vector2Int randomPos = new Vector2Int(
                    Random.Range(room.rect.x + 1, room.rect.x + room.rect.width - 1),
                    Random.Range(room.rect.y + 1, room.rect.y + room.rect.height - 1)
                );

                Vector2Int center = room.Center;

                Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(randomPos.x, randomPos.y, 0)) + new Vector3(0.5f, 0.5f, 0);
                Instantiate(enemyPrefab[enemyIndex], worldPos, Quaternion.identity);

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
}