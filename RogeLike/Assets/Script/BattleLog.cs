using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class BattleLog : MonoBehaviour
{
    public TextMeshProUGUI logText;//�퓬���O
    private Queue<string> log = new();//���O�ۑ��p�L���[
    private const int MAXLOG = 6;//�ߋ�6���܂ŕ\��

    public void ShowMessage(string text)
    {
        log.Enqueue(text);

        //�ő匏���ȏ�Ȃ�Â����O�폜
        if(log.Count > MAXLOG)
        {
            log.Dequeue();
        }
        //�\���X�V
        logText.text = string.Join("\n",log);
    }
}
