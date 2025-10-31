using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;



public class PlayerControl : MonoBehaviour
{
    public float moveCooldown = 0.01f;
    public GameObject AttackEffect;//攻撃モーション
    public GameObject HealEffect;//回復モーション
    private float lastMoveTime;
    Tilemap tilemap;
    private Vector2 moveInput;
    private Move inputActions;
    public static bool Player_Moving = false;
    public CharacterStatus PlayerStatus;
    private Slider PlayerHPBar;//プレイヤー体力バー

    private List<Enemys> nearbyEnemies = new();
    public static CharacterStatus p_status;//個別のステータスに変更
    public static int Grobal_Player_Level = 0;//敵の強さなどを設定する
    private int currentTargetIndex = 0;
    private Enemys currentTarget = null;
    private BattleLog Log;
    private TextMeshProUGUI HP_Text;//体力表示
    private TextMeshProUGUI level_text;//レベル表示
    private TextMeshProUGUI heal_text;//薬草の個数
    private TextMeshProUGUI Name;
    public static int max_exp = 5;
    public static int heal_count = 0;//回復可能回数
    void Awake()
    {
        inputActions = new Move();
    }

    private void Start()
    {
        Name = GameObject.Find("PlayerName").GetComponent<TextMeshProUGUI>();
        if (Name != null)
            Name.text = InputName.OutputName;
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        p_status = Instantiate(PlayerStatus);
        //リセットフラグがtrueならプレイヤーのレベルをリセットする
        if(GameState.Reset_Flag)
        {
            StatusReset();
            GameState.Reset_Flag = false;
        }
        p_status = PlayerStatus;
        //タイルマップを読み込む
        tilemap = Tile.Save_maps;
        Log = GameObject.Find("BattleLog").GetComponent<BattleLog>();
        Player_Moving = true;
        //体力表示用オブジェクト読み込み
        PlayerHPBar = GameObject.Find("HP").GetComponent<Slider>();
        HP_Text = GameObject.Find("HP_Text").GetComponent<TextMeshProUGUI>();
        p_status.HP = p_status.maxHP;
        PlayerHPBar.maxValue = p_status.maxHP;
        PlayerHPBar.value = p_status.HP;
        HP_Text.text = p_status.HP + "/" + p_status.maxHP;

        //レベル表示用テキスト表示
        level_text = GameObject.Find("level_text").GetComponent<TextMeshProUGUI>();
        heal_text = GameObject.Find("Heal_text").GetComponent<TextMeshProUGUI>();
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();
    void Update()
    {
        //表示
        level_text.text = "Lv." + p_status.level;
        heal_text.text = "× " + heal_count + "/10";

        if (p_status.HP > p_status.maxHP)
            p_status.HP = p_status.maxHP;
        if(!GameState.Setting_Flag)
        {
            UpdateNearbyEnemies(p_status.attackRange);
            UpdateHPValue();
            //索敵
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
                    Log.ShowMessage($"敵が周りにいない");
                }
            }
            //回復
            if(Keyboard.current.qKey.wasPressedThisFrame && heal_count > 0)
            {
                
                if (p_status.HP < p_status.maxHP)
                {
                    if (HealEffect != null)
                        Instantiate(HealEffect, gameObject.transform.position, Quaternion.identity);
                    p_status.HP += 30;
                    if (p_status.HP > p_status.maxHP)
                        p_status.HP = p_status.maxHP;
                    heal_count--;
                    //念のため
                    if (heal_count < 0)
                        heal_count = 0;
                    Log.ShowMessage("薬草を使用した");
                }
                else
                {
                    Log.ShowMessage($"薬草を食べる気分じゃない");
                }

            }
            //攻撃
            if (Keyboard.current.spaceKey.wasPressedThisFrame && currentTarget != null)
            {
                int damage = Mathf.Max(p_status.attack - currentTarget.EnemyStatus.diffence, 1);


                //攻撃アニメーションを生成
                if (AttackEffect != null)
                    Instantiate(AttackEffect, currentTarget.transform.position, Quaternion.identity);

                //ログ追加
                Log.ShowMessage($" {currentTarget.EnemyStatus.charaName} に {damage} ダメージ与えた！");
                currentTarget.TakeDamage(damage);
                moveInput = Vector2.zero;

                Player_Moving = false;
                foreach (var enemy in Tile.allEnemys)
                {
                    enemy.Enemy_Moving = true;
                }
                StartCoroutine(EnemyTurnRoutine());
            }

            //移動を制御
            if (Time.time - lastMoveTime < moveCooldown || moveInput == Vector2.zero) return;

            if (moveInput.x != 0 && moveInput.y != 0)
            {
                moveInput = new Vector2(moveInput.x, 0);
            }

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
    }
    // プレイヤーの移動先に敵がいるかチェック
    bool IsEnemyOnTarget(Vector3Int targetCell)
    {
        foreach (var enemy in Tile.allEnemys)
        {
            if (enemy == null) return false;
            Vector3Int enemyCell = tilemap.WorldToCell(enemy.transform.position);
            if (enemyCell == targetCell)
            {

                Player_Moving = false;
                StartCoroutine(EnemyTurnRoutine());
                return true;
            }
        }
        return false;
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
        return tile.name == Tile.FTILENAME.name|| tile.name == "Exit";
    }
    //周辺にいる敵を検知
    private void UpdateNearbyEnemies(int range)
    {
        nearbyEnemies.Clear();
        Vector3Int playerCell = tilemap.WorldToCell(transform.position);

        foreach (var enemy in Tile.allEnemys)
        {
            if (enemy == null) continue;
            Vector3Int enemyCell = tilemap.WorldToCell(enemy.transform.position);
            int dx = Mathf.Abs(playerCell.x - enemyCell.x);
            int dy = Mathf.Abs(playerCell.y - enemyCell.y);

            if (dx <= range && dy <= range)
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

        target.GetComponent<SpriteRenderer>().color = Color.orangeRed;
    }
    //プレイヤーの体力表示更新
    public void UpdateHPValue()
    {
        PlayerHPBar.maxValue = p_status.maxHP;
        PlayerHPBar.value = p_status.HP;
        HP_Text.text = p_status.HP + "/" + p_status.maxHP;
    }

    //経験値獲得
    public void Exp_gain(int exp)
    {
        p_status.level_exp += exp;
        if(p_status.level_exp > max_exp)
        {
            LevelUp();
        }
    }
    //アイテム取得関数
    public void GetItemValue(int value)
    {
        switch (value)
        {
            case 0:
                {
                    //１０こまで持てる
                    if(heal_count < 10)
                    {
                        heal_count++;
                        Log.ShowMessage($"薬草を拾った！");
                    }
                    else
                    {
                        if(p_status.HP < p_status.maxHP)
                        {
                            p_status.HP += 30;
                            if(p_status.HP > p_status.maxHP)
                                p_status.HP = p_status.maxHP;
                            if (HealEffect != null)
                                Instantiate(HealEffect, gameObject.transform.position, Quaternion.identity);
                            Log.ShowMessage($"持ちきれないので食べた！HP３０回復");
                        }
                    }
                    
                }break;
            case 1: 
                {
                    p_status.attack += 3;
                    Log.ShowMessage($" プレイヤーの攻撃力が３増加");
                }
                break;
            case 2: 
               {
                    p_status.diffence += 3;
                    Log.ShowMessage($" プレイヤーの防御力が３増加");
                } break;
            case 3:
                {
                    if(p_status.attackRange < 3)
                    {
                        p_status.attackRange++;
                        Log.ShowMessage($"秘伝の書で攻撃範囲が1増加");
                    }
                    else
                    {
                        p_status.maxHP += 3;
                        Log.ShowMessage($"秘伝の書で最大体力が3増加");
                    }
                }
                break;
        }
    }

    //レベルアップ
    void LevelUp()
    {
        //レベル増加
        p_status.level++;
        Log.ShowMessage($"レベル{p_status.level}にレベルアップ！");
        p_status.level_exp = 0;
        max_exp = p_status.level *35;

        //ステータスを増加させる
        p_status.attack     += 2;
        p_status.diffence   += 1;
        p_status.maxHP      += 2;

        if(p_status.HP < p_status.maxHP / 2)
        p_status.HP = p_status.maxHP /2;

        //表示を設定
        PlayerHPBar.maxValue = p_status.maxHP;
        PlayerHPBar.value = p_status.HP;


            //レベルを保存
            Grobal_Player_Level = p_status.level;
    }
    //デバッグ用、ゲームオーバー時ステータス初期化
    public void StatusReset()
    {
        PlayerStatus.level = 1;
        PlayerStatus.maxHP = 100;
        PlayerStatus.HP = PlayerStatus.maxHP;
        PlayerStatus.attack = 5;
        PlayerStatus.diffence = 5;
        PlayerStatus.searchRange = 1;
        PlayerStatus.level_exp = 0;
        max_exp = PlayerStatus.level *15;
        PlayerStatus.exp = 0;
        p_status = PlayerStatus;
        Grobal_Player_Level = PlayerStatus.level;
    }
}