# ZMUI 组件绑定技能

## 描述
根据用户描述的 UI 组件需求，自动生成组件绑定代码和事件回调方法。支持 Button、Toggle、InputField 等 UGUI 组件的事件绑定。

## 适用场景
- 用户说"给 XX 窗口的 XX 按钮绑定点击事件"
- 用户说"为 XX 窗口添加一个 Toggle 开关，绑定状态变化事件"
- 用户说"给 XX 窗口的输入框绑定文本变化和结束编辑事件"
- 用户说"为 XX 窗口生成所有组件的事件绑定代码"

## 组件命名解析规则

框架支持两种解析方式（由 UISetting.ParseType 配置）：

### 1. 名称解析 (ParseType.Name) — 默认方式
组件字段命名格式：`[组件类型]字段名`
```
[Button]CloseButton       → public Button CloseButton;
[Text]TitleText           → public Text TitleText;
[Toggle]SwitchToggle      → public Toggle SwitchToggle;
[InputField]NameInput     → public InputField NameInput;
[TMP_InputField]ChatInput → public TMP_InputField ChatInput;
[Image]BgImage            → public Image BgImage;
[GameObject]EffectRoot    → public GameObject EffectRoot;
```

### 2. Tag 解析 (ParseType.Tag)
使用 Tag 来标识组件类型，框架自动扫描子物体。

## 事件绑定方法

### Button 点击事件
```csharp
// 在 DataComponent.InitComponent 中绑定
target.AddButtonClickListener(CloseButton, mWindow.OnCloseButtonClick);

// 在 Window 中定义回调方法
public void OnCloseButtonClick()
{
    HideWindow();
}
```

### Toggle 状态变化事件
```csharp
// 在 DataComponent.InitComponent 中绑定
target.AddToggleClickListener(SwitchToggle, mWindow.OnSwitchToggleChange);

// 在 Window 中定义回调方法
public void OnSwitchToggleChange(bool state, Toggle toggle)
{
    // state: 当前开关状态
    // toggle: 当前 Toggle 组件引用
}
```

### InputField 输入事件
```csharp
// 在 DataComponent.InitComponent 中绑定
target.AddInputFieldListener(NameInput, mWindow.OnNameInputChange, mWindow.OnNameInputEnd);

// 在 Window 中定义回调方法
public void OnNameInputChange(string text)
{
    // 文本变化时触发
}

public void OnNameInputEnd(string text)
{
    // 编辑结束时触发
}
```

### TMP_InputField 输入事件
```csharp
// 在 DataComponent.InitComponent 中绑定
target.AddTMPInputFieldListener(ChatInput, mWindow.OnChatInputChange, mWindow.OnChatInputEnd);

// 在 Window 中定义回调方法
public void OnChatInputChange(string text) { }
public void OnChatInputEnd(string text) { }
```

## 完整代码生成示例

### DataComponent 中的 InitComponent 完整绑定代码
```csharp
public void InitComponent(WindowBase target)
{
    HallDemoWindow mWindow = (HallDemoWindow)target;

    // Button 绑定
    target.AddButtonClickListener(CloseButton, mWindow.OnCloseButtonClick);
    target.AddButtonClickListener(HeadInfoButton, mWindow.OnHeadInfoButtonClick);
    target.AddButtonClickListener(PlotButton, mWindow.OnPlotButtonClick);

    // Toggle 绑定
    target.AddToggleClickListener(SwitchToggle, mWindow.OnSwitchToggleChange);

    // InputField 绑定
    target.AddInputFieldListener(NameInput, mWindow.OnNameInputChange, mWindow.OnNameInputEnd);
}
```

### Window 中的完整事件回调
```csharp
#region UI组件事件

public void OnCloseButtonClick()
{
    HideWindow();
}

public void OnSureButtonClick()
{
    // 确认逻辑
}

public void OnCancelButtonClick()
{
    HideWindow();
}

public void OnSwitchToggleChange(bool state, Toggle toggle)
{
    Debug.Log($"Toggle状态: {state}");
}

public void OnNameInputChange(string text)
{
    Debug.Log($"输入变化: {text}");
}

public void OnNameInputEnd(string text)
{
    Debug.Log($"输入结束: {text}");
}

#endregion
```

## 组件映射表

| 字段命名前缀 | 组件类型 | C# 类型 | 事件绑定方法 |
|------------|---------|---------|------------|
| [Button] | Button | UnityEngine.UI.Button | AddButtonClickListener |
| [Toggle] | Toggle | UnityEngine.UI.Toggle | AddToggleClickListener |
| [InputField] | InputField | UnityEngine.UI.InputField | AddInputFieldListener |
| [TMP_InputField] | TMP_InputField | TMPro.TMP_InputField | AddTMPInputFieldListener |
| [Text] | Text | UnityEngine.UI.Text | - (纯展示) |
| [Image] | Image | UnityEngine.UI.Image | - (纯展示) |
| [Slider] | Slider | UnityEngine.UI.Slider | - (纯展示) |
| [ScrollRect] | ScrollRect | UnityEngine.UI.ScrollRect | - (纯展示) |
| [GameObject] | GameObject | UnityEngine.GameObject | - (纯展示) |

## 回调方法命名约定

| 组件类型 | 回调方法名格式 | 参数 |
|---------|--------------|------|
| Button | `On{FieldName}ButtonClick()` | 无 |
| Toggle | `On{FieldName}ToggleChange(bool state, Toggle toggle)` | 状态 + 组件引用 |
| InputField | `On{FieldName}InputChange(string text)` | 文本 |
| InputField | `On{FieldName}InputEnd(string text)` | 文本 |
| TMP_InputField | `On{FieldName}InputChange(string text)` | 文本 |
| TMP_InputField | `On{FieldName}InputEnd(string text)` | 文本 |

## 参考示例

- `Assets/ZMPackages/ZMUI/Example/Scripts/AutoGenerate/Window/HallDemoWindow.cs` — 完整的事件回调示例
- `Assets/ZMPackages/ZMUI/Example/Scripts/AutoGenerate/BindCompoent/HallDemoWindowDataComponent.cs` — 完整的 InitComponent 绑定示例
- `Assets/ZMPackages/ZMUI/Default/SelectWindow.cs` — 确认/取消按钮事件示例

## 注意事项
1. 事件绑定必须在 DataComponent.InitComponent 中完成，在 Window 的 OnAwake 中调用
2. Button 的 onClick 在绑定前会调用 RemoveAllListeners() 清空，确保不会重复绑定
3. 窗口销毁时框架会自动移除所有事件监听（OnDestroy 中调用 RemoveAllButtonListener 等）
4. 如果使用 Bind 生成模式，DataComponent 由工具自动生成并挂载到预制体；如果使用 Find 模式，组件在运行时通过查找获取