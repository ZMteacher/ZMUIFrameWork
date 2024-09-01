using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZMUI_Example : MonoBehaviour
{
   
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        //��ʼ�����
        UIModule.Instance.Initialize();
        //����UIʾ������
        UIModule.Instance.PopUpWindow<ExampleWindow>();
        //Invoke("DelayMethod",3);
    }

    public void DelayMethod()
    {
        //UIModule.Instance.HideWindow<ExampleWindow>();
    }

    private void Update()
    {
        //�ػ����
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UIModule.Instance.GetWindow<ExampleWindow>().dataCompt.TestButton.SetVisible(true);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            UIModule.Instance.GetWindow<ExampleWindow>().dataCompt.TestButton.SetVisible(false);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            UIModule.Instance.GetWindow<ExampleWindow>().dataCompt.TestButton.gameObject.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            UIModule.Instance.GetWindow<ExampleWindow>().dataCompt.TestButton.gameObject.SetActive(false);
        }
    }
}
