using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerControl : MonoBehaviour
{
    public float moveCooldown = 0.2f;
    private float lastMoveTime;
    Tilemap tilemap;
    private Vector2 moveInput;
    private Move inputActions;
    public static bool Player_Moving = false;
    void Awake()
    {
        inputActions = new Move();
    }

    private void Start()
    {
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        //タイルマップを読み込む
        tilemap = Tile.Save_maps;
        Player_Moving = true;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();
    void Update()
    {
        if (Time.time - lastMoveTime < moveCooldown || moveInput == Vector2.zero) return;

        if(Player_Moving)
        {
            Debug.Log("プレイヤー移動可能");
            Vector3Int direction = Vector3Int.RoundToInt(moveInput);
            Vector3Int currentCell = tilemap.WorldToCell(transform.position);
            Vector3Int targetCell = currentCell + direction;
            Vector3 targetWorld = tilemap.CellToWorld(targetCell) + new Vector3(0.5f, 0.5f, 0);
            TileBase targetTile = tilemap.GetTile(targetCell);

            if (targetTile != null && IsWalkableTile(targetTile))
            {
                Debug.Log("プレイヤーのターン");
                transform.position = targetWorld;
                Exit.playerPos = transform.position;
                lastMoveTime = Time.time;
                moveInput = Vector2.zero;
                Player_Moving = false;

                foreach (var enemy in Tile.allEnemys)
                {
                    enemy.Enemy_Moving = true;
                    StartCoroutine(EnemyTurnRoutine());
                }

            }
        }
    }
    public IEnumerator EnemyTurnRoutine()
    {

        foreach (var enemy in Tile.allEnemys)
        {
            enemy.ExecuteTurn();
            yield return new WaitForSeconds(0f);
        }
        Player_Moving = true;
    }

    bool IsWalkableTile(TileBase tile)
    {
        // 通行可能なTileだけを許可（例：FloorTile, PathTileなど）
        return tile.name == "Tile 001_0"|| tile.name == "Exit_0";
    }
}