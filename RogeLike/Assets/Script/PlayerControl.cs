using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerControl : MonoBehaviour
{
    public float moveCooldown = 0.2f;
    private float lastMoveTime;
    Tilemap tilemap;
    private Vector2 moveInput;
    private Move inputActions;
    public static bool Player_Moving = false;
    public CharacterStatus PlayerStatus;

    private List<Enemys> nearbyEnemies = new();
    private int currentTargetIndex = 0;
    private Enemys currentTarget = null;

    void Awake()
    {
        inputActions = new Move();
    }

    private void Start()
    {
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        //�^�C���}�b�v��ǂݍ���
        tilemap = Tile.Save_maps;
        Player_Moving = true;
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
                Debug.Log("�G��������܂���");
            }
        }
        if (Keyboard.current.spaceKey.wasPressedThisFrame && currentTarget != null)
        {

            int damage = Mathf.Max(PlayerStatus.attack - currentTarget.Status.diffence, 1);
            currentTarget.TakeDamage(damage);

            Debug.Log($"�v���C���[�� {currentTarget.name} �� {damage} �_���[�W��^����");
            moveInput = Vector2.zero;

            Player_Moving = false;
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
                Exit.playerPos = transform.position;
                lastMoveTime = Time.time;
                moveInput = Vector2.zero;
                Player_Moving = false;

                foreach (var enemy in Tile.allEnemys)
                {
                    enemy.Enemy_Moving = true;
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

        foreach (var enemy in Tile.allEnemys)
        {
            enemy.ExecuteTurn();
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
    private void UpdateNearbyEnemies()
    {
        nearbyEnemies.Clear();
        Vector3Int playerCell = tilemap.WorldToCell(transform.position);

        foreach (var enemy in Tile.allEnemys)
        {
            Vector3Int enemyCell = tilemap.WorldToCell(enemy.transform.position);
            int dx = Mathf.Abs(playerCell.x - enemyCell.x);
            int dy = Mathf.Abs(playerCell.y - enemyCell.y);

            if (dx <= PlayerStatus.attackRange && dy <= PlayerStatus.attackRange)
            {
                nearbyEnemies.Add(enemy);
            }
        }
    }
    private void HighlightTarget(Enemys target)
    {
        foreach (var enemy in nearbyEnemies)
        {
            enemy.GetComponent<SpriteRenderer>().color = Color.white;
        }

        target.GetComponent<SpriteRenderer>().color = Color.red;
    }

}