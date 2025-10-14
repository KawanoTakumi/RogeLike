using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Exit : MonoBehaviour
{
    private bool isOnExit = false;
    public static Vector3 exitCell;
    public static Vector3 playerPos;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isOnExit = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isOnExit = false;
        }
    }

    void Update()
    {
        Vector3Int playerCell = Tile.Save_maps.WorldToCell(playerPos);
        if (playerCell == exitCell && Input.GetKeyDown(KeyCode.E))
        {
            LoadNextMap();
        }
        Debug.Log(playerCell + "�v���C���[" + exitCell + "�S�[��");

        if(playerCell == exitCell)
        {
            Debug.Log("�S�[���Ƃ��Ԃ��Ă��܂�");
        }

    }

    void LoadNextMap()
    {
        // ��FSceneManager���g���Ď��̃V�[����
        SceneManager.LoadScene("SampleScene");
    }
}