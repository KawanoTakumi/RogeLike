using UnityEngine;

public class Anim_func : MonoBehaviour
{
    public AudioSource SSound;
    //アタッチされているオブジェクト削除
    public void DestroyAnimation()
    {
        Destroy( this.gameObject);
    }

    public void PlaySoundMusics()
    {
        SSound.Play();
    }
}