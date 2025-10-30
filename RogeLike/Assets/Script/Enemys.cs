using System;
using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Enemys : MonoBehaviour
{
    public static int Start_Level = -1;

    public CharacterStatus Status;//�X�e�[�^�X�ǂݍ��ݗp
    public TextMeshProUGUI Levels;//���x���\��
    public bool Enemy_Moving = false;//�v���C���[���ŕύX����̂�public
    public CharacterStatus EnemyStatus;//�ʃX�e�[�^�X�ɒu������
    public GameObject HPBook = null;//�{�X�h���b�v�p
    public GameObject AttackEffect = null;

    private AudioSource AttckSE;
    private Transform player;
    private GameObject Player;
    private Tilemap tilemaps; 
    private BattleLog Log;
    private PlayerControl p_control;
    private static int normal_levels = 0;//��{�ƂȂ郌�x��
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EnemyStatus = Instantiate(Status);//�ʂ̃X�e�[�^�X�ɒu����������
        EnemyLevelSetting(Exit.Now_Floor);//���x���␳��ݒ�
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
        AttckSE = GameObject.Find("AttckSE").GetComponent<AudioSource>();

    }
    //�v���C���[�̈ړ��������擾
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
        return tile.name == Tile.FTILENAME.name;
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
        Log.ShowMessage($"{EnemyStatus.charaName}��|�����I");
        //�{�X�Ȃ�
        if (EnemyStatus.name.Contains("Boss"))
        {
            Exit.Boss_Flag = false;
            if(HPBook)
            Instantiate(HPBook, this.transform.position, this.transform.rotation);
        }
            
        Tile.allEnemys.Remove(this); // ���X�g����폜
        p_control = Player.GetComponent<PlayerControl>();
        p_control.Exp_gain(EnemyStatus.exp);//�v���C���[�Ɍo���l������
        Destroy(gameObject);
    }
    private void AttackPlayer()
    {
        p_control = Player.GetComponent<PlayerControl>();
        int damage = Mathf.Max(EnemyStatus.attack - PlayerControl.p_status.diffence, 1);
        PlayerControl.p_status.HP -= damage;
        PlayerControl.p_status.HP = Mathf.Max(PlayerControl.p_status.HP, 0);
        p_control.UpdateHPValue();
        //���O�X�V
        if (Log != null)
        Log.ShowMessage($" �v���C���[��{damage}�_���[�W�������");
        //�G�t�F�N�g����
        if (AttackEffect != null)
            Instantiate(AttackEffect, Player.transform.position, Player.transform.rotation);
        //�U����BGM���Đ�
        if(AttckSE != null)
        {
            AttckSE.Play();
        }
            

        if (PlayerControl.p_status.HP <= 0)
        {
            GameState.Lose_Flag = true;
            Exit.Boss_Flag = false;
            Exit.Defeat_Player();//������ʂɂ���
        }
    }
    //�K�w��o���Ă����x�ɏ����X�e�[�^�X�ɊK�w���̃o�t��������
    public void EnemyLevelSetting(int floor_level)
    {
        int set_level = normal_levels;//�������ꂽ���x��
        //�P�K�w�̃v���C���[�̃��x�����擾
        if (Exit.Now_Floor == 1)
        {
            set_level = PlayerControl.Grobal_Player_Level;
            normal_levels = set_level;
        }
        set_level += (floor_level + UnityEngine.Random.Range(-1,1));//�v���C���[���x�����班�������_��������������
        //�Œ�P�͊m��
        if (set_level <= 0)
            set_level = 1;
        set_level += Exit.Clear_Dungeon;//�N���A�K�������x�����グ��
        if (gameObject.name.Contains("Boss"))
        {
            set_level += 5;//�{�X��5���x���グ��
        }
        //�X�e�[�^�X�ݒ�
        EnemyStatus.level = set_level;
        EnemyStatus.exp += EnemyStatus.level+5;
        EnemyStatus.maxHP += EnemyStatus.level+5;
        EnemyStatus.attack += EnemyStatus.level;
        Levels.text = EnemyStatus.level.ToString();
        //�h��͂̓��x����5�ȏ�̏ꍇ����������
        if(set_level > 5)
        {
            EnemyStatus.diffence += EnemyStatus.level;
        }
    }
}