<div align="center">

<img src="https://raw.githubusercontent.com/ZMteacher/ZMUIFrameWork/main/docs/newimage.png" alt="ZMUIFrameWork Banner" width="100%"/>

# ZMUIFrameWork

**国内领先的 Mono 分离式 Unity UI 框架 · 经商业项目验证**

[![Unity](https://img.shields.io/badge/Unity-2019.1%2B-black?logo=unity)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Version](https://img.shields.io/badge/Version-1.0.0-brightgreen.svg)]()
[![CSDN](https://img.shields.io/badge/Blog-CSDN-red?logo=csdn)](https://blog.csdn.net/qq_42461824/article/details/116570321)
[![Docs](https://img.shields.io/badge/API%20文档-zm--doc.com-orange)](http://www.zm-doc.com)

> 高流畅 · 高性能 · 自动化 · 彻底解放双手，节约 **70%** 开发时间

</div>

---

## ✨ 框架简介

**ZMUIFrameWork** 是一套经过商业项目验证、设计成熟的 **Mono 分离式 UI 管理框架**，属于 MVC 架构中的 **View 层**。

框架采用内存映射设计，彻底脱离 `MonoBehaviour` 生命周期束缚，将窗口从生成到销毁的全流程掌握在开发者手中。设计简洁清晰、轻便小巧，可无缝对接中小型至大型商业游戏项目。

### 核心优势

| 特性 | 说明 |
|------|------|
| 🚀 **Mono 分离架构** | 窗口无需挂载任何脚本，彻底摆脱 MonoBehaviour 生命周期紊乱问题 |
| ⚡ **极致性能** | 智能显隐系统自动优化渲染批次，从底层解决 UI 卡顿、掉帧问题 |
| 🤖 **高度自动化** | 组件查找代码自动生成、组件自动拖拽赋值、事件自动绑定，一条龙服务 |
| 🔥 **热更新支持** | 完整支持 HybridCLR (il2cpp) AOT 热更新方案 |
| 🛠️ **Editor 增强** | 丰富的 Editor 工具，大幅提升开发效率 |

---

## 🏗️ 七大核心系统

### 1. 🪟 Mono 分离式 UI 管理系统

脱离 Mono 的分离式架构，以内存映射方式设计框架。  
可自定义窗口渲染管线中的**任意生命周期函数**，窗口可在不挂载任何脚本的情况下正常执行弹出、销毁及生命周期调用。

```csharp
// 弹出窗口
UIModule.Instance.PopUpWindow<MainMenuWindow>();

// 关闭窗口
UIModule.Instance.CloseWindow<MainMenuWindow>();

// 预加载窗口（仅加载不显示）
UIModule.Instance.PreLoadWindow<LoadingWindow>();
```

### 2. 📚 堆栈系统

成熟的堆栈管理，支持：
- 首页堆栈弹出
- 顺序控制
- 半路压入
- 全局任意地方调用

```csharp
// 压入堆栈并顺序弹出
UIModule.Instance.PushWindowToStack<WindowA>();
UIModule.Instance.PushWindowToStack<WindowB>();
UIModule.Instance.PushWindowToStack<WindowC>();
UIModule.Instance.StartPopUpStack(); // 依次弹出 A → B → C
```

### 3. 🎭 遮罩系统

完善的遮罩处理方案，提供两种模式无缝切换：
- **单遮模式**：仅对当前窗口添加遮罩
- **叠遮模式**：多窗口叠加时的遮罩层级管理

### 4. 📐 层级系统

清晰透明的 UI 层级管理，彻底解决：
- UI 叠加层级混乱问题
- 特效、模型与 UI 的穿插问题

### 5. ⚡ 高性能系统

针对 Unity UGUI 的极致性能优化：

- **智能显隐（SmartShowHide）**：当全屏窗口打开时，自动伪隐藏被遮挡的窗口，使其不参与渲染计算，大幅提升帧率
- 被伪隐藏的窗口在逻辑上仍属于显示状态，关闭上层窗口后自动恢复
- 被伪隐藏的窗口不触发 UGUI 重绘，不参与渲染运算，不影响帧率

### 6. 🤖 自动化系统

一键生成所有 UI 相关代码，提供完整自动化服务：

- ✅ 自动生成组件查找代码
- ✅ 组件自动拖拽赋值绑定
- ✅ 字段自动生成与递增插入
- ✅ 方法自动声明
- ✅ 事件接口自动声明
- ✅ UI 组件事件绑定
- ✅ Item 脚本递增式自动生成
- ✅ 组件数组自动生成和赋值
- ✅ 节点过滤功能（支持 `#` 标记过滤节点）

### 7. 🛠️ 高效率系统

- 预制体管理系统
- 预制体模板拖拽
- Editor 编译器增强
- 菜单栏 `ZMFrame → ZMUI Setting`，支持自定义引入命名空间配置

---

## 📦 安装方式

### 方式一：Unity Package Manager（推荐）

在 Unity 编辑器中打开 **Package Manager**，点击左上角 `+` → `Add package from git URL`，输入：

```
https://github.com/ZMteacher/ZMUIFrameWork.git
```

### 方式二：手动导入

1. Clone 或下载本仓库
2. 将 `Assets/ZMPackages` 文件夹复制到你的项目中

### 环境要求

| 环境 | 版本要求 |
|------|---------|
| Unity | 2019.1 或更高 |
| .NET | Standard 2.0+ |
| Odin Inspector | 推荐安装（Editor 工具依赖）|

---

## 🚀 快速开始

### 第一步：初始化框架

```csharp
void Start()
{
    // 确保场景中有 UICamera 和 UIRoot 节点
    UIModule.Instance.Initialize();
}
```

### 第二步：创建窗口类

```csharp
public class MainMenuWindow : WindowBase
{
    // 窗口唤醒时调用（对应 Awake）
    public override void OnAwake()
    {
        base.OnAwake();
        // 初始化组件引用
    }

    // 窗口显示时调用（对应 OnEnable）
    public override void OnShow()
    {
        base.OnShow();
    }

    // 窗口隐藏时调用
    public override void OnHide()
    {
        base.OnHide();
    }

    // 窗口销毁时调用
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
```

### 第三步：弹出窗口

```csharp
// 在任意脚本中调用
UIModule.Instance.PopUpWindow<MainMenuWindow>();
```

### 第四步：配置 WindowConfig

在 `Resources` 文件夹下创建 `WindowConfig` ScriptableObject，并配置窗口预制体路径。  
*(在 Editor 模式下，`Initialize()` 会自动调用 `GeneratorWindowConfig()` 生成配置)*

---

## 📁 目录结构

```
Assets/
└── ZMPackages/
    ├── ZMUI/
    │   ├── Runtime/
    │   │   ├── Core/           # 框架核心（UIModule、WindowBase 等）
    │   │   ├── Base/           # 基础类
    │   │   ├── Agent/          # 代理系统
    │   │   ├── Adaptation/     # 刘海屏适配
    │   │   ├── Event/          # 事件系统
    │   │   └── SupperView/     # 超级视图
    │   ├── Editor/             # Editor 工具（自动化代码生成等）
    │   ├── AOT/                # HybridCLR 热更新支持
    │   ├── Example/            # 示例场景
    │   ├── Resources/          # 框架资源
    │   └── ThirdLibrary/       # 第三方库
    └── Library/
        ├── Newtonsoft.Json/    # JSON 序列化库
        └── Sirenix/            # Odin Inspector
```

---

## 📝 更新日志

### 2025.3.17

**Editor 功能新增**
- 新增 Item 脚本递增式自动生成，字段方法递归插入
- 新增节点过滤功能
- 新增标记 `#`，用于过滤节点解析
- 新增组件数组自动生成和赋值
- 新增标记 `,`，用于数组节点解析
- 菜单栏 `ZMFrame → ZMUI Setting` 增加自定义引入命名空间配置

**Editor 功能优化**
- 优化自定义选择路径生成器，修复路径展示错误问题
- 优化部分 Editor 脚本名称

---

## 📚 文档与资源

| 资源 | 链接 |
|------|------|
| 📖 API 文档 | [www.zm-doc.com](http://www.zm-doc.com) |
| 📝 博客教程 | [CSDN 详细教程](https://blog.csdn.net/qq_42461824/article/details/116570321) |
| 💬 QQ 联系 | 975659933 |
| 📧 邮箱 | zhumengxyedu@163.com |

---

## 🤝 贡献与反馈

欢迎提交 [Issue](https://github.com/ZMteacher/ZMUIFrameWork/issues) 或 [Pull Request](https://github.com/ZMteacher/ZMUIFrameWork/pulls)！

如果本框架对你有帮助，请给个 ⭐ Star 支持一下！

---

<div align="center">

**由 铸梦xy 精心打造 · 专为 Unity 游戏开发者设计**

</div>