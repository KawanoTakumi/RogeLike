using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
    PlayerControl Player_Cnt;
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
            if(Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Start_Flag = true;
                StartGame();
            }
            if(Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Exit_game();
            }
        }
        //�Q�[����
        if(Start_Flag && Keyboard.current.gKey.wasPressedThisFrame)
        {
            if (!Setting_Flag)
                Setting_Flag = true;
            else
                Setting_Flag = false;
        }
        //�ݒ蒆
        if(Setting_Flag)
        {
            if(Keyboard.current.rKey.wasPressedThisFrame)
            {
                Start_Flag = false;
                Setting_Flag = false;
                BackTitle();
            }
        }
        //������
        if(Lose_Flag)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
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
                Start_Flag = false;
                Setting_Flag = false;
                Clear_Flag = false;
                Exit.Now_Floor = 1;
                BackTitle();
            }
            if(Keyboard.current.eKey.wasPressedThisFrame)
            {
                Start_Flag = false;
                Setting_Flag = false;
                Clear_Flag = false;
                Exit.Now_Floor = 1;
                RestartGame();

            }

        }
    }
    //�^�C�g���ɖ߂�
    public static void BackTitle()
    {
        SceneManager.LoadScene("Title");
    }
    
    public static void StartGame()
    {
        SceneManager.LoadScene("Dungeon");
    }
    //�Q�[�����ŏ����炷��
    public void RestartGame()
    {
        Reset_Flag = true;
        Exit.Now_Floor = 1;//�P�K�ɖ߂�
        Exit.Clear_Dungeon = 0;//�N���A�񐔂�0�ɂ���
        SceneManager.LoadScene("Dungeon");
    }
    //�Q�[�����I������
    public void Exit_game()
    {
        Application.Quit();
    }
}