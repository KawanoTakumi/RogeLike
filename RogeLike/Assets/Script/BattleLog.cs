using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class BattleLog : MonoBehaviour
{
    public TextMeshProUGUI logText;//戦闘ログ
    private Queue<string> log = new();//ログ保存用キュー
    private const int MAXLOG = 6;//過去6件まで表示

    public void ShowMessage(string text)
    {
        log.Enqueue(text);

        //最大件数以上なら古いログ削除
        if(log.Count > MAXLOG)
        {
            log.Dequeue();
        }
        //表示更新
        logText.text = string.Join("\n",log);
    }
}
