/*---------------------------------
 *Title:UI表现层脚本自动化生成工具
 *Author:ZM 铸梦
 *Date:2025/3/23 15:04:13
 *Description:UI 表现层，该层只负责界面的交互、表现相关的更新，不允许编写任何业务逻辑代码
 *注意:以下文件是自动生成的，再次生成不会覆盖原有的代码，会在原有的代码上进行新增，可放心使用
---------------------------------*/

using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace ZM.UI
{
    public class TaskDemoWindow : WindowBase
    {
        public TaskDemoWindowDataComponent dataCompt;

        
        private List<TaskData> m_TaskDataList = new List<TaskData>();
        #region 生命周期函数

        //调用机制与Mono Awake一致
        public override void OnAwake()
        {
            dataCompt = gameObject.GetComponent<TaskDemoWindowDataComponent>();
            dataCompt.InitComponent(this);
            FullScreenWindow = true;//设置全屏窗口标记
            mDisableAnim = true;//设置动画禁用
            base.OnAwake();
            for (int i = 0; i < 100; i++)
            {
                m_TaskDataList.Add(new TaskData{ id= i,chapter = i,curProgress = Random.Range(0,51),maxProgress = 50});
            }
        }

        //物体显示时执行
        public override void OnShow()
        {
            base.OnShow();
            RefreshViewList();
        }

        //物体隐藏时执行
        public override void OnHide()
        {
            base.OnHide();
            dataCompt.taskZmuiListView.OnRelease();
        }

        //物体销毁时执行
        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        #endregion

        #region API Function
        /// <summary>
        /// 刷新无限滚动列表 必须等到 m_TaskDataList数据向服务器索取结束时调用
        /// </summary>
        private void RefreshViewList()
        {
            dataCompt.taskZmuiListView.RefreshListView(true,m_TaskDataList.Count,OnGetListDataCallBack);
        }
        /// <summary>
        /// 根据item索引获取无限滚动数据
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object OnGetListDataCallBack(int index)
        {
            Debug.Log($"GetListDataCallBack index:{index}");
            return m_TaskDataList[index];
        }

        #endregion

        #region UI组件事件

        public void OnCloseButtonClick()
        {
            HideWindow();
        }

        public void OnGotoButtonClick()
        {
        }

        #endregion
    }
    /// <summary>
    /// 任务数据(声明在这是演示无限滚动列表使用案例，合理的声明应该在数据层，可以参考老师的DMVC框架，当前ZMUI属于DMVC中的V)
    /// </summary>
    public class TaskData
    {
        /// <summary>
        /// 唯一id
        /// </summary>
        public int id;
        /// <summary>
        /// 章节
        /// </summary>
        public int chapter;
        /// <summary>
        /// 当前进度
        /// </summary>
        public int curProgress;
        /// <summary>
        /// 最大进度
        /// </summary>
        public int maxProgress;
    }
}