using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatus", menuName = "Scriptable Objects/CharacterStatus")]
public class CharacterStatus : ScriptableObject
{
    [Header("��{�X�e�[�^�X")]
    public string charaName = "None";//���O
    public int maxHP = 1;//�ő�HP
    public int HP = 1;//���݂�HP
    public int attack = 1;//�U����
    public int diffence = 1;//�h���
    public int attackRange = 1;//�U���͈�
    [Header("�G��p")]
    public int searchRange = 0;//���G�͈�
    public int exp = 1;//�l���o���l
}