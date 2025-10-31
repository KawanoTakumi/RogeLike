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
            Destroy(gameObject); // 既存のインスタンスがある場合は破棄
            return;
        }

        Instance = this;
        Reset_Flag = true;
        DontDestroyOnLoad(gameObject); // シーンをまたいで保持

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
        //ゲーム内
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
        //設定中
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
        //負け中
        if (Lose_Flag)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                hits.Play();
                Start_Flag = false;
                Setting_Flag = false;
                Lose_Flag = false;
                Reset_Flag = false;
                Exit.Now_Floor = 1;//１階に戻す
                BackTitle();
            }
        }
        //勝ち中
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
    //タイトルに戻る
    public static void BackTitle()
    {
        Start_Flag = false;
        Setting_Flag = false;
        Clear_Flag = false;
        Exit.Now_Floor = 1;
        SceneManager.LoadScene("Title");
    }
    //ダンジョンに進む
    public static void StartGame()
    {
        SceneManager.LoadScene("Dungeon");
    }
    //ゲームを最初からする
    public void RestartGame()
    {
        Reset_Flag = true;
        Start_Flag = false;
        Setting_Flag = false;
        Clear_Flag = false;

        Exit.Now_Floor = 1;//１階に戻す
        panel = GameObject.Find("Reset");
        if(panel)
            panel.SetActive(false);
        Exit.Clear_Dungeon = 0;//クリア回数を0にする
        SceneManager.LoadScene("Title");
    }
    //ゲームを終了する
    public void Exit_game()
    {
        Application.Quit();
    }
}