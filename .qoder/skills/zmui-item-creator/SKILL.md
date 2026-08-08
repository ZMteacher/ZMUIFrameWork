# ZMUI 列表项创建技能

## 描述
根据用户描述的列表项 UI 需求，自动生成实现 IZMUIViewListItem 接口的 Item 脚本，用于 ZMUIListView 和 ZMUIIGridView 的列表/网格显示。

## 适用场景
- 用户说"创建一个 XX 列表项，显示 XX 和 XX 数据"
- 用户说"为排行榜创建一个 Item 脚本"
- 用户说"生成一个商品列表项，包含图标、名称、价格"
- 用户说"给我创建货币Item，有图标和数量"

## 框架架构参考

### IZMUIViewListItem 接口
```csharp
namespace ZM.UI
{
    public interface IZMUIViewListItem
    {
        /// <summary> 初始化列表Item </summary>
        public void InitListItem();

        /// <summary> 设置Item显示数据 </summary>
        public void SetListItemShowData(int index, params object[] data);

        /// <summary> 脚本资源释放接口 </summary>
        public void OnRelease();
    }
}
```

### 列表/网格视图使用方式
```csharp
// ZMUIListView — 纵向滚动列表
public class ZMUIListView : MonoBehaviour
{
    public LoopListView2 loopListView;

    // 刷新列表
    public void RefreshListView(bool reSetPos, int viewDataCount, GetItemDataDelegate getItemDataCallBack);
}

// ZMUIIGridView — 网格布局
public class ZMUIIGridView : MonoBehaviour
{
    public LoopGridView loopListView;

    // 刷新网格
    public void RefreshListView(bool reSetPos, int viewDataCount, GetItemDataDelegate getItemDataCallBack);
}

// 数据委托
public delegate object GetItemDataDelegate(int index);
```

## 代码生成模板

### Item 脚本模板
```csharp
/*---------------------------------
 *Title:UI自动化组件生成代码生成工具
 *Author:ZM 铸梦
 *Date:{DateTime.Now}
 *Description:列表 Item 脚本，实现 IZMUIViewListItem 接口
 ---------------------------------*/
using UnityEngine;
using UnityEngine.UI;
using ZM.UI;

public class {ItemName} : MonoBehaviour, IZMUIViewListItem
{
    // 组件字段声明
    // public Image IconImage;
    // public Text NameText;
    // public Text ValueText;

    // 数据字段
    // private {DataType} mData;

    /// <summary>
    /// Item 初始化（仅执行一次）
    /// </summary>
    public void InitListItem()
    {
        // 在这里进行组件获取或初始化
        // 例如：获取子物体上的组件引用
        // IconImage = transform.Find("Icon").GetComponent<Image>();
    }

    /// <summary>
    /// 设置 Item 显示数据
    /// </summary>
    /// <param name="index">数据索引</param>
    /// <param name="data">数据参数（引用类型，无装箱拆箱）</param>
    public void SetListItemShowData(int index, params object[] data)
    {
        if (data == null || data.Length == 0) return;

        // 从 data 参数中提取数据并更新 UI
        // 例如：
        // mData = data[0] as {DataType};
        // if (mData == null) return;
        // NameText.text = mData.name;
        // ValueText.text = mData.value.ToString();
    }

    /// <summary>
    /// 资源释放
    /// </summary>
    public void OnRelease()
    {
        // 清理资源，取消事件注册等
    }
}
```

## 完整案例参考

### CurrencyItem（货币 Item）
```csharp
public class CurrencyItem : MonoBehaviour, IZMUIViewListItem
{
    public Image IconImage;
    public Text ValueText;

    public void InitListItem() { }

    public void SetListItemShowData(int index, params object[] data)
    {
        if (data == null || data.Length == 0) return;
        // 使用数据更新 UI
    }

    public void OnRelease() { }
}
```

### RankListItem（排行榜 Item）
```csharp
public class RankListItem : MonoBehaviour, IZMUIViewListItem
{
    public Text RankText;
    public Text NameText;
    public Text ScoreText;
    public RawImage HeadImage;
    public GameObject SelectObj;

    public void InitListItem() { }

    public void SetListItemShowData(int index, params object[] data)
    {
        // 从 data 中提取数据
        // 更新 UI 显示
    }

    public void OnRelease() { }
}
```

### RoleItem（角色 Item）
```csharp
public class RoleItem : MonoBehaviour, IZMUIViewListItem
{
    public Image IconImage;
    public Text NameText;
    public Text DesText;
    public Button ClickButton;

    public void InitListItem() { }

    public void SetListItemShowData(int index, params object[] data)
    {
        // 从 data 中提取数据
        // 更新 UI 显示
    }

    public void OnRelease() { }
}
```

### TaskItem（任务 Item）
```csharp
public class TaskItem : MonoBehaviour, IZMUIViewListItem
{
    public Text TaskNameText;
    public Text TaskDesText;
    public Slider ProgressSlider;
    public Text ProgressText;
    public Button RewardButton;
    public GameObject FinishObj;

    public void InitListItem() { }

    public void SetListItemShowData(int index, params object[] data)
    {
        // 从 data 中提取数据
        // 更新 UI 显示
    }

    public void OnRelease() { }
}
```

## 使用流程

1. 创建预制体：在窗口预制体的 UIContent 下放置 Item 预制体
2. 实现 Item 脚本：创建类实现 IZMUIViewListItem 接口
3. 挂载脚本：将 Item 脚本挂载到 Item 预制体根节点
4. 配置列表：在 ZMUIListView 或 ZMUIIGridView 中配置 ItemPrefabDataList
5. 刷新数据：调用 RefreshListView 传入数据数量和回调委托

```csharp
// 在窗口中使用示例
public override void OnShow()
{
    base.OnShow();
    dataCompt.RankListView.RefreshListView(true, rankDataList.Count, (index) =>
    {
        return rankDataList[index];
    });
}
```

## 参考文件
- `Assets/ZMPackages/ZMUI/Example/Scripts/AutoGenerate/Item/CurrencyItem.cs`
- `Assets/ZMPackages/ZMUI/Example/Scripts/AutoGenerate/Item/RankListItem.cs`
- `Assets/ZMPackages/ZMUI/Example/Scripts/AutoGenerate/Item/RoleItem.cs`
- `Assets/ZMPackages/ZMUI/Example/Scripts/AutoGenerate/Item/TaskItem.cs`
- `Assets/ZMPackages/ZMUI/Runtime/SupperView/IZMUIViewListItem.cs`
- `Assets/ZMPackages/ZMUI/Runtime/SupperView/ZMUIListView.cs`
- `Assets/ZMPackages/ZMUI/Runtime/SupperView/ZMUIIGridView.cs`

## 注意事项
1. Item 类不需要命名空间，框架自动识别
2. SetListItemShowData 的 data 参数是引用类型，不存在装箱拆箱开销
3. 在 InitListItem 中做一次性初始化，避免每次 SetData 时重复查找组件
4. OnRelease 在列表销毁或刷新时调用，用于清理资源
5. Item 预制体应放在窗口预制体目录下，或通过 Resources 加载