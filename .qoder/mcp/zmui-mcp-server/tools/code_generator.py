"""ZMUI 代码生成工具
根据窗口/Item 描述生成 WindowBase 子类、DataComponent、Item 脚本。
模板逻辑对齐 Editor/Generator/ 下的 C# 生成器。
"""

import os
import re
from datetime import datetime

# 组件类型 → 事件绑定模式（{field} 为完整字段名，{name} 为基础名）
EVENT_BINDINGS = {
    "Button": "target.AddButtonClickListener({field}, mWindow.On{name}ButtonClick);",
    "Toggle": "target.AddToggleClickListener({field}, mWindow.On{name}ToggleChange);",
    "InputField": "target.AddInputFieldListener({field}, mWindow.On{name}InputChange, mWindow.On{name}InputEnd);",
    "TMP_InputField": "target.AddTMPInputFieldListener({field}, mWindow.On{name}InputChange, mWindow.On{name}InputEnd);",
}

# 组件类型 → 回调方法名
EVENT_METHODS = {
    "Button": [("On{name}ButtonClick", "")],
    "Toggle": [("On{name}ToggleChange", "bool state,Toggle toggle")],
    "InputField": [("On{name}InputChange", "string text"), ("On{name}InputEnd", "string text")],
    "TMP_InputField": [("On{name}InputChange", "string text"), ("On{name}InputEnd", "string text")],
}


def _sanitize_name(name: str) -> str:
    """清洗类名/字段名，确保是合法 C# 标识符"""
    name = re.sub(r"[^A-Za-z0-9_]", "", name)
    if not name:
        raise ValueError("名称不能为空")
    if name[0].isdigit():
        name = "_" + name
    return name


def _parse_components(components: list | None) -> list[dict]:
    """规范化组件描述

    支持的输入格式:
      - [{"name": "CloseButton", "type": "Button"}]
      - [{"fieldName": "Close", "fieldType": "Button"}]
      - ["CloseButton:Button", "TitleText:Text"]
      - "CloseButton:Button,TitleText:Text"
      - "[Button]Close,[Text]TitleText"

    命名规则（与框架 AnalysisComponentDataTool 一致）：
      - 字段名 = fieldName + fieldType，如 fieldName="Close" + "Button" → CloseButton
      - 若传入的 name 已以类型结尾（如 CloseButton），自动提取 fieldName="Close"
      - 数组字段以 Array 后缀标识，如 TopRightCurrencyItemArray

    Returns:
        [{"name": 基础名 fieldName, "type": 组件类型, "fieldName": 完整字段名,
          "isArray": 是否数组}]
    """
    if not components:
        return []
    if isinstance(components, str):
        items = [c.strip() for c in components.split(",") if c.strip()]
    else:
        items = list(components)

    parsed = []
    for item in items:
        if isinstance(item, dict):
            name = item.get("name") or item.get("fieldName") or ""
            ctype = item.get("type") or item.get("fieldType") or ""
        elif isinstance(item, str):
            if ":" in item:
                name, ctype = item.split(":", 1)
            else:
                # 尝试 [Button]CloseButton 格式
                m = re.match(r"^\[(\w+)\](.+)$", item.strip())
                if m:
                    ctype, name = m.group(1), m.group(2)
                else:
                    name, ctype = item, ""
        else:
            continue

        name = name.strip()
        ctype = ctype.strip()
        if not name or not ctype:
            continue

        is_array = name.endswith("Array") or ctype.endswith("[]")
        if is_array:
            ctype = ctype.rstrip("[]")
            name = name[:-5] if name.endswith("Array") else name

        # 若 name 以类型名结尾，视为完整字段名，提取基础名
        # 如 name="CloseButton" type="Button" → field_name="CloseButton", base="Close"
        field_name = name + ctype if not name.endswith(ctype) else name
        base = field_name[: -len(ctype)] if field_name.endswith(ctype) else field_name

        parsed.append({
            "name": _sanitize_name(base),
            "type": ctype,
            "fieldName": _sanitize_name(field_name),
            "isArray": is_array,
        })
    return parsed


def generate_window_script(
    window_name: str,
    components: list | None = None,
    full_screen: bool = False,
    enable_update: bool = False,
    disable_anim: bool = False,
    namespace_name: str = "ZM.UI",
) -> str:
    """生成 WindowBase 子类脚本

    Args:
        window_name: 窗口类名（须与预制体名一致）
        components: 组件列表 [{"name", "type"}, ...]
        full_screen: 是否标记为全屏窗口
        enable_update: 是否开启 OnUpdate
        disable_anim: 是否禁用弹出动画
        namespace_name: 命名空间（空字符串表示不使用命名空间）

    Returns:
        生成的 .cs 文件内容
    """
    name = _sanitize_name(window_name)
    comps = _parse_components(components)
    now = datetime.now().strftime("%Y/%m/%d %H:%M:%S")

    lines = []
    lines.append("/*---------------------------------")
    lines.append(" *Title:UI表现层脚本自动化生成工具")
    lines.append(" *Author:ZM 铸梦")
    lines.append(f" *Date:{now}")
    lines.append(" *Description:UI 表现层，该层只负责界面的交互、表现相关的更新，不允许编写任何业务逻辑代码")
    lines.append(" *注意:以下文件是自动生成的，再次生成不会覆盖原有的代码，会在原有的代码上进行新增，可放心使用")
    lines.append("---------------------------------*/")
    lines.append("using UnityEngine.UI;")
    lines.append("using UnityEngine;")
    lines.append("")

    ns_indent = ""
    if namespace_name:
        lines.append(f"namespace {namespace_name}")
        lines.append("{")
        ns_indent = "    "

    lines.append(f"{ns_indent}public class {name} : WindowBase")
    lines.append(f"{ns_indent}{{")
    lines.append(f"{ns_indent}    public {name}DataComponent dataCompt;")
    lines.append("")

    # 生命周期函数
    lines.append(f"{ns_indent}    #region 生命周期函数")
    lines.append("")
    lines.append(f"{ns_indent}    //调用机制与Mono Awake一致")
    lines.append(f"{ns_indent}    public override void OnAwake()")
    lines.append(f"{ns_indent}    {{")
    lines.append(f"{ns_indent}        dataCompt = gameObject.GetComponent<{name}DataComponent>();")
    lines.append(f"{ns_indent}        dataCompt.InitComponent(this);")
    lines.append("")
    if enable_update:
        lines.append(f"{ns_indent}        //开启Update渲染帧更新")
    else:
        lines.append(f"{ns_indent}        //开启Update渲染帧更新，默认不开，节省性能")
    lines.append(f"{ns_indent}        // Update = true;")
    lines.append("")
    if full_screen:
        lines.append(f"{ns_indent}        //标记为全屏窗口，智能显隐会监测当全屏弹窗弹出时，被遮挡的窗口都会通过伪隐藏隐藏掉，从而提升性能")
        lines.append(f"{ns_indent}        FullScreenWindow = true;")
        lines.append("")
    if disable_anim:
        lines.append(f"{ns_indent}        //禁用窗口弹出动画，可在基类中自定义动画形态")
        lines.append(f"{ns_indent}        mDisableAnim = true;")
        lines.append("")
    lines.append(f"{ns_indent}        base.OnAwake();")
    lines.append(f"{ns_indent}    }}")
    lines.append("")
    lines.append(f"{ns_indent}    //物体显示时执行")
    lines.append(f"{ns_indent}    public override void OnShow()")
    lines.append(f"{ns_indent}    {{")
    lines.append(f"{ns_indent}        base.OnShow();")
    lines.append(f"{ns_indent}    }}")
    lines.append("")
    lines.append(f"{ns_indent}    //物体隐藏时执行")
    lines.append(f"{ns_indent}    public override void OnHide()")
    lines.append(f"{ns_indent}    {{")
    lines.append(f"{ns_indent}        base.OnHide();")
    lines.append(f"{ns_indent}    }}")
    lines.append("")
    lines.append(f"{ns_indent}    //物体销毁时执行")
    lines.append(f"{ns_indent}    public override void OnDestroy()")
    lines.append(f"{ns_indent}    {{")
    lines.append(f"{ns_indent}        base.OnDestroy();")
    lines.append(f"{ns_indent}    }}")
    lines.append("")
    lines.append(f"{ns_indent}    #endregion")
    lines.append("")

    # API Function 区域
    lines.append(f"{ns_indent}    #region API Function")
    lines.append(f"{ns_indent}    ")
    lines.append(f"{ns_indent}    #endregion")
    lines.append("")

    # UI 组件事件区域
    lines.append(f"{ns_indent}    #region UI组件事件")
    lines.append("")
    for comp in comps:
        base = comp["name"]
        ctype = comp["type"]
        if "Button" in ctype:
            method = f"On{base}ButtonClick"
            lines.append(f"{ns_indent}    public void {method}()")
            lines.append(f"{ns_indent}    {{")
            if base.lower().startswith("close"):
                lines.append(f"{ns_indent}        HideWindow();")
            lines.append(f"{ns_indent}    }}")
            lines.append("")
        elif "Toggle" in ctype:
            method = f"On{base}ToggleChange"
            lines.append(f"{ns_indent}    public void {method}(bool state, Toggle toggle)")
            lines.append(f"{ns_indent}    {{")
            lines.append(f"{ns_indent}    }}")
            lines.append("")
        elif "InputField" in ctype:
            for suffix, params in [("InputChange", "string text"), ("InputEnd", "string text")]:
                method = f"On{base}{suffix}"
                lines.append(f"{ns_indent}    public void {method}({params})")
                lines.append(f"{ns_indent}    {{")
                lines.append(f"{ns_indent}    }}")
                lines.append("")

    lines.append(f"{ns_indent}    #endregion")
    lines.append(f"{ns_indent}}}")
    if namespace_name:
        lines.append("}")

    return "\n".join(lines)


def generate_data_component(
    window_name: str,
    components: list | None = None,
    namespace_name: str = "ZM.UI",
) -> str:
    """生成 DataComponent 组件绑定脚本

    Args:
        window_name: 窗口类名
        components: 组件列表 [{"name", "type"}, ...]
        namespace_name: 命名空间（空字符串表示不使用命名空间）

    Returns:
        生成的 DataComponent .cs 文件内容
    """
    name = _sanitize_name(window_name)
    comps = _parse_components(components)
    now = datetime.now().strftime("%Y/%m/%d %H:%M:%S")

    lines = []
    lines.append("/*---------------------------------")
    lines.append(" *Title:UI自动化组件生成代码生成工具")
    lines.append(" *Author:铸梦")
    lines.append(f" *Date:{now}")
    lines.append(" *Description:变量需要以[Text]括号加组件类型的格式进行声明，然后右键窗口物体—— 一键生成UI数据组件脚本即可")
    lines.append(" *注意:以下文件是自动生成的，任何手动修改都会被下次生成覆盖,若手动修改后,尽量避免自动生成")
    lines.append("---------------------------------*/")
    lines.append("using UnityEngine;")
    lines.append("using UnityEngine.UI;")
    lines.append("using SuperScrollView;")
    lines.append("")

    ns_indent = ""
    if namespace_name:
        lines.append(f"namespace {namespace_name}")
        lines.append("{")
        ns_indent = "    "

    lines.append(f"{ns_indent}public class {name}DataComponent : MonoBehaviour")
    lines.append(f"{ns_indent}{{")
    lines.append("")

    # 字段声明
    for comp in comps:
        field_name = comp["fieldName"]
        ctype = comp["type"]
        if comp["isArray"]:
            lines.append(f"{ns_indent}    public {ctype}[] {field_name}Array;")
        else:
            lines.append(f"{ns_indent}    public {ctype} {field_name};")
        lines.append("")

    # InitComponent
    lines.append(f"{ns_indent}    public void InitComponent(WindowBase target)")
    lines.append(f"{ns_indent}    {{")
    lines.append(f"{ns_indent}        // 组件事件绑定")
    lines.append(f"{ns_indent}        {name} mWindow = ({name})target;")
    lines.append("")
    for comp in comps:
        field_name = comp["fieldName"]
        base = comp["name"]
        ctype = comp["type"]
        if comp["isArray"]:
            continue  # 数组字段不生成事件绑定
        binding = EVENT_BINDINGS.get(ctype)
        if binding:
            code = binding.format(field=field_name, type=ctype, name=base)
            lines.append(f"{ns_indent}        {code}")
    lines.append(f"{ns_indent}    }}")
    lines.append(f"{ns_indent}}}")
    if namespace_name:
        lines.append("}")

    return "\n".join(lines)


def generate_item_script(
    item_name: str,
    components: list | None = None,
    data_comment: str = "// 在此处添加数据字段",
) -> str:
    """生成 IZMUIViewListItem 实现类（列表项脚本）

    Args:
        item_name: Item 类名
        components: 组件列表 [{"name", "type"}, ...]
        data_comment: 数据字段注释说明

    Returns:
        生成的 Item .cs 文件内容
    """
    name = _sanitize_name(item_name)
    comps = _parse_components(components)
    now = datetime.now().strftime("%Y/%m/%d %H:%M:%S")

    lines = []
    lines.append("/*---------------------------------")
    lines.append(" *Title:UI自动化组件生成代码生成工具")
    lines.append(" *Author:ZM 铸梦")
    lines.append(f" *Date:{now}")
    lines.append(" *Description:列表 Item 脚本，实现 IZMUIViewListItem 接口")
    lines.append("---------------------------------*/")
    lines.append("using UnityEngine;")
    lines.append("using UnityEngine.UI;")
    lines.append("using ZM.UI;")
    lines.append("")

    lines.append(f"public class {name} : MonoBehaviour, IZMUIViewListItem")
    lines.append("{")
    lines.append("")
    lines.append("    // 组件字段声明")
    for comp in comps:
        field_name = comp["fieldName"]
        ctype = comp["type"]
        if comp["isArray"]:
            lines.append(f"    public {ctype}[] {field_name}Array;")
        else:
            lines.append(f"    public {ctype} {field_name};")
    lines.append("")
    lines.append("    // 数据字段")
    lines.append(f"    {data_comment}")
    lines.append("")

    lines.append("    /// <summary>")
    lines.append("    /// Item 初始化（仅执行一次）")
    lines.append("    /// </summary>")
    lines.append("    public void InitListItem()")
    lines.append("    {")
    lines.append("        // 在这里进行组件获取或一次性初始化")
    lines.append("    }")
    lines.append("")

    lines.append("    /// <summary>")
    lines.append("    /// 设置 Item 显示数据")
    lines.append("    /// </summary>")
    lines.append("    /// <param name=\"index\">数据索引</param>")
    lines.append("    /// <param name=\"data\">数据参数（引用类型，无装箱拆箱）</param>")
    lines.append("    public void SetListItemShowData(int index, params object[] data)")
    lines.append("    {")
    lines.append("        if (data == null || data.Length == 0) return;")
    lines.append("")
    lines.append("        // 从 data 参数中提取数据并更新 UI")
    lines.append("    }")
    lines.append("")

    lines.append("    /// <summary>")
    lines.append("    /// 资源释放")
    lines.append("    /// </summary>")
    lines.append("    public void OnRelease()")
    lines.append("    {")
    lines.append("        // 清理资源，取消事件注册等")
    lines.append("    }")
    lines.append("}")
    lines.append("")

    return "\n".join(lines)


def save_script(content: str, rel_path: str) -> dict:
    """将脚本内容保存到项目

    Args:
        content: 脚本内容
        rel_path: 相对项目根目录的路径，如 "Assets/Scripts/MyWindow.cs"

    Returns:
        {"success": bool, "path": 绝对路径, "error": 错误信息}
    """
    from .project_reader import PROJECT_ROOT

    full = PROJECT_ROOT / rel_path.replace("/", os.sep).replace("\\", os.sep)
    try:
        full.parent.mkdir(parents=True, exist_ok=True)
        full.write_text(content, encoding="utf-8")
        return {"success": True, "path": str(full), "error": ""}
    except Exception as e:
        return {"success": False, "path": str(full), "error": str(e)}
