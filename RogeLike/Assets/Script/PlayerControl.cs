using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerControl : MonoBehaviour
{
    public float moveCooldown = 0.2f;
    private float lastMoveTime;
    Tilemap tilemap;
    private Vector2 moveInput;
    private Move inputActions;
    
    void Awake()
    {
        inputActions = new Move();
    }

    private void Start()
    {
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        //タイルマップを読み込む
        tilemap = Tile.Save_maps;

        //Turnmanager.Instance.currentTurn = TurnState.PlayerTurn;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();
    void Update()
    {
        //プレイヤーのターンの場合
        //if (Turnmanager.Instance.currentTurn == TurnState.PlayerTurn)
        //{

        //    Turnmanager.Instance.EndPlayerTurn();
        //}

        if (Time.time - lastMoveTime < moveCooldown || moveInput == Vector2.zero) return;

        Vector3Int direction = Vector3Int.RoundToInt(moveInput);
        Vector3Int currentCell = tilemap.WorldToCell(transform.position);
        Vector3Int targetCell = currentCell + direction;
        Vector3 targetWorld = tilemap.CellToWorld(targetCell) + new Vector3(0.5f, 0.5f, 0);
        TileBase targetTile = tilemap.GetTile(targetCell);

        if (targetTile != null && IsWalkableTile(targetTile))
        {
            transform.position = targetWorld;
            Exit.playerPos = transform.position;
            lastMoveTime = Time.time;
            moveInput = Vector2.zero;
        }
    }

    bool IsWalkableTile(TileBase tile)
    {
        // 通行可能なTileだけを許可（例：FloorTile, PathTileなど）
        return tile.name == "Tile 001_0"|| tile.name == "Exit_0";
    }
}
