using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnState
{ 
    PlayerTurn,//プレイヤー
    EnemyTurn,//敵
    Waiting//待機状態
}


public class Turnmanager : MonoBehaviour
{
    public static Turnmanager Instance;

    public TurnState currentTurn = TurnState.PlayerTurn;
    public List<Enemys> enemies = new();

    private void Awake()
    {
        if (Instance == null) 
            Instance = this;
    }

    public void RegisterEnemy(Enemys enemy)
    {
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
        }
    }

    public void EndPlayerTurn()
    {
        //currentTurn = TurnState.EnemyTurn;
        //StartCoroutine(HandleEnemyTurn());
    }

    private IEnumerator HandleEnemyTurn()
    {
        foreach (Enemys enemy in enemies)
        {
            yield return enemy.TakeTurn();
        }

        currentTurn = TurnState.PlayerTurn;
    }

}