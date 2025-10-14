using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    public float moveCooldown = 0.2f;
    private float lastMoveTime;
    Tilemap tilemap;
    private Vector2 moveInput;
    private Move inputActions;
    public static bool Player_Moving = false;
    public CharacterStatus PlayerStatus;
    private Slider PlayerHPBar;//プレイヤー体力バー

    private List<Enemys> nearbyEnemies = new();
    public static CharacterStatus p_status;//個別のステータスに変更
    private int currentTargetIndex = 0;
    private Enemys currentTarget = null;
    private BattleLog Log;
    private Text HP_Text;//体力表示
    void Awake()
    {
        inputActions = new Move();
    }

    private void Start()
    {
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        p_status = Instantiate(PlayerStatus);
        //タイルマップを読み込む
        tilemap = Tile.Save_maps;
        Log = GameObject.Find("BattleLog").GetComponent<BattleLog>();
        Player_Moving = true;
        PlayerHPBar = GameObject.Find("HP").GetComponent<Slider>();
        HP_Text = GameObject.Find("HP_Text").GetComponent<Text>();
        p_status.HP = p_status.maxHP;
        PlayerHPBar.maxValue = p_status.maxHP;
        PlayerHPBar.value = p_status.HP;
        HP_Text.text = p_status.HP + "/" + p_status.maxHP;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();
    void Update()
    {
        UpdateNearbyEnemies();

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (nearbyEnemies.Count > 0)
            {
                currentTargetIndex = (currentTargetIndex + 1) % nearbyEnemies.Count;
                currentTarget = nearbyEnemies[currentTargetIndex];
                HighlightTarget(currentTarget);
            }
            else
            {
                Log.ShowMessage($"no hit enemys !");
            }
        }
        if (Keyboard.current.spaceKey.wasPressedThisFrame && currentTarget != null)
        {

            int damage = Mathf.Max(p_status.attack - currentTarget.Status.diffence, 1);
            currentTarget.TakeDamage(damage);
            //ログ追加
            Log.ShowMessage($" {currentTarget.name} hit {damage} damage");
            moveInput = Vector2.zero;

            Player_Moving = false;
            foreach (var enemy in Tile.allEnemys)
            {
                enemy.Enemy_Moving = true;
            }
            StartCoroutine(EnemyTurnRoutine());
        }
<<<<<<< HEAD
=======

        //移動を制御
        if (Time.time - lastMoveTime < moveCooldown || moveInput == Vector2.zero) return;


        if (Player_Moving)
        {

            Vector3Int direction = Vector3Int.RoundToInt(moveInput);
            Vector3Int currentCell = tilemap.WorldToCell(transform.position);
            Vector3Int targetCell = currentCell + direction;
            Vector3 targetWorld = tilemap.CellToWorld(targetCell) + new Vector3(0.5f, 0.5f, 0);
            TileBase targetTile = tilemap.GetTile(targetCell);

            if (targetTile != null && IsWalkableTile(targetTile) && !IsEnemyOnTarget(targetCell))
            {
                transform.position = targetWorld;
                //移動したら自然回復、ターンごとに１回復
                if(p_status.HP < p_status.maxHP)
                {
                    p_status.HP++;
                    UpdateHPValue();
                }
                Exit.playerPos = transform.position;
                lastMoveTime = Time.time;
                moveInput = Vector2.zero;
                Player_Moving = false;

                foreach (var enemy in Tile.allEnemys)
                {
                    enemy.Enemy_Moving = true;
                }
                StartCoroutine(EnemyTurnRoutine());
            }
        }
    }
    // プレイヤーの移動先に敵がいるかチェック
    bool IsEnemyOnTarget(Vector3Int targetCell)
    {
        foreach (var enemy in Tile.allEnemys)
        {
            Vector3Int enemyCell = tilemap.WorldToCell(enemy.transform.position);
            if (enemyCell == targetCell)
            {

                Player_Moving = false;
                StartCoroutine(EnemyTurnRoutine());
                return true;
            }
        }
        return false;
>>>>>>> 6af5d67ab20c2ecc9d41dc2304ce09a43b25cfc3
    }

    //敵の行動に移行させる
    public IEnumerator EnemyTurnRoutine()
    {
        List<Enemys> enemiesCopy = new List<Enemys>(Tile.allEnemys);

        for (int i = 0; i < enemiesCopy.Count; i++)
        {
            if (enemiesCopy[i] == null) continue;
            enemiesCopy[i].ExecuteTurn();
            yield return new WaitForSeconds(0f);
        }
        Player_Moving = true;
    }
    //移動できるタイルを設定
    bool IsWalkableTile(TileBase tile)
    {
        // 通行可能なTileだけを許可（例：FloorTile, PathTileなど）
        return tile.name == "Tile 001_0"|| tile.name == "Exit_0";
    }
    //周辺にいる敵を検知
    private void UpdateNearbyEnemies()
    {
        nearbyEnemies.Clear();
        Vector3Int playerCell = tilemap.WorldToCell(transform.position);

        foreach (var enemy in Tile.allEnemys)
        {
            Vector3Int enemyCell = tilemap.WorldToCell(enemy.transform.position);
            int dx = Mathf.Abs(playerCell.x - enemyCell.x);
            int dy = Mathf.Abs(playerCell.y - enemyCell.y);

            if (dx <= p_status.attackRange && dy <= p_status.attackRange)
            {
                nearbyEnemies.Add(enemy);
            }
        }
    }
    //ロックオン中の敵の色を赤くする
    private void HighlightTarget(Enemys target)
    {
        foreach (var enemy in nearbyEnemies)
        {
            enemy.GetComponent<SpriteRenderer>().color = Color.white;
        }

        target.GetComponent<SpriteRenderer>().color = Color.red;
    }
    //プレイヤーの体力表示更新
    public void UpdateHPValue()
    {
        PlayerHPBar.value = p_status.HP;
        HP_Text.text = p_status.HP + "/" + p_status.maxHP;
    }
}