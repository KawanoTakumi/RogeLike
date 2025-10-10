using UnityEngine;

public class CameraCnt : MonoBehaviour
{
    private Transform Player;//プレイヤー
    public float smoothSpeed = 5f;


    private void LateUpdate()
    {
        //プレイヤーが見つかっていない場合
        if(Player == null)
        {
            GameObject PlayerObj = GameObject.FindGameObjectWithTag("Player");
            if(PlayerObj != null)
            {
                Player = PlayerObj.transform;//プレイヤーの位置を取得
            }
            else
            {
                //発見されていない場合はそのまま終了
                return;
            }
        }
        //常に追従させる
        transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y, transform.position.z);


    }
}
