using UnityEngine;

public class Anim_func : MonoBehaviour
{
    public AudioSource SSound;
    //�A�^�b�`����Ă���I�u�W�F�N�g�폜
    public void DestroyAnimation()
    {
        Destroy( this.gameObject);
    }

    public void PlaySoundMusics()
    {
        SSound.Play();
    }
}