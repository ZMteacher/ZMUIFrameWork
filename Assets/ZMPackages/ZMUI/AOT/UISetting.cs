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
using UnityEditor;
using UnityEngine;

public enum GeneratorType
{
    Find, // 组件查找
    Bind, // 组件绑定
}

public enum ParseType
{
    Name, // 名称解析，格式：[Button]fieldName
    Tag,  // 标签 Tag 解析
}

[CreateAssetMenu(fileName = "UISetting", menuName = "UISetting", order = 0)]
public class UISetting : ScriptableObject
{
    private static UISetting _instance;
    public static UISetting Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<UISetting>("UISetting");
            return _instance;
        }
    }

    /// <summary>
    /// True：单遮罩模式（多个窗口叠加只有一个 Mask，透明度唯一）
    /// False：叠遮模式（每个窗口独立一个 Mask，透明度叠加）
    /// </summary>
    public bool SINGMASK_SYSTEM;

    /// <summary>组件解析方式</summary>
    public ParseType ParseType = ParseType.Name;

    /// <summary>代码生成方式</summary>
    public GeneratorType GeneratorType = GeneratorType.Bind;

    /// <summary>组件绑定脚本生成路径</summary>
    public string BindComponentGeneratorPath = "";

    /// <summary>组件查找脚本生成路径（GeneratorType == Find 时生效）</summary>
    public string FindComponentGeneratorPath = "";

    /// <summary>窗口交互脚本生成路径</summary>
    public string WindowGeneratorPath = "";

    /// <summary>Item 脚本生成路径</summary>
    public string ItemScriptsGeneratorPath = "";

    /// <summary>窗口预制体存放路径列表</summary>
    public string[] WindowPrefabFolderPathArr;

    /// <summary>自动生成脚本时 using 的命名空间列表</summary>
    public string[] UsingNameSpaceArr;

    public void Save()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssetIfDirty(this);
#endif
    }
}