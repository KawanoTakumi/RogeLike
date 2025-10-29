using UnityEngine;
using UnityEngine.InputSystem;

public class ItemDate : MonoBehaviour
{
    public int ItemStatus = 0;//ÉAÉCÉeÉÄÇÃå¯â î‘çÜ
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


        if (playerCell == itemCell && Keyboard.current.eKey.wasPressedThisFrame)
        {

            playerControl.GetItemValue(ItemStatus);
            Destroy(gameObject);
        }
    }
}