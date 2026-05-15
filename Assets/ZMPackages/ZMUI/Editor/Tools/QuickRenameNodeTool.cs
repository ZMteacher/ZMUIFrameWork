/*----------------------------------------------------------------------------
* Title: ZMUIFrameWork 一款Mono分离式UI管理框架
*
* Author: 铸梦xy
*
* Date: 2024/09/01 14:15:58
*
* Description: 快速将节点重命名为 [ComponentType]originalName 格式
*
* Remarks: QQ:975659933 邮箱：zhumengxyedu@163.com
*
* GitHub：https://github.com/ZMteacher?tab=repositories
----------------------------------------------------------------------------*/
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class QuickRenameNodeTool
{
    // 不参与命名的内置组件类型（前缀匹配）
    private static readonly string[] kSkipTypes =
    {
        "Transform",
        "RectTransform",
        "CanvasRenderer",
    };

    [MenuItem("GameObject/ZMUI/快速命名 [Component]name", false, -10)]
    private static void QuickRename()
    {
        var objs = Selection.gameObjects;
        if (objs == null || objs.Length == 0)
        {
            Debug.LogWarning("[QuickRename] 请先在 Hierarchy 中选中节点");
            return;
        }

        Undo.RecordObjects(objs, "QuickRename [Component]name");

        int renamed = 0;
        foreach (var obj in objs)
        {
            string compType = GetLastComponentType(obj);
            if (string.IsNullOrEmpty(compType))
            {
                Debug.LogWarning($"[QuickRename] {obj.name} 上未找到可用组件，跳过");
                continue;
            }

            // 去掉原名中已有的 [xxx] 前缀，保留字段名部分
            string baseName = StripBracketPrefix(obj.name);
            obj.name = $"[{compType}]{baseName}";
            renamed++;
        }

        if (renamed > 0)
            Debug.Log($"[QuickRename] 已重命名 {renamed} 个节点");
    }

    /// <summary>
    /// 获取节点上最后一个有效组件的类型名（跳过内置无意义组件）
    /// </summary>
    private static string GetLastComponentType(GameObject obj)
    {
        var components = obj.GetComponents<Component>();
        string result = null;
        foreach (var comp in components)
        {
            if (comp == null) continue;
            string typeName = comp.GetType().Name;
            if (IsSkipped(typeName)) continue;
            result = typeName;
        }
        return result;
    }

    /// <summary>
    /// 去掉 [xxx] 前缀，返回剩余字段名；若无前缀则返回原名
    /// </summary>
    private static string StripBracketPrefix(string name)
    {
        var match = Regex.Match(name, @"^\[[^\]]+\](.*)$");
        return match.Success ? match.Groups[1].Value : name;
    }

    private static bool IsSkipped(string typeName)
    {
        foreach (var skip in kSkipTypes)
            if (typeName == skip) return true;
        return false;
    }
}
