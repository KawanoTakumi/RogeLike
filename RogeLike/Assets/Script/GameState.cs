using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
    PlayerControl Player_Cnt;
    public AudioSource hits;
    GameObject panel;
    TextMeshProUGUI CCount;
    TextMeshProUGUI LCount;
    public static bool Setting_Flag = false;
    public static bool Start_Flag = false;
    public static bool Lose_Flag = false;
    public static bool Clear_Flag = false;
    public static bool Reset_Flag = false;
    public static GameState Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // �����̃C���X�^���X������ꍇ�͔j��
            return;
        }

        Instance = this;
        Reset_Flag = true;
        DontDestroyOnLoad(gameObject); // �V�[�����܂����ŕێ�

    }

    private void Update()
    {

        if(!Start_Flag)
        {
            if (!CCount || !LCount)
            {
                CCount = GameObject.Find("CCount").GetComponent<TextMeshProUGUI>();
                LCount = GameObject.Find("LCount").GetComponent<TextMeshProUGUI>();
            }

            if (Exit.Clear_Dungeon > 0)
            {

                CCount.text = "Clear : " + Exit.Clear_Dungeon;
                LCount.text = "Level : " + PlayerControl.p_status.level;
            }


            if(!InputName.Rename_flag)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    Start_Flag = true;
                    hits.Play();
                    StartGame();
                }
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    hits.Play();
                    Exit_game();
                }
            }
        }
        //�Q�[����
        if(Start_Flag && Keyboard.current.gKey.wasPressedThisFrame)
        {
            hits.Play();
            if (!Setting_Flag)
            {
                Setting_Flag = true;
            }
            else
            {
                Setting_Flag = false;
            }
        }
        //�ݒ蒆
        if(Setting_Flag)
        {
            if(Keyboard.current.rKey.wasPressedThisFrame)
            {
                hits.Play();
                BackTitle();
            }
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                hits.Play();
                RestartGame();
            }
        }
        //������
        if (Lose_Flag)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                hits.Play();
                Start_Flag = false;
                Setting_Flag = false;
                Lose_Flag = false;
                Reset_Flag = false;
                Exit.Now_Floor = 1;//�P�K�ɖ߂�
                BackTitle();
            }
        }
        //������
        if (Clear_Flag)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                hits.Play();
                BackTitle();
            }
            if(Keyboard.current.pKey.wasPressedThisFrame)
            {
                hits.Play();
                RestartGame();
            }

        }
    }
    //�^�C�g���ɖ߂�
    public static void BackTitle()
    {
        Start_Flag = false;
        Setting_Flag = false;
        Clear_Flag = false;
        Exit.Now_Floor = 1;
        SceneManager.LoadScene("Title");
    }
    //�_���W�����ɐi��
    public static void StartGame()
    {
        SceneManager.LoadScene("Dungeon");
    }
    //�Q�[�����ŏ����炷��
    public void RestartGame()
    {
        Reset_Flag = true;
        Start_Flag = false;
        Setting_Flag = false;
        Clear_Flag = false;

        Exit.Now_Floor = 1;//�P�K�ɖ߂�
        panel = GameObject.Find("Reset");
        if(panel)
            panel.SetActive(false);
        Exit.Clear_Dungeon = 0;//�N���A�񐔂�0�ɂ���
        SceneManager.LoadScene("Title");
    }
    //�Q�[�����I������
    public void Exit_game()
    {
        Application.Quit();
    }
}