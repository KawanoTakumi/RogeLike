using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Exit : MonoBehaviour
{
    public static Vector3 exitCell;
    public static Vector3 playerPos;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                LoadNextMap();
                Tile.allEnemys.Clear();
            }
        }
    }
    void Update()
    {
<<<<<<< HEAD
        Vector3Int playerCell = Tile.Save_maps.WorldToCell(playerPos);
        if (playerCell == exitCell && Input.GetKeyDown(KeyCode.E))
        {
            LoadNextMap();
        }
        Debug.Log(playerCell + "プレイヤー" + exitCell + "ゴール");

        if(playerCell == exitCell)
        {
            Debug.Log("ゴールとかぶっています");
        }

=======
        //Vector3Int playerCell = Tile.Save_maps.WorldToCell(playerPos);
        //if (playerCell == exitCell && Keyboard.current.eKey.wasPressedThisFrame)
        //{
        //    LoadNextMap();
        //    Tile.allEnemys.Clear();
        //}
>>>>>>> 6af5d67ab20c2ecc9d41dc2304ce09a43b25cfc3
    }

    void LoadNextMap()
    {
        // 例：SceneManagerを使って次のシーンへ
        SceneManager.LoadScene("SampleScene");
    }
}