using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    private Slider PlayerHPBar;//�v���C���[�̗̓o�[

    private List<Enemys> nearbyEnemies = new();
    public static CharacterStatus p_status;//�ʂ̃X�e�[�^�X�ɕύX
    private int currentTargetIndex = 0;
    private Enemys currentTarget = null;
    private BattleLog Log;
    private Text HP_Text;//�̗͕\��
    private TextMeshProUGUI level_text;//���x���\��
    public static int max_exp = 5;
    void Awake()
    {
        inputActions = new Move();
    }

    private void Start()
    {
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        p_status = Instantiate(PlayerStatus);
        p_status = PlayerStatus;
        //�^�C���}�b�v��ǂݍ���
        tilemap = Tile.Save_maps;
        Log = GameObject.Find("BattleLog").GetComponent<BattleLog>();
        Player_Moving = true;
        //�̗͕\���p�I�u�W�F�N�g�ǂݍ���
        PlayerHPBar = GameObject.Find("HP").GetComponent<Slider>();
        HP_Text = GameObject.Find("HP_Text").GetComponent<Text>();
        p_status.HP = p_status.maxHP;
        PlayerHPBar.maxValue = p_status.maxHP;
        PlayerHPBar.value = p_status.HP;
        HP_Text.text = p_status.HP + "/" + p_status.maxHP;

        //���x���\���p�e�L�X�g�\��
        level_text = GameObject.Find("level_text").GetComponent<TextMeshProUGUI>();
        Debug.Log("���݂̃��x��" + p_status.level);
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();
    void Update()
    {
        //���x���\��
        level_text.text = "Lv." + p_status.level;

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
            //���O�ǉ�
            Log.ShowMessage($" {currentTarget.name} hit {damage} damage");
            moveInput = Vector2.zero;

            Player_Moving = false;
            foreach (var enemy in Tile.allEnemys)
            {
                enemy.Enemy_Moving = true;
            }
            StartCoroutine(EnemyTurnRoutine());
        }

        //�ړ��𐧌�
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
                //�ړ������玩�R�񕜁A�^�[�����ƂɂP��
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
    // �v���C���[�̈ړ���ɓG�����邩�`�F�b�N
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

    //�G�̍s���Ɉڍs������
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
    //�ړ��ł���^�C����ݒ�
    bool IsWalkableTile(TileBase tile)
    {
        // �ʍs�\��Tile���������i��FFloorTile, PathTile�Ȃǁj
        return tile.name == "Tile 001_0"|| tile.name == "Exit_0";
    }
    //���ӂɂ���G�����m
    private void UpdateNearbyEnemies()
    {
        nearbyEnemies.Clear();
        Vector3Int playerCell = tilemap.WorldToCell(transform.position);

        foreach (var enemy in Tile.allEnemys)
        {
            if (enemy == null) break;
            Vector3Int enemyCell = tilemap.WorldToCell(enemy.transform.position);
            int dx = Mathf.Abs(playerCell.x - enemyCell.x);
            int dy = Mathf.Abs(playerCell.y - enemyCell.y);

            if (dx <= p_status.attackRange && dy <= p_status.attackRange)
            {
                nearbyEnemies.Add(enemy);
            }
        }
    }
    //���b�N�I�����̓G�̐F��Ԃ�����
    private void HighlightTarget(Enemys target)
    {
        foreach (var enemy in nearbyEnemies)
        {
            enemy.GetComponent<SpriteRenderer>().color = Color.white;
        }

        target.GetComponent<SpriteRenderer>().color = Color.red;
    }
    //�v���C���[�̗͕̑\���X�V
    public void UpdateHPValue()
    {
        PlayerHPBar.value = p_status.HP;
        HP_Text.text = p_status.HP + "/" + p_status.maxHP;
    }
    //�o���l�l��
    public void Exp_gain(int exp)
    {
        p_status.exp += exp;
        if(p_status.exp > max_exp)
        {
            LevelUp();
        }
    }
    //���x���A�b�v
    void LevelUp()
    {
        //���x������
        p_status.level++;
        p_status.level_exp = 0;
        max_exp = p_status.level * 5;

        //�X�e�[�^�X�𑝉�������
        p_status.attack     += 2;
        p_status.diffence   += 1;
        p_status.maxHP      += 5;
        p_status.HP = p_status.maxHP;
        PlayerStatus = p_status;
        //���x�����R�ȏ�Ȃ�U���͈͂�+�P����
        if(p_status.level > 3)
        {
            p_status.attackRange = 2;
        }
    }
}