using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZMUI_Example : MonoBehaviour
{
   
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        //初始化框架
        UIModule.Instance.Initialize();
        //弹出UI示例窗口
        UIModule.Instance.PopUpWindow<ExampleWindow>();
        //Invoke("DelayMethod",3);
    }

    public void DelayMethod()
    {
        //UIModule.Instance.HideWindow<ExampleWindow>();
    }

    private void Update()
    {
        //重绘测试
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
