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
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI事件派发中心
/// 由逻辑层调用，UI层接收
/// 代替直接交互，进行解耦
/// </summary>
public class UIEventControl
{
    /// <summary>
    /// 委托事件
    /// </summary>
    public delegate void EventHandler(object data);

    /// <summary>
    /// 事件派发注册字典
    /// </summary>
    private static Dictionary<UIEventEnum, List<EventHandler>> mEventDic = new Dictionary<UIEventEnum, List<EventHandler>>();

    /// <summary>
    /// 注册事件
    /// </summary>
    public static void AddEvent(UIEventEnum eventType, EventHandler eventHandler)
    {
        if (!mEventDic.ContainsKey(eventType))
        {
            mEventDic.Add(eventType, new List<EventHandler>());
        }
        if (!mEventDic[eventType].Contains(eventHandler))
        {
            mEventDic[eventType].Add(eventHandler);
        }
    }

    /// <summary>
    /// 移除事件
    /// </summary>
    public static void RemoveEvent(UIEventEnum eventType, EventHandler eventHandler)
    {
        if (mEventDic.TryGetValue(eventType, out List<EventHandler> eventList))
        {
            eventList.Remove(eventHandler);
            // 无订阅者时移除 key，避免空列表堆积
            if (eventList.Count == 0)
                mEventDic.Remove(eventType);
        }
    }

    /// <summary>
    /// 分发事件
    /// 修复：1. key 不存在时不再抛 NullReferenceException
    ///       2. 拷贝列表后迭代，防止回调内部调用 RemoveEvent 引发集合修改异常
    ///       3. 每个回调独立 try-catch，单个异常不影响其他订阅者执行
    /// </summary>
    public static void DispensEvent(UIEventEnum eventType, object data = null)
    {
        if (!mEventDic.TryGetValue(eventType, out List<EventHandler> eventList) || eventList.Count == 0)
            return;

        // 拷贝快照，防止回调中增删订阅者导致集合被修改
        EventHandler[] snapshot = eventList.ToArray();
        for (int i = 0; i < snapshot.Length; i++)
        {
            try
            {
                snapshot[i]?.Invoke(data);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIEventControl] 事件 {eventType} 的第 {i} 个回调执行异常：{e}");
            }
        }
    }

    /// <summary>
    /// 移除某事件类型下的所有订阅者
    /// </summary>
    public static void RemoveAllEvents(UIEventEnum eventType)
    {
        if (mEventDic.ContainsKey(eventType))
            mEventDic.Remove(eventType);
    }

    /// <summary>
    /// 清空全部事件（场景切换时调用）
    /// </summary>
    public static void ClearAllEvents()
    {
        mEventDic.Clear();
    }
}