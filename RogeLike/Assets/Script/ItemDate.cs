using UnityEngine;
using UnityEngine.InputSystem;

public class ItemDate : MonoBehaviour
{
    public int ItemStatus = 0;//アイテムの効果番号
    public static Vector3 playerPos;
    private Vector3 Item_Pos;
    private PlayerControl playerControl;

    public void Start()
    {
        playerControl = GameObject.FindWithTag("Player").GetComponent < PlayerControl>();
        Item_Pos = transform.position;
    }
    public void Update()
    {
        playerPos = playerControl.transform.position;
        Vector3Int playerCell = Tile.Save_maps.WorldToCell(playerPos);
        Vector3Int itemCell = Tile.Save_maps.WorldToCell(Item_Pos);

        if (playerCell == itemCell)
        {
            Debug.Log("アイテムの上に乗っています");
        }

        if (playerCell == itemCell && Keyboard.current.eKey.wasPressedThisFrame)
        {

            playerControl.GetItemValue(ItemStatus);
            Debug.Log("アイテム効果を取得");
            Destroy(gameObject);
        }
    }
}