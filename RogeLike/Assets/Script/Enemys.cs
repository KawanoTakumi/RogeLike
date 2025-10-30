using System;
using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Enemys : MonoBehaviour
{
    public static int Start_Level = -1;

    public CharacterStatus Status;//ステータス読み込み用
    public TextMeshProUGUI Levels;//レベル表示
    public bool Enemy_Moving = false;//プレイヤー側で変更するのでpublic
    public CharacterStatus EnemyStatus;//個別ステータスに置きかえ
    public GameObject HPBook = null;//ボスドロップ用
    public GameObject AttackEffect = null;

    private AudioSource AttckSE;
    private Transform player;
    private GameObject Player;
    private Tilemap tilemaps; 
    private BattleLog Log;
    private PlayerControl p_control;
    private static int normal_levels = 0;//基本となるレベル
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EnemyStatus = Instantiate(Status);//個別のステータスに置き換えする
        EnemyLevelSetting(Exit.Now_Floor);//レベル補正を設定
        EnemyStatus.HP = EnemyStatus.maxHP;//念のためHPを初期化
        Log = GameObject.Find("BattleLog").GetComponent<BattleLog>();
        tilemaps = Tile.Save_maps;

        if (Player == null)
        {
            Player = GameObject.FindWithTag("Player");
            if (Player != null)
            {
                player = Player.transform;
            }
            else
            {
                Debug.LogWarning("プレイヤーが読み込まれませんでした");
            }
        }
        AttckSE = GameObject.Find("AttckSE").GetComponent<AudioSource>();

    }
    //プレイヤーの移動方向を取得
    Vector2Int GetMoveDirection()
    {
        if (CanSeePlayer())
        {
            Vector2Int enemyPos = (Vector2Int)tilemaps.WorldToCell(transform.position);
            Vector2Int playerPos = (Vector2Int)tilemaps.WorldToCell(player.transform.position);
            Vector2Int delta = playerPos - enemyPos;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                return new Vector2Int(Math.Sign(delta.x), 0);
            else
                return new Vector2Int(0, Math.Sign(delta.y));
        }
        else
        {
            Vector2Int[] directions = {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            };
            return directions[UnityEngine.Random.Range(0, directions.Length)];
        }
    }
    //プレイヤーを視界に収めたかどうか
    bool CanSeePlayer()
    {
        float distance = Vector2.Distance(transform.position, player.transform.position);
        return distance <= EnemyStatus.searchRange;
    }

    bool IsWalkableTile(TileBase tile)
    {
        // 通行可能なTileだけを許可（例：FloorTile, PathTileなど）
        return tile.name == Tile.FTILENAME.name;
    }

    public void ExecuteTurn()
    {
        if (!Enemy_Moving) return;
        Enemy_Moving = false; // 行動開始と同時にOFF
        StartCoroutine(DoAction());
    }
    private IEnumerator DoAction()
    {
        // プレイヤーとの距離をチェック
        Vector3Int enemyCell = tilemaps.WorldToCell(transform.position);
        Vector3Int playerCell = tilemaps.WorldToCell(player.position);

        int dx = Mathf.Abs(enemyCell.x - playerCell.x);
        int dy = Mathf.Abs(enemyCell.y - playerCell.y);

        if (dx <= 1 && dy <= 1)
        {
            AttackPlayer();
            yield return new WaitForSeconds(0.2f);
            yield break; // 攻撃したら移動しない
        }

        // それ以外なら移動処理
        Vector2Int direction = GetMoveDirection();
        Vector3Int targetCell = enemyCell + new Vector3Int(direction.x, direction.y, 0);
        TileBase targetTile = tilemaps.GetTile(targetCell);
        // 他の敵がそのセルにいるかチェック
        foreach (var other in Tile.allEnemys)
        {
            if (other == null) break;
            if (other == this) continue;
            Vector3Int otherCell = tilemaps.WorldToCell(other.transform.position);
            if (otherCell == targetCell)
            {
                yield return new WaitForSeconds(0.2f);
                yield break;
            }
        }

        if (targetTile != null && IsWalkableTile(targetTile))
        {
            Vector3 targetWorld = tilemaps.CellToWorld(targetCell) + new Vector3(0.5f, 0.5f, 0);
            transform.position = targetWorld;
        }
        yield return new WaitForSeconds(0.05f); // 演出用の待機
    }

    public void TakeDamage(int damage)
    {
        EnemyStatus.HP -= damage;
        EnemyStatus.HP = Mathf.Max(EnemyStatus.HP, 0);

        if (EnemyStatus.HP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Log.ShowMessage($"{EnemyStatus.charaName}を倒した！");
        //ボスなら
        if (EnemyStatus.name.Contains("Boss"))
        {
            Exit.Boss_Flag = false;
            if(HPBook)
            Instantiate(HPBook, this.transform.position, this.transform.rotation);
        }
            
        Tile.allEnemys.Remove(this); // リストから削除
        p_control = Player.GetComponent<PlayerControl>();
        p_control.Exp_gain(EnemyStatus.exp);//プレイヤーに経験値を入れる
        Destroy(gameObject);
    }
    private void AttackPlayer()
    {
        p_control = Player.GetComponent<PlayerControl>();
        int damage = Mathf.Max(EnemyStatus.attack - PlayerControl.p_status.diffence, 1);
        PlayerControl.p_status.HP -= damage;
        PlayerControl.p_status.HP = Mathf.Max(PlayerControl.p_status.HP, 0);
        p_control.UpdateHPValue();
        //ログ更新
        if (Log != null)
        Log.ShowMessage($" プレイヤーは{damage}ダメージくらった");
        //エフェクト発生
        if (AttackEffect != null)
            Instantiate(AttackEffect, Player.transform.position, Player.transform.rotation);
        //攻撃のBGMを再生
        if(AttckSE != null)
        {
            AttckSE.Play();
        }
            

        if (PlayerControl.p_status.HP <= 0)
        {
            GameState.Lose_Flag = true;
            Exit.Boss_Flag = false;
            Exit.Defeat_Player();//負け画面にする
        }
    }
    //階層を登っていく度に初期ステータスに階層分のバフをかける
    public void EnemyLevelSetting(int floor_level)
    {
        int set_level = normal_levels;//調整されたレベル
        //１階層のプレイヤーのレベルを取得
        if (Exit.Now_Floor == 1)
        {
            set_level = PlayerControl.Grobal_Player_Level;
            normal_levels = set_level;
        }
        set_level += (floor_level + UnityEngine.Random.Range(-1,1));//プレイヤーレベルから少しランダム制を持たせる
        //最低１は確保
        if (set_level <= 0)
            set_level = 1;
        set_level += Exit.Clear_Dungeon;//クリア階数分レベルを上げる
        if (gameObject.name.Contains("Boss"))
        {
            set_level += 5;//ボスは5レベル上げる
        }
        //ステータス設定
        EnemyStatus.level = set_level;
        EnemyStatus.exp += EnemyStatus.level+5;
        EnemyStatus.maxHP += EnemyStatus.level+5;
        EnemyStatus.attack += EnemyStatus.level;
        Levels.text = EnemyStatus.level.ToString();
        //防御力はレベルが5以上の場合増加させる
        if(set_level > 5)
        {
            EnemyStatus.diffence += EnemyStatus.level;
        }
    }
}