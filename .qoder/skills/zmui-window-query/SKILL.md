# ZMUI 窗口查询技能

## 描述
查询 ZMUIFrameWork 项目中的窗口配置、预制体结构、UISetting 配置信息，帮助理解和浏览项目的 UI 架构。

## 适用场景
- 用户说"查看当前项目有哪些窗口"
- 用户说"检查 XX 窗口的配置和状态"
- 用户说"查看 UISetting 配置"
- 用户说"列出所有可见窗口"
- 用户说"查询窗口预制体存放位置"

## 框架配置结构

### 1. WindowConfig（窗口配置）
存储在 `WindowConfig.asset` 中，包含所有窗口预制体的路径映射。

```csharp
[CreateAssetMenu(fileName = "WindowConfig", menuName = "WindowConfig", order = 0)]
public class WindowConfig : ScriptableObject
{
    public List<WindowData> windowDataList = new List<WindowData>();

    // 获取窗口数据
    public WindowData GetWindowData(string wndName, bool log = true);
}

[System.Serializable]
public class WindowData
{
    public string name;  // 窗口名称（与预制体名一致）
    public string path;  // 预制体资源路径
}
```

**加载方式**: `Resources.Load<WindowConfig>("WindowConfig")`

### 2. UISetting（框架设置）
存储在 `UISetting.asset` 中，包含框架的全局配置。

```csharp
public class UISetting : ScriptableObject
{
    public bool SINGMASK_SYSTEM;           // 单遮罩模式
    public ParseType ParseType;            // 组件解析方式 (Name/Tag)
    public GeneratorType GeneratorType;    // 代码生成方式 (Find/Bind)
    public string BindComponentGeneratorPath;  // DataComponent 生成路径
    public string FindComponentGeneratorPath;  // Find 模式生成路径
    public string WindowGeneratorPath;     // Window 脚本生成路径
    public string ItemScriptsGeneratorPath;    // Item 脚本生成路径
    public string[] WindowPrefabFolderPathArr; // 窗口预制体存放目录列表
    public string[] UsingNameSpaceArr;     // 自动生成脚本的 using 命名空间
    public ComponentMapping[] ComponentMappings; // 组件映射表
}
```

**加载方式**: `Resources.Load<UISetting>("UISetting")`

### 3. UIModule（运行时窗口管理）
```csharp
public class UIModule
{
    // 所有已克隆的窗口（包含显示和隐藏）
    private Dictionary<string, WindowBase> mAllWindowDic;
    private List<WindowBase> mAllWindowList;

    // 所有可见窗口
    private List<WindowBase> mVisibleWindowList;

    // 窗口弹出堆栈
    private List<WindowBase> mWindowStack;

    // 查询接口
    public T GetWindow<T>() where T : WindowBase;
    public WindowBase GetWindow(string winName);
}
```

## 项目路径规划

框架的路径配置遵循以下约定：

```
Assets/ZMPackages/ZMUI/                    # 框架根目录
├── Runtime/                               # 运行时代码
│   ├── Core/UIModule.cs                   # 核心管理器
│   ├── Base/WindowBase.cs                 # 窗口基类
│   ├── Base/WindowBehaviour.cs            # 窗口抽象基类
│   ├── Base/WindowConfig.cs               # 窗口配置
│   ├── Event/UIEventControl.cs            # 事件系统
│   ├── Agent/UGUIAgent.cs                 # 扩展方法
│   ├── Adaptation/                        # 适配组件
│   └── SupperView/                        # 列表/网格视图
├── Editor/                                # 编辑器工具
│   ├── Generator/                         # 代码生成器
│   ├── Settings/                          # 设置面板
│   └── UGUI-Editor/                       # UGUI 编辑器扩展
├── Resources/                             # 资源文件
│   ├── UISetting.asset                    # 框架设置
│   ├── WindowConfig.asset                 # 窗口配置
│   └── Temp/                              # 临时资源
├── Default/                               # 内置窗口
│   ├── Toast.cs / ToastManager.cs         # 提示框
│   └── SelectWindow.cs                    # 选择弹窗
├── Example/                               # 示例
│   └── Scripts/AutoGenerate/              # 自动生成的示例
│       ├── Window/                        # 窗口脚本
│       ├── BindCompoent/                  # 数据组件
│       └── Item/                          # 列表项
└── AOT/                                   # AOT 程序集
```

## 窗口状态查询

### 运行时窗口状态
```csharp
// 获取所有已加载的窗口
UIModule.Instance.GetWindow<MyWindow>();  // 仅获取可见窗口中的

// 检查窗口是否已加载
WindowBase win = UIModule.Instance.GetWindow("WindowName");

// 通过 UIModule 内部数据查看
// mAllWindowDic — 所有已克隆的窗口
// mVisibleWindowList — 当前可见的窗口
// mWindowStack — 堆栈中的窗口
```

### 窗口配置查询
```csharp
// 通过 WindowConfig 查询
WindowConfig config = Resources.Load<WindowConfig>("WindowConfig");
WindowData data = config.GetWindowData("WindowName");
// data.name — 窗口名称
// data.path — 预制体资源路径

// 遍历所有已注册窗口
foreach (var windowData in config.windowDataList)
{
    Debug.Log($"窗口: {windowData.name}, 路径: {windowData.path}");
}
```

## 常见问题排查

### 窗口找不到预制体
- 检查窗口名是否与预制体文件名一致
- 检查预制体是否在 UISetting.WindowPrefabFolderPathArr 配置的目录下
- 检查 WindowConfig 是否已生成（编辑器模式下自动生成，手动模式下需调用 GeneratorWindowConfig）

### 窗口无法弹出
- 确认窗口类有无参构造函数（new() 约束）
- 确认窗口预制体存在且 Resources 可加载
- 检查 UIModule 是否已初始化（调用 Initialize）

### 组件绑定失败
- 确认 DataComponent 已挂载到预制体根节点
- 确认组件字段命名格式正确：[类型]字段名
- 确认 InitComponent 在 OnAwake 中被调用

## 参考文件
- `Assets/ZMPackages/ZMUI/Runtime/Core/UIModule.cs`
- `Assets/ZMPackages/ZMUI/Runtime/Base/WindowConfig.cs`
- `Assets/ZMPackages/ZMUI/AOT/UISetting.cs`
- `Assets/ZMPackages/ZMUI/Resources/WindowConfig.asset`
- `Assets/ZMPackages/ZMUI/Resources/UISetting.asset`

## 注意事项
1. WindowConfig.asset 和 UISetting.asset 是 ScriptableObject，需要从 Resources 加载
2. 编辑器模式下自动调用 GeneratorWindowConfig 刷新窗口列表
3. 热更场景下通过 AddAOTWindowMetadata 添加额外窗口元数据