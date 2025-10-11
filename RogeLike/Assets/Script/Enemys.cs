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
    public bool Enemy_Moving = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
                Debug.LogWarning("�v���C���[���ǂݍ��܂�܂���ł���");
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
    //�v���C���[�����E�Ɏ��߂����ǂ���
    bool CanSeePlayer()
    {
        float distance = Vector2.Distance(transform.position, player.transform.position);
        return distance <= Status.searchRange;
    }

    bool IsWalkableTile(TileBase tile)
    {
        // �ʍs�\��Tile���������i��FFloorTile, PathTile�Ȃǁj
        return tile.name == "Tile 001_0" || tile.name == "Exit_0";
    }

    public void ExecuteTurn()
    {
        if (!Enemy_Moving) return;
        Debug.Log("�G�̍s����");
        Enemy_Moving = false; // �s���J�n�Ɠ�����OFF
        StartCoroutine(DoAction());
    }
    private IEnumerator DoAction()
    {
        // �v���C���[�Ƃ̋������`�F�b�N
        Vector3Int enemyCell = tilemaps.WorldToCell(transform.position);
        Vector3Int playerCell = tilemaps.WorldToCell(player.position);

        int dx = Mathf.Abs(enemyCell.x - playerCell.x);
        int dy = Mathf.Abs(enemyCell.y - playerCell.y);

        // �v���C���[�̎���1�}�X�ȓ��Ȃ�ړ����Ȃ�
        if (dx <= 1 && dy <= 1)
        {
            yield return new WaitForSeconds(0.2f); // ���o�p�ҋ@
            yield break;
        }

        // ����ȊO�Ȃ�ړ�����
        Vector2Int direction = GetMoveDirection();
        Vector3Int targetCell = enemyCell + new Vector3Int(direction.x, direction.y, 0);
        TileBase targetTile = tilemaps.GetTile(targetCell);
        // ���̓G�����̃Z���ɂ��邩�`�F�b�N
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
        yield return new WaitForSeconds(0f); // ���o�p�̑ҋ@
    }
}