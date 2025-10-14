using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatus", menuName = "Scriptable Objects/CharacterStatus")]
public class CharacterStatus : ScriptableObject
{
    [Header("Šî–{ƒXƒe[ƒ^ƒX")]
    public string charaName = "None";//–¼‘O
    public int maxHP = 1;//Å‘åHP
    public int HP = 1;//Œ»İ‚ÌHP
    public int attack = 1;//UŒ‚—Í
    public int diffence = 1;//–hŒä—Í
    public int attackRange = 1;//UŒ‚”ÍˆÍ
    [Header("“Gê—p")]
    public int searchRange = 0;//õ“G”ÍˆÍ
    public int exp = 1;//Šl“¾ŒoŒ±’l
}