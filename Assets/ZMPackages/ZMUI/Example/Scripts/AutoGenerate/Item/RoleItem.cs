/*---------------------------------
 *Title:UI自动化组件生成代码生成工具
 *Author:铸梦
 *Date:2025/3/23 22:20:21
 *Description:变量需要以[Text]括号加组件类型的格式进行声明，然后右键窗口物体—— 一键生成UI数据组件脚本即可
 *注意:以下文件是自动生成的，再次生成后会以代码追加的形式新增,若手动修改后,尽量避免自动生成
---------------------------------*/
using UnityEngine;
using UnityEngine.UI;
using SuperScrollView;

namespace ZM.UI
{
	public class RoleItem:MonoBehaviour,IZMUIViewListItem
	{
		#region 自定义字段
		public Image iconImage;
		
		#endregion


		#region 生命周期
		//脚本初始化接口 (为保证生命周期的执行顺序，请在View层调用该接口确保需要初始化的数据正常执行)
		

		public void OnInitialize()
		{
			//按钮事件自动注册绑定
		}
		//物体设置数据接口 (请自定以你的参数，方便外部调用传参)
		public  void SetItemData()
		{
		}
		//物体销毁时执行 (为保证生命周期的执行顺序，请在View层调用该接口确保需要释放时的接口正常调用)
		public  void OnDispose()
		{
		}
		#endregion


		#region UI组件事件
		
		#endregion

		public void InitListItem()
		{
			 
		}

		public void SetListItemShowData(int index, params object[] data)
		{ 
			Debug.Log("index:"+index);
			 iconImage.sprite = Resources.Load<Sprite>($"RoleImage/{index+1}");
		}

		public void OnRelease()
		{
			  
		}
		
	}
}
