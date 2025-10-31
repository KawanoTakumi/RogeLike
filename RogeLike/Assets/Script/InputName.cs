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
    public TextMeshProUGUI name_guide;

    public static string OutputName;
    public static bool Rename_flag = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        View_name.text = OutputName;
        name_guide.text = "N : Rename";
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
                name_guide.text = "LeftShift : Apply";
            }
        }

        if (Rename_flag && Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            //���O���X�V
            EventSystem.current.SetSelectedGameObject(null);
            Rename_flag = false;
            OutputName = name_field.text;
            name_guide.text = "N : Rename";
        }
        IMP_Obj.SetActive(Rename_flag);

    }
    IEnumerator FocusInputFieldNextFrame()
    {
        yield return null; // 1�t���[���҂�
        EventSystem.current.SetSelectedGameObject(name_field.gameObject,null);
    }
}