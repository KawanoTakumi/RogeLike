using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Enemys : MonoBehaviour
{
    public CharacterStatus Status;
    private Transform player;
    private GameObject Player;
    private int currentHP = 0;
    private Tilemap tilemaps;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tilemaps = Tile.Save_maps;

        if (Player == null)
        {
            Player = GameObject.FindWithTag("Player");
            if (Player == null)
            {
                Debug.LogWarning("プレイヤーが読み込まれませんでした");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public IEnumerator TakeTurn()
    {
        Vector2Int direction = GetMoveDirection();
        Vector3Int currentCell = tilemaps.WorldToCell(transform.position);
        Vector3Int targetCell = currentCell + new Vector3Int(direction.x, direction.y, 0);

        if (tilemaps.GetTile(targetCell) != null)
        {
            Vector3 targetWorld = tilemaps.CellToWorld(targetCell) + new Vector3(0.5f, 0.5f, 0);
            transform.position = targetWorld;
        }

        yield return new WaitForSeconds(0.3f); // 行動の演出時間
    }
    void MoveTowardPlayer()
    {
        //ここに行動を書く
    }
    Vector2Int GetMoveDirection()
    {
        if (CanSeePlayer())
        {
            Vector2Int enemyPos = (Vector2Int)tilemaps.WorldToCell(transform.position);
            Vector2Int playerPos = (Vector2Int)tilemaps.WorldToCell(player.transform.position);
            Vector2Int delta = playerPos - enemyPos;

            // 最も距離のある軸方向に1マス移動
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return new Vector2Int(Math.Sign(delta.x), 0);
            }
            else
            {
                return new Vector2Int(0, Math.Sign(delta.y));
            }
        }
        else
        {
            // ランダムな上下左右
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
        return distance <= Status.searchRange;
    }
}
