using UnityEngine;

public class ItemDate : MonoBehaviour
{
    public int ItemStatus = 0;//�A�C�e���̌��ʔԍ�

    private PlayerControl playerControl;

    public void Start()
    {
        playerControl = GameObject.FindWithTag("Player").GetComponent < PlayerControl>();
    }

    private void OnTriggerStay(Collider other)
    {
        playerControl.GetItemValue(ItemStatus);
    }
}