using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Enemys : MonoBehaviour
{
    public CharacterStatus Status;//�X�e�[�^�X�ǂݍ��ݗp
    public bool Enemy_Moving = false;//�v���C���[���ŕύX����̂�public
    private CharacterStatus EnemyStatus;//�ʃX�e�[�^�X�ɒu������
    private Transform player;
    private GameObject Player;
    private Tilemap tilemaps; 
    private BattleLog Log;
    private PlayerControl p_control;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EnemyStatus = Instantiate(Status);//�ʂ̃X�e�[�^�X�ɒu����������
        EnemyStatus.HP = EnemyStatus.maxHP;//�O�̂���HP��������
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
        return distance <= EnemyStatus.searchRange;
    }

    bool IsWalkableTile(TileBase tile)
    {
        // �ʍs�\��Tile���������i��FFloorTile, PathTile�Ȃǁj
        return tile.name == "Tile 001_0" || tile.name == "Exit_0";
    }

    public void ExecuteTurn()
    {
        if (!Enemy_Moving) return;
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

        if (dx <= 1 && dy <= 1)
        {
            AttackPlayer();
            yield return new WaitForSeconds(0.2f);
            yield break; // �U��������ړ����Ȃ�
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
        yield return new WaitForSeconds(0.05f); // ���o�p�̑ҋ@
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
        Log.ShowMessage($"{EnemyStatus.charaName} to die");

        Tile.allEnemys.Remove(this); // ���X�g����폜
        Destroy(gameObject);
    }
    private void AttackPlayer()
    {
        p_control = Player.GetComponent<PlayerControl>();
        int damage = Mathf.Max(Status.attack - PlayerControl.p_status.diffence, 1);
        PlayerControl.p_status.HP -= damage;
        PlayerControl.p_status.HP = Mathf.Max(PlayerControl.p_status.HP, 0);
        p_control.UpdateHPValue();
        if (Log != null)
        Log.ShowMessage($" player hit to{damage} damage");

        if (PlayerControl.p_status.HP <= 0)
        {
            Debug.Log("�v���C���[���S");
        }
    }
}