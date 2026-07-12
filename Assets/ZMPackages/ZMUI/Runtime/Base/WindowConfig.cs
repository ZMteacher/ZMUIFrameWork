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
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "WindowConfig", menuName = "WindowConfig", order = 0)]
public class WindowConfig : ScriptableObject
{
    public List<WindowData> windowDataList = new List<WindowData>();

    /// <summary>
    /// 生成窗口预制体加载路径
    /// </summary>
    public void GeneratorWindowConfig()
    {
        if (UISetting.Instance == null || UISetting.Instance.WindowPrefabFolderPathArr == null)
        {
            Debug.LogError("UISetting.Instance 或 WindowPrefabFolderPathArr 为空，无法生成窗口配置");
            return;
        }

        string[] windowRootArr = UISetting.Instance.WindowPrefabFolderPathArr;
        List<WindowData> scannedWindowDataList = new List<WindowData>();

        string projectRootPath = Application.dataPath.Replace("Assets", string.Empty);
        foreach (var item in windowRootArr)
        {
            if (string.IsNullOrEmpty(item))
            {
                continue;
            }

            string folder = projectRootPath + item;
            if (!Directory.Exists(folder))
            {
                Debug.LogWarning("窗口预制体目录不存在: " + folder);
                continue;
            }

            string[] filePathArr = Directory.GetFiles(folder, "*.prefab", SearchOption.AllDirectories);
            foreach (var path in filePathArr)
            {
                if (path.EndsWith(".meta"))
                {
                    continue;
                }

                string fileName = Path.GetFileNameWithoutExtension(path);
                string filePath = item + "/" + fileName;
                scannedWindowDataList.Add(new WindowData { name = fileName, path = NormalizePath(filePath) });
            }
        }

        // 同名窗口会导致按 name 查询时产生歧义，这里提前给出明确错误。
        HashSet<string> uniqueNames = new HashSet<string>();
        foreach (var data in scannedWindowDataList)
        {
            if (!uniqueNames.Add(data.name))
            {
                Debug.LogError("检测到重复窗口名: " + data.name + "，请确保窗口名唯一");
            }
        }

        bool needUpdate = !IsSameWindowData(scannedWindowDataList, windowDataList);
        if (!needUpdate)
        {
            // Debug.Log("预制体配置无变化，不生成窗口配置");
            return;
        }

        windowDataList.Clear();
        windowDataList.AddRange(scannedWindowDataList);

#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(this)))
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
#endif
    }
    /// <summary>
    /// 是否是相同配置数据
    /// </summary>
    /// <param name="latest"></param>
    /// <param name="current"></param>
    /// <returns></returns>
    private static bool IsSameWindowData(List<WindowData> latest, List<WindowData> current)
    {
        if (latest == null || current == null) return false;
       
        if (latest.Count != current.Count)  return false;
 
        Dictionary<string, string> currentMap = new Dictionary<string, string>();
        foreach (var item in current)
        {
            if (item == null || string.IsNullOrEmpty(item.name))
            {
                return false;
            }

            string normalizedPath = NormalizePath(item.path);
            if (currentMap.ContainsKey(item.name))
            {
                return false;
            }
            currentMap.Add(item.name, normalizedPath);
        }

        foreach (var item in latest)
        {
            if (item == null || string.IsNullOrEmpty(item.name))
            {
                return false;
            }

            string normalizedPath = NormalizePath(item.path);
            string oldPath;
            if (!currentMap.TryGetValue(item.name, out oldPath))
            {
                return false;
            }

            if (!string.Equals(oldPath, normalizedPath))
            {
                return false;
            }
        }

        return true;
    }
    /// <summary>
    /// 统一路径符号
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        return path.Replace('\\', '/').Trim();
    }

    /// <summary>
    /// 添加窗口元数据 (在多模块资源+独立代码热更程序集时使用) 主要作用是添加热更窗口数据至AOT或热更程序集内
    /// </summary>
    public void AddAOTWindowMetadata(WindowConfig windowConfig)
    {
        foreach (var item in windowConfig.windowDataList)
        {
            if (GetWindowData(item.name, false)==null)
            {
                windowDataList.Add(item);
                Debug.Log("补充窗口元数据:"+item.name);
            }
        }
    }
    

    /// <summary>
    /// 获取窗口数据
    /// </summary>
    /// <param name="wndName">窗口名称</param>
    /// <param name="log">是否打印窗口不存在日志.</param>
    /// <returns></returns>
    public WindowData GetWindowData(string wndName,bool log=true)
    {
        foreach (var item in windowDataList)
        {
            if (string.Equals(item.name,wndName))
            {
                return item;
            }
        }
        if (log)
            Debug.LogError(wndName+"不存在在配置文件中，请检查预制体存放位置，或配置文件");
        return null;
    }
}
[System.Serializable]
public class WindowData
{
    public string name;
    public string path;
}