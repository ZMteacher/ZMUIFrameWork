/*---------------------------------
 *Title:UI表现层脚本自动化生成工具
 *Author:ZM 铸梦
 *Date:2025/3/18 23:28:53
 *Description:UI 表现层，该层只负责界面的交互、表现相关的更新，不允许编写任何业务逻辑代码
 *注意:以下文件是自动生成的，再次生成不会覆盖原有的代码，会在原有的代码上进行新增，可放心使用
---------------------------------*/

using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;

namespace ZM.UI
{
    public class HallDemoWindow : WindowBase
    {
        public HallDemoWindowDataComponent dataCompt;

        #region 生命周期函数

        //调用机制与Mono Awake一致
        public override void OnAwake()
        {
            dataCompt = gameObject.GetComponent<HallDemoWindowDataComponent>();
            dataCompt.InitComponent(this);
            base.OnAwake();
        }

        //物体显示时执行
        public override void OnShow()
        {
            base.OnShow();
            dataCompt.ZMUIFrameWorkText.DOText("ZMUI Framwork", 1.5f).OnComplete(() =>
            {
                dataCompt.ContentDesText.DOText("是一款高性能、自动化、高流畅\n经过百万DAU商业项目验证的UI框架", 5);
            });
        }

        //物体隐藏时执行
        public override void OnHide()
        {
            base.OnHide();
        }

        //物体销毁时执行
        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        #endregion

        #region API Function

        #endregion

        #region UI组件事件

        public void OnCloseButtonClick()
        {
            HideWindow();
        }

        public void OnHeadInfoButtonClick()
        {
            UIModule.Instance.PopUpWindow<SelectWindow>().InitViewState(SelectType.Only_OK,"铸梦老师是一个非常非常非常帅气的男人！");
        }

        public void OnNoticeButtonClick()
        {
        }

        public void OnPlotButtonClick()
        {
            PopUpWindow<TaskDemoWindow>();
        }

        public void OnEphemerisButtonClick()
        {
            PopUpWindow<RoleDemoWindow>();
        }

        public void OnDreamlandButtonClick()
        {
        }

        public void OnLaboratoryButtonClick()
        {
        }

        public void OnSupplyButtonClick()
        {
            ToastManager.ShowToast("尚未解锁");
        }

        public void OnLaborUnionButtonClick()
        {
            ToastManager.ShowToast("尚未解锁");
        }

        public void OnManageButtonClick()
        {
            ToastManager.ShowToast("尚未解锁");
        }

        #endregion
    }
}