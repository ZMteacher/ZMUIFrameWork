# ZMUI 窗口创建技能

## 描述
根据用户描述的窗口 UI 需求，自动生成完整的 ZMUI 窗口脚本，包括 WindowBase 子类和 DataComponent 组件绑定脚本。

## 适用场景
- 用户说"创建一个新的 XX 窗口，包含 XX 按钮、XX 文本框等"
- 用户说"帮我生成一个弹窗，有确认和取消按钮"
- 用户说"新建一个全屏窗口，包含..."

## 框架架构参考

### 窗口生命周期 (WindowBase -> WindowBehaviour)
```
OnAwake()   - 只执行一次，初始化组件与数据
OnShow()    - 每次窗口显示时执行
OnUpdate()  - 渲染帧更新（需设置 Update=true 才开启）
OnHide()    - 窗口隐藏时执行
OnDestroy() - 窗口销毁时执行
```

### 窗口使用方式
```csharp
// 弹出窗口
UIModule.Instance.PopUpWindow<MyWindow>();

// 获取已弹出的窗口
UIModule.Instance.GetWindow<MyWindow>();

// 隐藏窗口
UIModule.Instance.HideWindow<MyWindow>();

// 销毁窗口
UIModule.Instance.DestroyWinodw<MyWindow>();
```

### 关键配置
- `FullScreenWindow = true` — 标记为全屏窗口，智能显隐将自动隐藏被遮挡窗口
- `Update = true` — 开启 OnUpdate 回调（默认关闭以节省性能）
- `mDisableAnim = true` — 禁用窗口弹出动画

## 代码生成模板

### Window 脚本模板 (生成到 UISetting.WindowGeneratorPath)
```csharp
/*---------------------------------
 *Title:UI表现层脚本自动化生成工具
 *Author:ZM 铸梦
 *Date:{DateTime.Now}
 *Description:UI 表现层，该层只负责界面的交互、表现相关的更新，不允许编写任何业务逻辑代码
 *注意:以下文件是自动生成的，再次生成不会覆盖原有的代码，会在原有的代码上进行新增，可放心使用
 ---------------------------------*/
using UnityEngine.UI;
using UnityEngine;

namespace ZM.UI
{
    public class {WindowName} : WindowBase
    {
        public {WindowName}DataComponent dataCompt;

        #region 生命周期函数

        //调用机制与Mono Awake一致
        public override void OnAwake()
        {
            dataCompt = gameObject.GetComponent<{WindowName}DataComponent>();
            dataCompt.InitComponent(this);

            // 可选：开启Update渲染帧更新
            // Update = true;

            // 可选：标记为全屏窗口（智能显隐）
            // FullScreenWindow = true;

            // 可选：禁用窗口弹出动画
            // mDisableAnim = true;

            base.OnAwake();
        }

        //物体显示时执行
        public override void OnShow()
        {
            base.OnShow();
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

        // 示例：关闭按钮
        public void OnCloseButtonClick()
        {
            HideWindow();
        }

        // 示例：确认按钮
        public void OnSureButtonClick()
        {
        }

        // 示例：取消按钮
        public void OnCancelButtonClick()
        {
        }

        #endregion
    }
}
```

### DataComponent 模板 (生成到 UISetting.BindComponentGeneratorPath)
```csharp
/*---------------------------------
 *Title:UI自动化组件生成代码生成工具
 *Author:铸梦
 *Date:{DateTime.Now}
 *Description:变量需要以[Text]括号加组件类型的格式进行声明，然后右键窗口物体—— 一键生成UI数据组件脚本即可
 *注意:以下文件是自动生成的，任何手动修改都会被下次生成覆盖,若手动修改后,尽量避免自动生成
 ---------------------------------*/
using UnityEngine;
using UnityEngine.UI;

namespace ZM.UI
{
    public class {WindowName}DataComponent : MonoBehaviour
    {
        // 声明组件字段 — 命名规则: [组件类型]字段名
        // 例如: public Button CloseButton;
        //       public Text TitleText;
        //       public Image BgImage;
        //       public Toggle SwitchToggle;
        //       public InputField NameInputField;

        public void InitComponent(WindowBase target)
        {
            // 组件事件绑定
            {WindowName} mWindow = ({WindowName})target;

            // 事件绑定规则:
            // Button    -> target.AddButtonClickListener(btn, mWindow.OnBtnNameButtonClick)
            // Toggle    -> target.AddToggleClickListener(toggle, mWindow.OnToggleNameToggleChange)
            // InputField -> target.AddInputFieldListener(input, mWindow.OnInputNameInputChange, mWindow.OnInputNameInputEnd)
        }
    }
}
```

## 组件映射表
| 命名前缀 | C# 组件类型 |
|---------|------------|
| Text | Text |
| Image | Image |
| RawImage | RawImage |
| Button | Button |
| InputField | InputField |
| TMP_InputField | TMP_InputField |
| Toggle | Toggle |
| Slider | Slider |
| Scrollbar | Scrollbar |
| Dropdown | Dropdown |
| ScrollRect | ScrollRect |
| LoopListView2 | LoopListView2 |
| GameObject | GameObject |

## 完整示例参考

参考项目中的现有实现：
- `Assets/ZMPackages/ZMUI/Example/Scripts/AutoGenerate/Window/HallDemoWindow.cs`
- `Assets/ZMPackages/ZMUI/Example/Scripts/AutoGenerate/BindCompoent/HallDemoWindowDataComponent.cs`
- `Assets/ZMPackages/ZMUI/Default/SelectWindow.cs`
- `Assets/ZMPackages/ZMUI/Default/SelectWindowDataComponent.cs`

## 注意事项
1. 窗口类名必须与预制体名完全一致
2. DataComponent 必须挂载到预制体根节点
3. 窗口预制体必须放在 UISetting.WindowPrefabFolderPathArr 配置的目录下
4. 组件字段命名遵循 [类型]字段名 格式，例如 [Button]CloseButton
5. 生成的 Window 脚本在再次生成时不会覆盖已有代码，DataComponent 会完全覆盖