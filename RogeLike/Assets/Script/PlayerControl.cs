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
    public GameObject AttackEffect;//�U�����[�V����
    public GameObject HealEffect;//�񕜃��[�V����
    private float lastMoveTime;
    Tilemap tilemap;
    private Vector2 moveInput;
    private Move inputActions;
    public static bool Player_Moving = false;
    public CharacterStatus PlayerStatus;
    private Slider PlayerHPBar;//�v���C���[�̗̓o�[

    private List<Enemys> nearbyEnemies = new();
    public static CharacterStatus p_status;//�ʂ̃X�e�[�^�X�ɕύX
    public static int Grobal_Player_Level = 0;//�G�̋����Ȃǂ�ݒ肷��
    private int currentTargetIndex = 0;
    private Enemys currentTarget = null;
    private BattleLog Log;
    private TextMeshProUGUI HP_Text;//�̗͕\��
    private TextMeshProUGUI level_text;//���x���\��
    private TextMeshProUGUI heal_text;//�򑐂̌�
    private TextMeshProUGUI Name;
    public static int max_exp = 5;
    public static int heal_count = 0;//�񕜉\��
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
        //���Z�b�g�t���O��true�Ȃ�v���C���[�̃��x�������Z�b�g����
        if(GameState.Reset_Flag)
        {
            StatusReset();
            GameState.Reset_Flag = false;
        }
        p_status = PlayerStatus;
        //�^�C���}�b�v��ǂݍ���
        tilemap = Tile.Save_maps;
        Log = GameObject.Find("BattleLog").GetComponent<BattleLog>();
        Player_Moving = true;
        //�̗͕\���p�I�u�W�F�N�g�ǂݍ���
        PlayerHPBar = GameObject.Find("HP").GetComponent<Slider>();
        HP_Text = GameObject.Find("HP_Text").GetComponent<TextMeshProUGUI>();
        p_status.HP = p_status.maxHP;
        PlayerHPBar.maxValue = p_status.maxHP;
        PlayerHPBar.value = p_status.HP;
        HP_Text.text = p_status.HP + "/" + p_status.maxHP;

        //���x���\���p�e�L�X�g�\��
        level_text = GameObject.Find("level_text").GetComponent<TextMeshProUGUI>();
        heal_text = GameObject.Find("Heal_text").GetComponent<TextMeshProUGUI>();
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();
    void Update()
    {
        //�\��
        level_text.text = "Lv." + p_status.level;
        heal_text.text = "�~ " + heal_count + "/10";

        if (p_status.HP > p_status.maxHP)
            p_status.HP = p_status.maxHP;
        if(!GameState.Setting_Flag)
        {
            UpdateNearbyEnemies(p_status.attackRange);
            UpdateHPValue();
            //���G
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
                    Log.ShowMessage($"�G������ɂ��Ȃ�");
                }
            }
            //��
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
                    //�O�̂���
                    if (heal_count < 0)
                        heal_count = 0;
                    Log.ShowMessage("�򑐂��g�p����");
                }
                else
                {
                    Log.ShowMessage($"�򑐂�H�ׂ�C������Ȃ�");
                }

            }
            //�U��
            if (Keyboard.current.spaceKey.wasPressedThisFrame && currentTarget != null)
            {
                int damage = Mathf.Max(p_status.attack - currentTarget.EnemyStatus.diffence, 1);


                //�U���A�j���[�V�����𐶐�
                if (AttackEffect != null)
                    Instantiate(AttackEffect, currentTarget.transform.position, Quaternion.identity);

                //���O�ǉ�
                Log.ShowMessage($" {currentTarget.EnemyStatus.charaName} �� {damage} �_���[�W�^�����I");
                currentTarget.TakeDamage(damage);
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
        return tile.name == Tile.FTILENAME.name|| tile.name == "Exit";
    }
    //���ӂɂ���G�����m
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
    //���b�N�I�����̓G�̐F��Ԃ�����
    private void HighlightTarget(Enemys target)
    {
        foreach (var enemy in nearbyEnemies)
        {
            enemy.GetComponent<SpriteRenderer>().color = Color.white;
        }

        target.GetComponent<SpriteRenderer>().color = Color.orangeRed;
    }
    //�v���C���[�̗͕̑\���X�V
    public void UpdateHPValue()
    {
        PlayerHPBar.maxValue = p_status.maxHP;
        PlayerHPBar.value = p_status.HP;
        HP_Text.text = p_status.HP + "/" + p_status.maxHP;
    }

    //�o���l�l��
    public void Exp_gain(int exp)
    {
        p_status.level_exp += exp;
        if(p_status.level_exp > max_exp)
        {
            LevelUp();
        }
    }
    //�A�C�e���擾�֐�
    public void GetItemValue(int value)
    {
        switch (value)
        {
            case 0:
                {
                    //�P�O���܂Ŏ��Ă�
                    if(heal_count < 10)
                    {
                        heal_count++;
                        Log.ShowMessage($"�򑐂��E�����I");
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
                            Log.ShowMessage($"��������Ȃ��̂ŐH�ׂ��IHP�R�O��");
                        }
                    }
                    
                }break;
            case 1: 
                {
                    p_status.attack += 3;
                    Log.ShowMessage($" �v���C���[�̍U���͂��R����");
                }
                break;
            case 2: 
               {
                    p_status.diffence += 3;
                    Log.ShowMessage($" �v���C���[�̖h��͂��R����");
                } break;
            case 3:
                {
                    if(p_status.attackRange < 3)
                    {
                        p_status.attackRange++;
                        Log.ShowMessage($"��`�̏��ōU���͈͂�1����");
                    }
                    else
                    {
                        p_status.maxHP += 3;
                        Log.ShowMessage($"��`�̏��ōő�̗͂�3����");
                    }
                }
                break;
        }
    }

    //���x���A�b�v
    void LevelUp()
    {
        //���x������
        p_status.level++;
        Log.ShowMessage($"���x��{p_status.level}�Ƀ��x���A�b�v�I");
        p_status.level_exp = 0;
        max_exp = p_status.level *35;

        //�X�e�[�^�X�𑝉�������
        p_status.attack     += 2;
        p_status.diffence   += 1;
        p_status.maxHP      += 2;

        if(p_status.HP < p_status.maxHP / 2)
        p_status.HP = p_status.maxHP /2;

        //�\����ݒ�
        PlayerHPBar.maxValue = p_status.maxHP;
        PlayerHPBar.value = p_status.HP;


            //���x����ۑ�
            Grobal_Player_Level = p_status.level;
    }
    //�f�o�b�O�p�A�Q�[���I�[�o�[���X�e�[�^�X������
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