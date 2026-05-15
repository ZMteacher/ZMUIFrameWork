/*----------------------------------------------------------------------------
* Title: ZMUIFrameWork 一款Mono分离式UI管理框架
*
* Author: 铸梦xy
*
* Date: 2024/09/01 14:15:58
*
* Description: 高性能、自动化、自定义生命周期工作管线是该框架的特点，该框架属于MVC中的View层架构。
* 设计简洁清晰、轻便小巧，可以对接至任意重中小型游戏项目中。
*
* Remarks: QQ:975659933 邮箱：zhumengxyedu@163.com
*
* GitHub：https://github.com/ZMteacher?tab=repositories
----------------------------------------------------------------------------*/
using System.Collections.Generic;
using UnityEngine;


/// <summary>用于 JsonUtility 序列化 List&lt;EditorObjectData&gt; 的包装类</summary>
[System.Serializable]
public class EditorObjectDataWrapper
{
    public List<EditorObjectData> items;
}

public class GeneratorConfig
{
  
    public static string OBJDATALIST_KEY = "objDataList";

    /// <summary>
    /// 内置默认映射（UISetting 未配置时回退使用）
    /// Key 与 ComponentType 相同，表示字面量即类型名
    /// </summary>
    public static readonly ComponentMapping[] DefaultMappings =
    {
        new ComponentMapping { Key = "Text",           ComponentType = "Text" },
        new ComponentMapping { Key = "Image",          ComponentType = "Image" },
        new ComponentMapping { Key = "RawImage",       ComponentType = "RawImage" },
        new ComponentMapping { Key = "Button",         ComponentType = "Button" },
        new ComponentMapping { Key = "InputField",     ComponentType = "InputField" },
        new ComponentMapping { Key = "Toggle",         ComponentType = "Toggle" },
        new ComponentMapping { Key = "Slider",         ComponentType = "Slider" },
        new ComponentMapping { Key = "Scrollbar",      ComponentType = "Scrollbar" },
        new ComponentMapping { Key = "Dropdown",       ComponentType = "Dropdown" },
        new ComponentMapping { Key = "Canvas",         ComponentType = "Canvas" },
        new ComponentMapping { Key = "Panel",          ComponentType = "Panel" },
        new ComponentMapping { Key = "ScrollRect",     ComponentType = "ScrollRect" },
        new ComponentMapping { Key = "LoopListView2",  ComponentType = "LoopListView2" },
        new ComponentMapping { Key = "Transform",      ComponentType = "Transform" },
        new ComponentMapping { Key = "RectTransform",  ComponentType = "RectTransform" },
        new ComponentMapping { Key = "GameObject",     ComponentType = "GameObject" },
    };

    /// <summary>
    /// 获取当前生效的映射表（优先 UISetting，否则用内置默认值）
    /// </summary>
    public static ComponentMapping[] GetMappings()
    {
        var mappings = UISetting.Instance?.ComponentMappings;
        return (mappings != null && mappings.Length > 0) ? mappings : DefaultMappings;
    }

    /// <summary>
    /// 所有合法的 Key 列表（用于 Tag 解析的合法性判断）
    /// </summary>
    public static string[] TAGArr
    {
        get
        {
            var mappings = GetMappings();
            var keys = new string[mappings.Length];
            for (int i = 0; i < mappings.Length; i++)
                keys[i] = mappings[i].Key;
            return keys;
        }
    }

    /// <summary>
    /// 根据 Key 查找对应的 ComponentType（找不到则返回 key 本身）
    /// </summary>
    public static string GetComponentType(string key)
    {
        foreach (var m in GetMappings())
            if (m.Key == key) return m.ComponentType;
        return key;
    }

    /// <summary>
    /// 将 List&lt;EditorObjectData&gt; 序列化为 JSON 字符串（使用 Unity JsonUtility）
    /// </summary>
    public static string SerializeDataList(List<EditorObjectData> list)
    {
        var wrapper = new EditorObjectDataWrapper { items = list ?? new List<EditorObjectData>() };
        return JsonUtility.ToJson(wrapper);
    }

    /// <summary>
    /// 将 JSON 字符串反序列化为 List&lt;EditorObjectData&gt;（使用 Unity JsonUtility）
    /// </summary>
    public static List<EditorObjectData> DeserializeDataList(string json)
    {
        if (string.IsNullOrEmpty(json)) return new List<EditorObjectData>();
        var wrapper = JsonUtility.FromJson<EditorObjectDataWrapper>(json);
        return wrapper?.items ?? new List<EditorObjectData>();
    }
}
