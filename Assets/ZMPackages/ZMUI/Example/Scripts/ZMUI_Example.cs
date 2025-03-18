using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZMUI_Example : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        //初始化UI框架
        UIModule.Instance.Initialize();
    }

    void Start()
    {
        //弹出窗口
        UIModule.Instance.PopUpWindow<StartDemoWindow>();
    }
  
}
