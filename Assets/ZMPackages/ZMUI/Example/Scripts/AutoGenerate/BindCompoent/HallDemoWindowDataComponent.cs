/*---------------------------------
 *Title:UI自动化组件生成代码生成工具
 *Author:铸梦
 *Date:2025/3/18 23:33:20
 *Description:变量需要以[Text]括号加组件类型的格式进行声明，然后右键窗口物体—— 一键生成UI数据组件脚本即可
 *注意:以下文件是自动生成的，任何手动修改都会被下次生成覆盖,若手动修改后,尽量避免自动生成
---------------------------------*/
using UnityEngine;
using UnityEngine.UI;
namespace ZM.UI
{
	public class HallDemoWindowDataComponent:MonoBehaviour
	{
		public   Button  CloseButton;

		public   Image  LogoImage;

		public   Text  ContentDesText;
		
		public   Text  ZMUIFrameWorkText;

		public   Button  HeadInfoButton;

		public   Button  NoticeButton;

		public   CurrencyItem[] TopRightCurrencyItemArray;

		public   Button  PlotButton;

		public   Button  EphemerisButton;

		public   Button  DreamlandButton;

		public   Button  LaboratoryButton;

		public   Button  SupplyButton;

		public   Button  LaborUnionButton;

		public   Button  ManageButton;

		public  void InitComponent(WindowBase target)
		{
		     //组件事件绑定
		     HallDemoWindow mWindow=(HallDemoWindow)target;
		     target.AddButtonClickListener(CloseButton,mWindow.OnCloseButtonClick);
		     target.AddButtonClickListener(HeadInfoButton,mWindow.OnHeadInfoButtonClick);
		     target.AddButtonClickListener(NoticeButton,mWindow.OnNoticeButtonClick);
		     target.AddButtonClickListener(PlotButton,mWindow.OnPlotButtonClick);
		     target.AddButtonClickListener(EphemerisButton,mWindow.OnEphemerisButtonClick);
		     target.AddButtonClickListener(DreamlandButton,mWindow.OnDreamlandButtonClick);
		     target.AddButtonClickListener(LaboratoryButton,mWindow.OnLaboratoryButtonClick);
		     target.AddButtonClickListener(SupplyButton,mWindow.OnSupplyButtonClick);
		     target.AddButtonClickListener(LaborUnionButton,mWindow.OnLaborUnionButtonClick);
		     target.AddButtonClickListener(ManageButton,mWindow.OnManageButtonClick);
		}
	}
}
