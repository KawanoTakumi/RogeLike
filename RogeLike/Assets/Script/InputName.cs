using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputName : MonoBehaviour
{
    public TextMeshProUGUI View_name;
    public TMP_InputField name_field;
    public GameObject IMP_Obj;

    public static string OutputName;
    public static bool Rename_flag = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        View_name.text = OutputName;
        IMP_Obj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        View_name.text = OutputName;
        if (View_name.text == "" || View_name.text == null)
            OutputName = "Player";
        Imput_nameing();

    }

    public void Imput_nameing()
    {
        //InputFierld��\��
        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            if (!Rename_flag)
            {
                Rename_flag = true;
                StartCoroutine(FocusInputFieldNextFrame());

            }
        }

        if (Rename_flag && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            //���O���X�V
            EventSystem.current.SetSelectedGameObject(null);
            Rename_flag = false;
            OutputName = name_field.text;
        }
        IMP_Obj.SetActive(Rename_flag);

    }
    IEnumerator FocusInputFieldNextFrame()
    {
        yield return null; // 1�t���[���҂�
        EventSystem.current.SetSelectedGameObject(name_field.gameObject,null);
    }
}