"""ZMUI MCP 服务器
为 AI 提供 ZMUIFrameWork 项目的窗口查询、配置读取、代码生成等工具。

运行方式:
    python server.py
    或: mcp run server.py
"""

import sys
from pathlib import Path

# 确保可以导入 tools 模块
sys.path.insert(0, str(Path(__file__).parent))

try:
    from mcp.server.fastmcp import FastMCP
except ImportError:
    print("缺少 mcp 依赖，请先执行: pip install -r requirements.txt", file=sys.stderr)
    sys.exit(1)

from tools import code_generator, project_reader, window_query

# 创建 MCP 服务器
mcp = FastMCP(
    "zmui-mcp-server",
    instructions=(
        "ZMUI Unity UI 框架辅助工具集。提供窗口配置查询、UISetting 配置读取、"
        "项目结构浏览、窗口/DataComponent/Item 脚本代码生成能力。"
    ),
)


# ============ 窗口配置查询 ============

@mcp.tool()
def query_window_config(filter_name: str = "") -> dict:
    """查询 ZMUI 框架的 WindowConfig 配置，返回所有已注册的窗口及其预制体路径。

    Args:
        filter_name: 可选，按窗口名模糊过滤（如 "Hall" 匹配 HallDemoWindow）

    Returns:
        窗口列表，包含 name（窗口名）和 path（预制体资源路径）
    """
    return window_query.query_window_config(filter_name or None)


@mcp.tool()
def query_ui_setting() -> dict:
    """查询 ZMUI 框架的 UISetting 配置。

    包含：组件解析方式(ParseType)、代码生成方式(GeneratorType)、
    各类脚本生成路径、窗口预制体存放目录、组件映射表等。

    Returns:
        UISetting 配置信息
    """
    return window_query.query_ui_setting()


@mcp.tool()
def query_windows_in_folders() -> dict:
    """扫描 UISetting 配置的预制体目录，列出其中所有窗口预制体。

    Returns:
        扫描目录列表及窗口预制体清单
    """
    return window_query.query_windows_in_folders()


# ============ 项目结构查询 ============

@mcp.tool()
def query_project_structure(path: str = "Assets", depth: int = 2) -> dict:
    """查询项目目录结构树。

    Args:
        path: 相对项目根目录的路径，如 "Assets/ZMPackages/ZMUI"
        depth: 目录展开深度（1-4）

    Returns:
        目录结构树（dir/file 节点）
    """
    depth = max(1, min(4, int(depth)))
    return project_reader.list_directory_tree(path or None, depth)


@mcp.tool()
def find_prefabs(path: str = "") -> dict:
    """查找项目中的窗口预制体文件。

    Args:
        path: 相对项目根目录的搜索路径，为空则搜索全部 Assets

    Returns:
        预制体列表 [{name, path}]
    """
    prefabs = project_reader.find_prefabs(path or None)
    return {"prefabs": prefabs, "total": len(prefabs)}


@mcp.tool()
def read_project_file(rel_path: str) -> dict:
    """读取项目中的文本文件内容（C# 脚本、配置文件等）。

    Args:
        rel_path: 相对项目根目录的路径，如 "Assets/ZMPackages/ZMUI/Runtime/Core/UIModule.cs"

    Returns:
        文件内容
    """
    return project_reader.read_file(rel_path)


@mcp.tool()
def scan_prefab_components(prefab_rel_path: str) -> dict:
    """扫描 Unity 预制体文件，提取其中的 GameObject 名称和组件类型列表。

    Args:
        prefab_rel_path: 预制体相对路径，如 "Assets/ZMPackages/ZMUI/Resources/SelectWindow.prefab"

    Returns:
        预制体中的组件类型和 GameObject 名称
    """
    return project_reader.scan_prefab_components(prefab_rel_path)


# ============ 代码生成 ============

@mcp.tool()
def generate_window_script(
    window_name: str,
    components: list = None,
    full_screen: bool = False,
    enable_update: bool = False,
    disable_anim: bool = False,
    namespace_name: str = "ZM.UI",
) -> dict:
    """生成 ZMUI 窗口脚本（WindowBase 子类）。

    Args:
        window_name: 窗口类名（须与预制体名一致）
        components: 组件列表，格式 [{"name": "CloseButton", "type": "Button"}]，
                    或字符串 "CloseButton:Button,TitleText:Text"
        full_screen: 是否标记为全屏窗口（启用智能显隐）
        enable_update: 是否开启 OnUpdate 渲染帧更新
        disable_anim: 是否禁用窗口弹出动画
        namespace_name: 命名空间，空字符串表示不使用

    Returns:
        生成的脚本内容及建议保存路径
    """
    try:
        content = code_generator.generate_window_script(
            window_name, components, full_screen, enable_update, disable_anim, namespace_name
        )
        return {
            "content": content,
            "suggested_path": f"Assets/Scripts/AutoGenerate/Window/{window_name}.cs",
            "note": "实际保存路径应以 UISetting.WindowGeneratorPath 配置为准",
        }
    except ValueError as e:
        return {"error": str(e)}


@mcp.tool()
def generate_data_component(
    window_name: str,
    components: list = None,
    namespace_name: str = "ZM.UI",
) -> dict:
    """生成 ZMUI 窗口数据组件脚本（DataComponent，含事件绑定代码）。

    Args:
        window_name: 窗口类名
        components: 组件列表，格式 [{"name": "CloseButton", "type": "Button"}]，
                    或字符串 "CloseButton:Button,TitleText:Text"
        namespace_name: 命名空间，空字符串表示不使用

    Returns:
        生成的脚本内容及建议保存路径
    """
    try:
        content = code_generator.generate_data_component(window_name, components, namespace_name)
        return {
            "content": content,
            "suggested_path": f"Assets/Scripts/AutoGenerate/BindCompoent/{window_name}DataComponent.cs",
            "note": "实际保存路径应以 UISetting.BindComponentGeneratorPath 配置为准",
        }
    except ValueError as e:
        return {"error": str(e)}


@mcp.tool()
def generate_item_script(
    item_name: str,
    components: list = None,
    data_comment: str = "// 在此处添加数据字段",
) -> dict:
    """生成 ZMUI 列表项脚本（实现 IZMUIViewListItem 接口）。

    Args:
        item_name: Item 类名
        components: 组件列表，格式 [{"name": "IconImage", "type": "Image"}]，
                    或字符串 "IconImage:Image,NameText:Text"
        data_comment: 数据字段注释

    Returns:
        生成的脚本内容及建议保存路径
    """
    try:
        content = code_generator.generate_item_script(item_name, components, data_comment)
        return {
            "content": content,
            "suggested_path": f"Assets/Scripts/AutoGenerate/Item/{item_name}.cs",
            "note": "实际保存路径应以 UISetting.ItemScriptsGeneratorPath 配置为准",
        }
    except ValueError as e:
        return {"error": str(e)}


@mcp.tool()
def save_generated_script(content: str, rel_path: str) -> dict:
    """将生成的脚本内容保存到项目中。

    Args:
        content: 脚本内容
        rel_path: 相对项目根目录的保存路径，如 "Assets/Scripts/AutoGenerate/Window/MyWindow.cs"

    Returns:
        保存结果
    """
    return code_generator.save_script(content, rel_path)


if __name__ == "__main__":
    mcp.run()
