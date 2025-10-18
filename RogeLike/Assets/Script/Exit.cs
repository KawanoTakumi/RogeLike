using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Exit : Tile
{
    public static Vector3 exitCell;
    public static Vector3 playerPos;
    public static int Now_Floor= 1;//���݂̊K��
    public static int Clear_Dungeon = 0;//�_���W�����N���A��
    GameObject FText;
    TextMeshProUGUI Floor_Text;
    private SpriteRenderer Sp;

    public void Start()
    {
        Sp = gameObject.GetComponent<SpriteRenderer>();
        Sp.color = Color.white;
        FText = GameObject.Find("FloorText");
        Floor_Text = FText.GetComponent<TextMeshProUGUI>();
        Floor_Text.text = Now_Floor + "F";
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        Vector3Int playerCell = Save_maps.WorldToCell(playerPos);
        if (playerCell == exitCell && Keyboard.current.eKey.wasPressedThisFrame)
        {
            LoadNextMap();
            allEnemys.Clear();
        }
    }
    void Update()
    {
        if(!Boss_Flag)
        {
            Sp.color = Color.white;
            Vector3 playerCell = Save_maps.WorldToCell(playerPos);
            playerCell.x += 0.5f;
            playerCell.y += 0.5f;
            if (playerCell == exitCell && Keyboard.current.eKey.wasPressedThisFrame)
            {
                allEnemys.Clear();
                LoadNextMap();
            }
        }
        else
        {
            Sp.color = Color.black;
        }
    }

    void LoadNextMap()
    {
        Now_Floor++;
        if(Now_Floor <= MAX_FLOOR)
        {
            SceneManager.LoadScene("Dungeon");
        }
        else
        {
            Clear_Dungeon++;//�N���A�K���𑝉�
            GameState.Clear_Flag = true;
            SceneManager.LoadScene("Cleard");
        }
    }
    public static void Defeat_Player()
    {
        SceneManager.LoadScene("Lose");
    }
}