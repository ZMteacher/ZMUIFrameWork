"""ZMUI 窗口配置查询工具
解析 WindowConfig.asset 和 UISetting.asset (Unity YAML 格式)，提供窗口信息查询。
"""

import os
import re
from pathlib import Path

from .project_reader import PROJECT_ROOT, normalize_path

# 默认配置路径
WINDOW_CONFIG_DEFAULT = "Assets/ZMPackages/ZMUI/Resources/WindowConfig.asset"
UI_SETTING_DEFAULT = "Assets/ZMPackages/ZMUI/Resources/UISetting.asset"

# Unity YAML 中 null / 空值标记
_NULL_VALUES = {"null", "~", ""}


def _strip_unity_yaml(yaml_text: str) -> str:
    """清理 Unity YAML：保留 %TAG 指令（多构造函数依赖它解析 !u! 句柄），
    移除 %YAML 版本声明避免兼容问题"""
    lines = yaml_text.splitlines()
    cleaned = []
    for line in lines:
        if line.startswith("%YAML"):
            continue
        cleaned.append(line)
    return "\n".join(cleaned)


def _unity_multi_constructor(loader, tag_suffix, node):
    """处理 Unity 自定义 tag（如 !u!114），直接返回对应节点结构"""
    import yaml
    if isinstance(node, yaml.MappingNode):
        return loader.construct_mapping(node)
    if isinstance(node, yaml.SequenceNode):
        return loader.construct_sequence(node)
    return loader.construct_scalar(node)


_register_yaml = False


def _register_unity_yaml_constructor():
    """注册 Unity YAML 多构造函数（全局只注册一次）"""
    global _register_yaml
    if _register_yaml:
        return
    import yaml
    yaml.add_multi_constructor("tag:unity3d.com,2011:", _unity_multi_constructor, Loader=yaml.SafeLoader)
    _register_yaml = True


def _regex_parse_asset(content: str) -> dict:
    """正则回退解析：提取非 Unity 内部字段的 key: value 与列表项"""
    data = {}
    lines = content.splitlines()
    for line in lines:
        stripped = line.strip()
        if not stripped or stripped.startswith("---") or stripped.startswith("%"):
            continue
        if stripped == "MonoBehaviour:" or stripped.startswith("m_"):
            continue

        # 列表项: - key: value
        list_item = re.match(r"^- (\w+):\s*(.*)$", stripped)
        if list_item:
            key, value = list_item.group(1), list_item.group(2).strip()
            if key not in data or not isinstance(data.get(key), list):
                data[key] = []
            if value and value not in _NULL_VALUES:
                data[key].append(value)
            continue

        # 简单字段: key: value 或 key:（空，可能是列表/对象头部）
        simple = re.match(r"^(\w+):\s*(.*)$", stripped)
        if simple:
            key, value = simple.group(1), simple.group(2).strip()
            if value.startswith("[") and value.endswith("]"):
                items = [v.strip().strip('"').strip("'") for v in value[1:-1].split(",")]
                data[key] = [v for v in items if v]
            elif value not in _NULL_VALUES:
                data[key] = value
    return data


def _parse_unity_asset(rel_path: str) -> dict:
    """解析 Unity ScriptableObject asset 文件

    优先使用 PyYAML，失败时回退到正则提取。

    Returns:
        {"exists": bool, "path": 绝对路径, "data": 解析后的字段 dict, "error": 错误信息}
    """
    result = {"exists": False, "path": "", "data": {}, "error": ""}
    full = PROJECT_ROOT / rel_path.replace("/", os.sep).replace("\\", os.sep)
    result["path"] = str(full)
    if not full.exists():
        result["error"] = f"文件不存在: {rel_path}"
        return result

    try:
        content = full.read_text(encoding="utf-8", errors="ignore")
    except Exception as e:
        result["error"] = f"读取失败: {e}"
        return result

    # 尝试 PyYAML 解析
    try:
        import yaml
        _register_unity_yaml_constructor()
        yaml_text = _strip_unity_yaml(content)
        parsed = yaml.safe_load(yaml_text)
        if isinstance(parsed, dict):
            # 找到第一个 MonoBehaviour 节点
            for key, value in parsed.items():
                if isinstance(value, dict):
                    result["data"] = value
                    break
            else:
                result["data"] = parsed
            result["exists"] = True
            return result
    except ImportError:
        pass  # PyYAML 未安装，回退到正则
    except Exception:
        pass  # 解析失败，回退到正则

    # 回退：正则提取字段
    result["exists"] = True
    result["data"] = _regex_parse_asset(content)
    return result


def query_window_config(filter_name: str | None = None) -> dict:
    """查询 WindowConfig 配置

    Args:
        filter_name: 可选窗口名过滤（模糊匹配）

    Returns:
        {"config_path": 配置文件路径, "windows": [{"name", "path"}], "total": 数量}
    """
    result = _parse_unity_asset(WINDOW_CONFIG_DEFAULT)
    if not result["exists"]:
        return {"error": result["error"], "config_path": result["path"], "windows": [], "total": 0}

    windows = []
    raw = result["data"]
    # PyYAML 解析出的 windowDataList 是 list[dict] 形式
    data_list = raw.get("windowDataList", raw.get("windows", []))
    if isinstance(data_list, dict):  # 正则回退时可能是 dict
        names = data_list.get("name", [])
        paths = data_list.get("path", [])
        data_list = [
            {"name": n, "path": p}
            for n, p in zip(names, paths)
        ]

    for item in data_list:
        if not isinstance(item, dict):
            continue
        name = str(item.get("name", "")).strip()
        path = normalize_path(str(item.get("path", "")))
        if not name:
            continue
        if filter_name and filter_name not in name:
            continue
        windows.append({"name": name, "path": path})

    return {
        "config_path": result["path"],
        "windows": windows,
        "total": len(windows),
    }


def query_ui_setting() -> dict:
    """查询 UISetting 配置

    Returns:
        {"config_path": 配置文件路径, "setting": {配置字段}}
    """
    result = _parse_unity_asset(UI_SETTING_DEFAULT)
    if not result["exists"]:
        return {"error": result["error"], "config_path": result["path"], "setting": {}}

    raw = result["data"]

    # 枚举字段映射
    parse_types = {0: "Name", 1: "Tag"}
    generator_types = {0: "Find", 1: "Bind"}

    # 解析数组字段（PyYAML 时是 list，正则回退时是 list[str]）
    def _to_list(value):
        if value is None:
            return []
        if isinstance(value, list):
            return value
        if isinstance(value, str):
            return [v.strip() for v in value.split(",") if v.strip()]
        return []

    setting = {
        "SINGMASK_SYSTEM": bool(raw.get("SINGMASK_SYSTEM", False)),
        "ParseType": parse_types.get(raw.get("ParseType"), str(raw.get("ParseType", ""))),
        "GeneratorType": generator_types.get(raw.get("GeneratorType"), str(raw.get("GeneratorType", ""))),
        "BindComponentGeneratorPath": str(raw.get("BindComponentGeneratorPath", "")),
        "FindComponentGeneratorPath": str(raw.get("FindComponentGeneratorPath", "")),
        "WindowGeneratorPath": str(raw.get("WindowGeneratorPath", "")),
        "ItemScriptsGeneratorPath": str(raw.get("ItemScriptsGeneratorPath", "")),
        "WindowPrefabFolderPathArr": _to_list(raw.get("WindowPrefabFolderPathArr")),
        "UsingNameSpaceArr": _to_list(raw.get("UsingNameSpaceArr")),
    }

    # 组件映射表
    mappings = raw.get("ComponentMappings", [])
    component_mappings = []
    if isinstance(mappings, list):
        for m in mappings:
            if isinstance(m, dict):
                component_mappings.append({
                    "Key": str(m.get("Key", "")),
                    "ComponentType": str(m.get("ComponentType", "")),
                })
    elif isinstance(mappings, dict):
        keys = mappings.get("Key", [])
        types = mappings.get("ComponentType", [])
        component_mappings = [
            {"Key": str(k), "ComponentType": str(t)}
            for k, t in zip(keys, types)
        ]
    setting["ComponentMappings"] = component_mappings

    return {"config_path": result["path"], "setting": setting}


def query_windows_in_folders() -> dict:
    """扫描 UISetting 中配置的预制体目录，列出所有窗口预制体

    Returns:
        {"folders": 扫描的目录列表, "windows": [{"name", "path"}], "total": 数量}
    """
    setting_result = query_ui_setting()
    if "error" in setting_result:
        return {"error": setting_result["error"], "folders": [], "windows": [], "total": 0}

    folders = setting_result["setting"].get("WindowPrefabFolderPathArr", [])
    from .project_reader import find_prefabs

    windows = []
    seen = set()
    for folder in folders:
        if not folder:
            continue
        for prefab in find_prefabs(folder):
            if prefab["name"] in seen:
                continue
            seen.add(prefab["name"])
            windows.append(prefab)

    return {"folders": folders, "windows": windows, "total": len(windows)}
