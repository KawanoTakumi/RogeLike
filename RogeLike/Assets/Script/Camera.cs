using UnityEngine;

public class CameraCnt : MonoBehaviour
{
    private Transform Player;//�v���C���[
    public float smoothSpeed = 5f;


    private void LateUpdate()
    {
        //�v���C���[���������Ă��Ȃ��ꍇ
        if(Player == null)
        {
            GameObject PlayerObj = GameObject.FindGameObjectWithTag("Player");
            if(PlayerObj != null)
            {
                Player = PlayerObj.transform;//�v���C���[�̈ʒu���擾
            }
            else
            {
                //��������Ă��Ȃ��ꍇ�͂��̂܂܏I��
                return;
            }
        }
        //��ɒǏ]������
        transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y, transform.position.z);


    }
}
