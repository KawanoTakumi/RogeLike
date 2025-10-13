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
        //Vector3Int playerCell = Tile.Save_maps.WorldToCell(playerPos);
        //if (playerCell == exitCell && Keyboard.current.eKey.wasPressedThisFrame)
        //{
        //    LoadNextMap();
        //    Tile.allEnemys.Clear();
        //}
    }

    void LoadNextMap()
    {
        // 例：SceneManagerを使って次のシーンへ
        SceneManager.LoadScene("SampleScene");
    }
}