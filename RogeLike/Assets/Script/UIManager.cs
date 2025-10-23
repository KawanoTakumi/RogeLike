using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public GameObject UPose;
    public GameObject Status;
    public TextMeshProUGUI PoseGuide;
    public TextMeshProUGUI StatusGuide;

    //�e�X�e�[�^�X
    public TextMeshProUGUI MAXHP;
    public TextMeshProUGUI ATK;
    public TextMeshProUGUI DEF;
    public TextMeshProUGUI RANGE;
    public TextMeshProUGUI EXP;

    bool ST_Flag = false;
    private void Update()
    {
        if(GameState.Setting_Flag)
        {
            UPose.SetActive(true);
            PoseGuide.text = "G : Back";
        }
        else
        {
            UPose.SetActive(false);
            PoseGuide.text = "G : Pose";

        }
        //�X�e�[�^�X�\��
        if(Keyboard.current.fKey.wasPressedThisFrame)
        {
            if(!ST_Flag)
                ST_Flag = true;
            else
                ST_Flag = false;
        }

        if(!ST_Flag)
        {
            StatusGuide.text = "F : Status";
            Status.SetActive(false);
        }
        else
        {
            Status.SetActive(true);
            StatusGuide.text = "F : Back";

            MAXHP.text = "�̗�   " + PlayerControl.p_status.HP + "/" + PlayerControl.p_status.maxHP;
            ATK.text = "�U����   " + PlayerControl.p_status.attack;
            DEF.text = "�h���   " + PlayerControl.p_status.diffence;
            RANGE.text = "�U���͈�   " + PlayerControl.p_status.attackRange;
            EXP.text = "EXP   " + PlayerControl.p_status.level_exp + "/" + PlayerControl.max_exp;
        }
    }
}