using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Enemys : MonoBehaviour
{
    public CharacterStatus Status;
    public CharacterStatus EnemyStatus;
    private Transform player;
    private GameObject Player;
    private int currentHP = 0;
    private Tilemap tilemaps;
    public bool Enemy_Moving = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EnemyStatus = Instantiate(Status);
        EnemyStatus.HP = EnemyStatus.maxHP;//念のためHPを初期化
        Debug.Log("HP" + EnemyStatus.HP);
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
    }

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
        return tile.name == "Tile 001_0" || tile.name == "Exit_0";
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

        // プレイヤーの周囲1マス以内なら移動しない
        if (dx <= 1 && dy <= 1)
        {
            yield return new WaitForSeconds(0.1f); // 演出用待機
            yield break;
        }

        // それ以外なら移動処理
        Vector2Int direction = GetMoveDirection();
        Vector3Int targetCell = enemyCell + new Vector3Int(direction.x, direction.y, 0);
        TileBase targetTile = tilemaps.GetTile(targetCell);
        // 他の敵がそのセルにいるかチェック
        foreach (var other in Tile.allEnemys)
        {
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
        yield return new WaitForSeconds(0f); // 演出用の待機
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("攻撃前の体力" + EnemyStatus.HP);
        EnemyStatus.HP -= damage;
        EnemyStatus.HP = Mathf.Max(EnemyStatus.HP, 0);
        Debug.Log("敵の体力" + EnemyStatus.HP);

        if (EnemyStatus.HP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} は死亡しました");
        Tile.allEnemys.Remove(this); // リストから削除
        Destroy(gameObject);
    }

}