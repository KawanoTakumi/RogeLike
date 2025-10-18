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
    string SceneName;
    public static GameState Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 既存のインスタンスがある場合は破棄
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // シーンをまたいで保持
    }
    private void Start()
    {        
        SceneManager.GetSceneByName(SceneName);
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
        //ゲーム内
        if(Start_Flag && Keyboard.current.gKey.wasPressedThisFrame)
        {
            if (!Setting_Flag)
                Setting_Flag = true;
            else
                Setting_Flag = false;
        }
        //設定中
        if(Setting_Flag)
        {
            if(Keyboard.current.rKey.wasPressedThisFrame)
            {
                Start_Flag = false;
                Setting_Flag = false;
                BackTitle();
            }
        }
        //負け中
        if(Lose_Flag)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                Start_Flag = false;
                Setting_Flag = false;
                Lose_Flag = false;
                Player_Cnt.StatusReset();//プレイヤーのステータスを戻す
                Exit.Now_Floor = 1;//１階に戻す
                BackTitle();
            }
        }
        //勝ち中
        if (Clear_Flag)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                Start_Flag = false;
                Setting_Flag = false;
                Clear_Flag = false;
                BackTitle();
            }
            //ステータスそのままで新しくゲームをする
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Start_Flag = false;
                Setting_Flag = false;
                Clear_Flag = false;
                StartGame();
            }
        }
    }
    //タイトルに戻る
    public static void BackTitle()
    {
        Debug.Log("タイトル");
        SceneManager.LoadScene("Title");
    }
    
    public static void StartGame()
    {
        Debug.Log("スタートゲーム");
        SceneManager.LoadScene("Dungeon");
    }
    //ゲームを最初からする
    public void RestartGame()
    {
        Player_Cnt.StatusReset();//プレイヤーのステータスを戻す
        Exit.Now_Floor = 1;//１階に戻す
        Exit.Clear_Dungeon = 0;//クリア回数を0にする
        SceneManager.LoadScene("Dungeon");
    }
    //ゲームを終了する
    public void Exit_game()
    {
        Player_Cnt.StatusReset();
        Application.Quit();
    }
}