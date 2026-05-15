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
///
/// 性能设计：
///   - DispensEvent 遍历原始列表，零堆内存分配
///   - 派发期间若调用 AddEvent/RemoveEvent，操作会被缓存
///   - 派发结束后统一应用，彻底避免"集合被修改"异常
/// </summary>
public class UIEventControl
{
    public delegate void EventHandler(object data);

    private static readonly Dictionary<UIEventEnum, List<EventHandler>> mEventDic
        = new Dictionary<UIEventEnum, List<EventHandler>>();

    // 当前正在派发的事件类型（支持嵌套派发）
    private static int mDispatchingDepth = 0;

    // 派发期间缓存的待处理操作
    private static readonly List<(bool isAdd, UIEventEnum type, EventHandler handler)> mPendingOps
        = new List<(bool, UIEventEnum, EventHandler)>();

    /// <summary>
    /// 注册事件
    /// </summary>
    public static void AddEvent(UIEventEnum eventType, EventHandler eventHandler)
    {
        if (mDispatchingDepth > 0)
        {
            // 派发中：延迟到派发结束后添加
            mPendingOps.Add((true, eventType, eventHandler));
            return;
        }
        AddEventInternal(eventType, eventHandler);
    }

    /// <summary>
    /// 移除事件
    /// </summary>
    public static void RemoveEvent(UIEventEnum eventType, EventHandler eventHandler)
    {
        if (mDispatchingDepth > 0)
        {
            // 派发中：延迟到派发结束后移除，避免修改正在遍历的列表
            mPendingOps.Add((false, eventType, eventHandler));
            return;
        }
        RemoveEventInternal(eventType, eventHandler);
    }

    /// <summary>
    /// 分发事件（零 GC Alloc）
    /// </summary>
    public static void DispensEvent(UIEventEnum eventType, object data = null)
    {
        if (!mEventDic.TryGetValue(eventType, out List<EventHandler> eventList) || eventList.Count == 0)
            return;

        mDispatchingDepth++;
        try
        {
            for (int i = 0; i < eventList.Count; i++)
            {
                try
                {
                    eventList[i]?.Invoke(data);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[UIEventControl] 事件 {eventType} 第 {i} 个回调执行异常：{e}");
                }
            }
        }
        finally
        {
            mDispatchingDepth--;
            // 最外层派发结束后，统一应用期间缓存的增删操作
            if (mDispatchingDepth == 0 && mPendingOps.Count > 0)
            {
                ApplyPendingOperations();
            }
        }
    }

    /// <summary>
    /// 移除某事件类型的所有订阅者
    /// </summary>
    public static void RemoveAllEvents(UIEventEnum eventType)
    {
        if (mDispatchingDepth > 0)
        {
            Debug.LogWarning($"[UIEventControl] 派发中不支持 RemoveAllEvents，请在派发结束后调用");
            return;
        }
        mEventDic.Remove(eventType);
    }

    /// <summary>
    /// 清空全部事件（场景切换时调用）
    /// </summary>
    public static void ClearAllEvents()
    {
        mEventDic.Clear();
        mPendingOps.Clear();
        mDispatchingDepth = 0;
    }

    // ── 内部方法 ────────────────────────────────────────

    private static void AddEventInternal(UIEventEnum eventType, EventHandler eventHandler)
    {
        if (!mEventDic.ContainsKey(eventType))
            mEventDic.Add(eventType, new List<EventHandler>());

        if (!mEventDic[eventType].Contains(eventHandler))
            mEventDic[eventType].Add(eventHandler);
    }

    private static void RemoveEventInternal(UIEventEnum eventType, EventHandler eventHandler)
    {
        if (!mEventDic.TryGetValue(eventType, out List<EventHandler> eventList))
            return;

        eventList.Remove(eventHandler);
        if (eventList.Count == 0)
            mEventDic.Remove(eventType);
    }

    private static void ApplyPendingOperations()
    {
        for (int i = 0; i < mPendingOps.Count; i++)
        {
            var (isAdd, type, handler) = mPendingOps[i];
            if (isAdd)
                AddEventInternal(type, handler);
            else
                RemoveEventInternal(type, handler);
        }
        mPendingOps.Clear();
    }
}