using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatus", menuName = "Scriptable Objects/CharacterStatus")]
public class CharacterStatus : ScriptableObject
{
    [Header("基本ステータス")]
    public string charaName = "None";//名前
    public int level = 1;//レベル
    public int maxHP = 1;//最大HP
    public int HP = 1;//現在のHP
    public int attack = 1;//攻撃力
    public int diffence = 1;//防御力
    public int attackRange = 1;//攻撃範囲
    public int level_exp = 0;//レベルアップに必要な経験値
    [Header("敵専用")]
    public int searchRange = 0;//索敵範囲
    public int exp = 1;//獲得経験値
}