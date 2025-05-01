/*----------------------------------------------------------------------------
* Title: ZMUIFrameWork 一款Mono分离式UI管理框架
*
* Author: 铸梦xy
*
* Date: 2024/09/01 14:15:58
*
* Description: 高性能、自动化、自定义生命周期工作管线是该框架的特点，该框架属于MVC中的View层架构。
* 设计简洁清晰、轻便小巧，可以对接至任意重中小型游戏项目中。
*
* Remarks: QQ:975659933 邮箱：zhumengxyedu@163.com
*
* GitHub：https://github.com/ZMteacher?tab=repositories
----------------------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class WindowBehaviour
{
    public GameObject gameObject { get; set; } //当前窗口物体
    public Transform transform { get; set; } //代表自己
    public Canvas Canvas { get; set; }
    /// <summary>
    /// 窗口名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 是否在显示中
    /// </summary>
    public bool Visible { get; set; }
    /// <summary>
    /// 是否开启Update渲染帧更新
    /// </summary>
    public bool Update { get; protected set; }
    /// <summary>
    /// 是否是通过堆栈系统弹出的弹窗
    /// </summary>
    public bool PopStack { get; set; }
    /// <summary>
    /// 全屏窗口标志(在窗口Awake接口中进行设置,智能显隐开启后当全屏弹窗弹出时，被遮挡的窗口都会通过伪隐藏隐藏掉，从而提升性能)
    /// </summary>
    public bool FullScreenWindow { get; set; }

    public Action<WindowBase> PopStackListener { get; set; }
    /// <summary>
    ///只会在窗口创建时执行一次 ，与Mono Awake调用时机和次数保持一致
    /// </summary>
    public virtual void OnAwake() { }
    /// <summary>
    /// 在窗口每次显示时执行一次，与MonoOnEnable一致
    /// </summary>
    public virtual void OnShow() { }
    /// <summary>
    /// 渲染帧更新接口(需在Awake中把Update字段设置为True，对应窗口才会开启OnUpdate回调，防止性能滥用)
    /// </summary>
    public virtual void OnUpdate() { }
    /// <summary>
    /// 在窗口隐藏时执行一次，与Mono OnDisable 一致
    /// </summary>
    public virtual void OnHide() { }
    /// <summary>
    /// 在当前界面被销毁时调用一次
    /// </summary>
    public virtual void OnDestroy() { }
    /// <summary>
    /// 设置窗口的可见性
    /// </summary>
    /// <param name="isVisible"></param>
    public virtual void SetVisible(bool isVisible) { }
}
