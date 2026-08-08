"""ZMUI 项目结构读取工具
读取 Unity 项目的目录结构、窗口预制体信息等。
"""

import os
import re
from pathlib import Path

def _find_project_root() -> Path:
    """向上查找 Unity 项目根目录（含 Assets 的目录）"""
    current = Path(__file__).resolve().parent
    for _ in range(8):
        if (current / "Assets").is_dir():
            return current
        parent = current.parent
        if parent == current:
            break
        current = parent
    return Path(__file__).resolve().parents[3]


# 项目根目录（向上查找到含 Assets 的目录）
PROJECT_ROOT = _find_project_root()

# 需要跳过的目录
SKIP_DIRS = {
    ".git", ".vs", ".idea", "Library", "Logs", "obj", "Temp",
    "Packages", "ProjectSettings", "UserSettings", ".qoder", ".gradle",
}


def get_project_root() -> Path:
    return PROJECT_ROOT


def normalize_path(path: str) -> str:
    return path.replace("\\", "/").strip()


def is_unity_asset(path: Path) -> bool:
    """判断是否为 Unity 资源（排除 .meta 文件）"""
    return path.suffix not in {".meta", ".dll", ".csproj", ".sln", ".asmdef"}


def list_directory_tree(root: str | None = None, depth: int = 2) -> dict:
    """查询项目目录结构树

    Args:
        root: 相对项目根目录的子路径，如 "Assets/ZMPackages/ZMUI"
        depth: 目录深度限制

    Returns:
        {"root": 绝对根路径, "children": [目录/文件列表]}
    """
    base = PROJECT_ROOT
    if root:
        base = base / root.replace("/", os.sep).replace("\\", os.sep)
    if not base.exists():
        return {"error": f"路径不存在: {root}", "root": str(PROJECT_ROOT), "children": []}

    result = {"root": str(base), "children": []}

    def walk(path: Path, current_depth: int):
        if current_depth > depth:
            return
        try:
            entries = sorted(path.iterdir(), key=lambda p: (not p.is_dir(), p.name.lower()))
        except PermissionError:
            return
        for entry in entries:
            if entry.is_dir():
                if entry.name in SKIP_DIRS:
                    continue
                node = {"name": entry.name, "type": "dir", "children": []}
                walk(entry, current_depth + 1)
                result["children"].append(node) if path == base else None
                if path == base:
                    # 只展开根节点下一层的子目录结构
                    sub = {"name": entry.name, "type": "dir", "children": []}
                    _collect_children(entry, sub, 1)
                    result["children"][-1] = sub
            else:
                if is_unity_asset(entry):
                    result["children"].append({"name": entry.name, "type": "file"})

    def _collect_children(path: Path, node: dict, current_depth: int):
        if current_depth >= depth:
            return
        try:
            entries = sorted(path.iterdir(), key=lambda p: (not p.is_dir(), p.name.lower()))
        except PermissionError:
            return
        for entry in entries:
            if entry.is_dir():
                if entry.name in SKIP_DIRS:
                    continue
                sub = {"name": entry.name, "type": "dir", "children": []}
                _collect_children(entry, sub, current_depth + 1)
                node["children"].append(sub)
            else:
                if is_unity_asset(entry):
                    node["children"].append({"name": entry.name, "type": "file"})

    walk(base, 0)
    return result


def find_prefabs(path: str | None = None) -> list[dict]:
    """查找项目中的窗口预制体

    Args:
        path: 相对项目根目录的搜索路径，为空则搜索全部 Assets 目录

    Returns:
        [{"name": 预制体名, "path": 相对项目根的路径}, ...]
    """
    search_root = PROJECT_ROOT / "Assets"
    if path:
        search_root = PROJECT_ROOT / path.replace("/", os.sep).replace("\\", os.sep)
    if not search_root.exists():
        return []

    prefabs = []
    for root, dirs, files in os.walk(search_root):
        dirs[:] = [d for d in dirs if d not in SKIP_DIRS]
        for file in files:
            if file.endswith(".prefab"):
                full = Path(root) / file
                rel = full.relative_to(PROJECT_ROOT).as_posix()
                prefabs.append({"name": file[:-7], "path": rel})
    prefabs.sort(key=lambda x: x["name"].lower())
    return prefabs


def find_cs_files(path: str | None = None) -> list[dict]:
    """查找项目中的 C# 脚本文件

    Args:
        path: 相对项目根目录的搜索路径，为空则搜索全部 Assets 目录

    Returns:
        [{"name": 脚本名, "path": 相对项目根的路径}, ...]
    """
    search_root = PROJECT_ROOT / "Assets"
    if path:
        search_root = PROJECT_ROOT / path.replace("/", os.sep).replace("\\", os.sep)
    if not search_root.exists():
        return []

    files = []
    for root, dirs, file_list in os.walk(search_root):
        dirs[:] = [d for d in dirs if d not in SKIP_DIRS]
        for file in file_list:
            if file.endswith(".cs"):
                full = Path(root) / file
                rel = full.relative_to(PROJECT_ROOT).as_posix()
                files.append({"name": file[:-3], "path": rel})
    files.sort(key=lambda x: x["name"].lower())
    return files


def read_file(rel_path: str) -> dict:
    """读取文本文件内容

    Args:
        rel_path: 相对项目根目录的路径，如 "Assets/ZMPackages/ZMUI/Runtime/Core/UIModule.cs"

    Returns:
        {"path": 绝对路径, "exists": 是否存在, "content": 文件内容}
    """
    full = PROJECT_ROOT / rel_path.replace("/", os.sep).replace("\\", os.sep)
    if not full.exists():
        return {"path": str(full), "exists": False, "content": ""}
    try:
        content = full.read_text(encoding="utf-8", errors="ignore")
        return {"path": str(full), "exists": True, "content": content}
    except Exception as e:
        return {"path": str(full), "exists": False, "content": "", "error": str(e)}


def scan_prefab_components(prefab_rel_path: str) -> dict:
    """扫描预制体文件中的组件类型和命名约定

    通过解析 .prefab 的 YAML 内容，提取 GameObject 名称和挂载的组件类型。
    Unity 预制体为 YAML 文本格式，可正则解析。

    Args:
        prefab_rel_path: 预制体相对路径，如 "Assets/ZMPackages/ZMUI/Resources/SelectWindow.prefab"

    Returns:
        {"name": 预制体名, "path": 相对路径, "components": [组件名列表]}
    """
    result = {"name": "", "path": prefab_rel_path, "components": [], "gameObjects": []}
    full = PROJECT_ROOT / prefab_rel_path.replace("/", os.sep).replace("\\", os.sep)
    if not full.exists():
        result["error"] = f"预制体不存在: {prefab_rel_path}"
        return result

    result["name"] = full.stem
    try:
        content = full.read_text(encoding="utf-8", errors="ignore")
    except Exception as e:
        result["error"] = str(e)
        return result

    # 提取 GameObject 名称
    go_names = re.findall(r"--- !u!1 &(\d+)\nGameObject:\n(.*?)(?=--- !u!)", content, re.DOTALL)
    for fid, body in go_names:
        name_match = re.search(r"m_Name: (.+)", body)
        if name_match:
            result["gameObjects"].append(name_match.group(1).strip())

    # 提取组件类型
    comp_types = re.findall(r"--- !u!\d+ &(\d+)\n(\w+):", content)
    seen = set()
    for fid, comp_type in comp_types:
        # 去掉 MonoBehaviour（脚本类无法从 YAML 直接获取类名，但可获取 m_Script 引用）
        if comp_type == "MonoBehaviour":
            continue
        if comp_type not in seen:
            seen.add(comp_type)
            result["components"].append(comp_type)

    return result
