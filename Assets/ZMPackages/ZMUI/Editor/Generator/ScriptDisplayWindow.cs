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
using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;


public class ScriptDisplayWindow : EditorWindow
{
    private const float HeaderHeight = 78f;
    private const float FooterHeight = 126f;
    private string scriptContent;
    private string filePath;
    private string mFileName;
    private string pathPreferenceKey;
    private Vector2 scroll = new Vector2();
    [NonSerialized] private GUIStyle codeStyle;
    [NonSerialized] private bool themeAcquired;
    /// <summary>
    /// 显示代码展示窗口
    /// </summary>
    public static void ShowWindow(string content, string filePath,
        Dictionary<string, string> _insertDic = null, List<EditorObjectData> fieldList = null,
        string generationPathPreferenceKey = null)
    {
        //创建代码展示窗口
        ScriptDisplayWindow window = GetWindow<ScriptDisplayWindow>(
            false, "ZMUI Window 生成器", true);
        window.minSize = new Vector2(860, 620);
        window.maxSize = new Vector2(4096, 4096);
        // GetWindow 创建普通可停靠 EditorWindow；仅在首次打开时设置推荐尺寸，
        // 后续保留用户自行调整后的窗口位置和大小。
        if (window.position.width < window.minSize.x || window.position.height < window.minSize.y)
            window.position = new Rect(100, 50, 980, 720);
        window.scriptContent = content;
        window.filePath = filePath;
        window.mFileName = Path.GetFileName(filePath);
        window.pathPreferenceKey = generationPathPreferenceKey;
        //处理代码新增
        string originScript = string.Empty;
        bool isInsterSuccess = false;
        
        if (File.Exists(window.filePath) && (_insertDic!=null || fieldList!=null))
        {
            originScript = File.ReadAllText(window.filePath);
            
            if (string.IsNullOrEmpty(originScript) == false)
            {
                if (fieldList!=null)
                {
                    //插入字段(生成item脚本时使用)
                    foreach (var item in fieldList)
                    {
                        if (!originScript.Contains($"{item.fieldName}{item.fieldType}"))
                        {
                            string insterArrayType = item.dataList != null && item.dataList.Count > 0 ? "[]" : "";
                            string insterArray = item.dataList != null && item.dataList.Count > 0 ? "Array" : "";
                            //插入新增的数据
                            originScript = window.scriptContent = originScript.Insert(window.GetInsertFieldIndex(originScript)
                                , $"public { item.fieldType }{insterArrayType} {item.fieldName}{item.fieldType}{insterArray};\n\n\t\t");
                            isInsterSuccess = true;
                        }
                    }
                }
                if (_insertDic != null)
                {
                    //插入方法
                    foreach (var item in _insertDic)
                    {
                        if (!originScript.Contains(item.Key))
                        {
                            int insterIndex = window.GetInsertMethodIndex(originScript);
                            //插入新增的数据
                            originScript = window.scriptContent = originScript.Insert(insterIndex,"\n"+ item.Value+"\n\t\t");
                            isInsterSuccess = true;
                        }
                    }
                }


                if (fieldList!=null)
                {
                 
                    //插入事件(生成item脚本时使用)
                    foreach (var item in fieldList)
                    {  
                        string field = $"{item.fieldName}{item.fieldType}";
                        string type = item.fieldType;
                        string methodName = "On" + item.fieldName;
                        string suffix = "";
                        StringBuilder sb=new StringBuilder();
                        if (type.Contains("Button"))
                        {
                            suffix = "ButtonClick";
                            sb.AppendLine($"\t\t\t{field}.onClick.AddListener({methodName}{suffix});");
                        }
                        else if (type.Contains("InputField"))
                        {
                            suffix = "InputChange";
                            sb.AppendLine($"\t\t\t{field}.onValueChanged.AddListener({methodName}{suffix});");
                            suffix = "InputEnd";
                            sb.AppendLine($"\t\t\t{field}.onEndEdit.AddListener({methodName}{suffix});");
                        }
                        else if (type.Contains("Toggle"))
                        {
                            suffix = "ToggleChange";
                            sb.AppendLine($"\t\t\t{field}.onValueChanged.AddListener({methodName}{suffix});");
                        }
                        else
                        {
                            continue;
                        }
                        if (!originScript.Contains($"AddListener({methodName}{suffix})"))
                        {
                            sb.Insert(0,"//按钮事件自动注册绑定\n");
                            originScript = window.scriptContent = originScript.Replace("//按钮事件自动注册绑定", $"{sb.ToString()}");
                            isInsterSuccess = true;
                        }
                    }
                }
            }
            
            if (isInsterSuccess == false)
            {
                window.scriptContent = originScript;
            }
        }

        originScript = null;
        _insertDic = null;
        window.Show();
        window.Focus();
    }

    private void OnEnable()
    {
        // Unity 域重载期间 EditorStyles 可能尚未就绪，主题在首次 OnGUI 中初始化。
        themeAcquired = false;
        codeStyle = null;
    }

    private void OnDisable()
    {
        if (themeAcquired)
            ZMUIEditorTheme.Release();
        themeAcquired = false;
        codeStyle = null;
    }

    public void OnGUI()
    {
        EnsureTheme();
        EnsureLocalStyles();
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), ZMUIEditorTheme.Window);
        DrawHeader();
        DrawCodePreview();
        DrawFooter();
    }

    private void EnsureTheme()
    {
        if (themeAcquired) return;
        ZMUIEditorTheme.Acquire();
        themeAcquired = true;
    }

    private void DrawHeader()
    {
        Rect header = new Rect(0, 0, position.width, HeaderHeight);
        EditorGUI.DrawRect(header, ZMUIEditorTheme.Header);
        EditorGUI.DrawRect(new Rect(0, header.yMax - 1, header.width, 1), ZMUIEditorTheme.Border);

        Rect iconBox = new Rect(22, 17, 44, 44);
        GUI.Box(iconBox, GUIContent.none, ZMUIEditorTheme.ModeIconBoxSelected);
        ZMUIEditorIcons.Draw(new Rect(iconBox.x + 10, iconBox.y + 10, 24, 24),
            ZMUIEditorIcons.Icon.Code, ZMUIEditorTheme.Accent, 1.8f);
        GUI.Label(new Rect(80, 12, 400, 31), "Window 脚本生成", ZMUIEditorTheme.PageTitle);
        GUI.Label(new Rect(80, 43, 390, 20), "预览并确认组件字段、生命周期与 UI 事件代码",
            ZMUIEditorTheme.PageSubtitle);

        bool updating = !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath);
        Rect status = new Rect(header.xMax - 128, 22, 104, 34);
        GUI.Box(status, updating ? "更新现有脚本" : "创建新脚本", ZMUIEditorTheme.StatusChip);
    }

    private void DrawCodePreview()
    {
        Rect card = new Rect(20, HeaderHeight + 16, position.width - 40,
            Mathf.Max(120, position.height - HeaderHeight - FooterHeight - 28));
        GUI.Box(card, GUIContent.none, ZMUIEditorTheme.CardBox);
        GUI.Label(new Rect(card.x + 18, card.y + 12, 160, 24), "代码预览",
            ZMUIEditorTheme.CardTitle);
        GUI.Label(new Rect(card.xMax - 108, card.y + 13, 88, 22),
            $"{CountLines(scriptContent)} 行", new GUIStyle(ZMUIEditorTheme.Hint)
            {
                alignment = TextAnchor.MiddleRight
            });

        Rect viewport = new Rect(card.x + 18, card.y + 44, card.width - 36, card.height - 60);
        GUI.Box(viewport, GUIContent.none, ZMUIEditorTheme.TableBox);
        Rect scrollViewport = new Rect(viewport.x + 1, viewport.y + 1,
            viewport.width - 2, viewport.height - 2);
        float contentHeight = Mathf.Max(scrollViewport.height - 4,
            CountLines(scriptContent) * 17f + 24f);
        Rect contentRect = new Rect(0, 0, scrollViewport.width - 18, contentHeight);
        scroll = GUI.BeginScrollView(scrollViewport, scroll, contentRect);
        scriptContent = GUI.TextArea(new Rect(0, 0, contentRect.width, contentRect.height),
            scriptContent ?? string.Empty, codeStyle);
        GUI.EndScrollView();
    }

    private void DrawFooter()
    {
        float top = position.height - FooterHeight + 8;
        GUI.Label(new Rect(22, top, 120, 20), "脚本生成路径", ZMUIEditorTheme.Label);
        Rect pathBox = new Rect(20, top + 25, position.width - 96, 38);
        GUI.Box(pathBox, GUIContent.none, ZMUIEditorTheme.FieldBox);
        EditorGUI.SelectableLabel(new Rect(pathBox.x + 12, pathBox.y + 1,
            pathBox.width - 24, pathBox.height - 2),
            string.IsNullOrWhiteSpace(filePath) ? "请选择脚本输出位置…" : filePath,
            ZMUIEditorTheme.Input);

        Rect folderButton = new Rect(pathBox.xMax + 8, pathBox.y, 48, 38);
        if (GUI.Button(folderButton, GUIContent.none, ZMUIEditorTheme.IconButton))
            SelectOutputFolder();
        ZMUIEditorIcons.Draw(new Rect(folderButton.x + 14, folderButton.y + 10, 20, 20),
            ZMUIEditorIcons.Icon.Folder, ZMUIEditorTheme.Text, 1.6f);

        Rect generateButton = new Rect(position.width - 166, position.height - 49, 146, 38);
        Rect cancelButton = new Rect(generateButton.x - 104, generateButton.y, 94, 38);
        if (GUI.Button(cancelButton, "取消", ZMUIEditorTheme.SecondaryButton)) Close();
        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(filePath) ||
                                            string.IsNullOrWhiteSpace(scriptContent)))
        {
            if (GUI.Button(generateButton, "生成脚本", ZMUIEditorTheme.PrimaryButton))
                ButtonClick();
        }
    }

    private void SelectOutputFolder()
    {
        string initialDirectory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(initialDirectory) || !Directory.Exists(initialDirectory))
            initialDirectory = Application.dataPath;
        string selectedDirectory = EditorUtility.OpenFolderPanel("选择脚本生成目录",
            initialDirectory, "ZMUI");
        if (string.IsNullOrEmpty(selectedDirectory)) return;
        string selectedPath = Path.Combine(selectedDirectory, string.IsNullOrEmpty(mFileName)
            ? "Window.cs"
            : mFileName);
        if (!TryGetAssetPath(selectedPath, out string assetPath))
        {
            EditorUtility.DisplayDialog("目录不可用",
                "ZMUI 脚本必须生成在当前项目的 Assets 目录内。", "确定");
            return;
        }
        filePath = selectedPath;
        EditorPrefs.SetString("GeneratorClassPath", filePath);
        if (!string.IsNullOrEmpty(pathPreferenceKey))
            EditorPrefs.SetString(pathPreferenceKey, assetPath);
        Repaint();
    }

    private void EnsureLocalStyles()
    {
        if (codeStyle != null) return;
        codeStyle = new GUIStyle(ZMUIEditorTheme.CodeCell)
        {
            alignment = TextAnchor.UpperLeft,
            wordWrap = false,
            padding = new RectOffset(12, 12, 10, 10),
            fontSize = 12
        };
        Font monoFont = EditorGUIUtility.Load("Fonts/RobotoMono/RobotoMono-Regular.ttf") as Font;
        if (monoFont != null) codeStyle.font = monoFont;
        codeStyle.normal.background = null;
        codeStyle.focused.background = null;
    }

    private static int CountLines(string content)
    {
        if (string.IsNullOrEmpty(content)) return 0;
        int lines = 1;
        for (int index = 0; index < content.Length; index++)
            if (content[index] == '\n') lines++;
        return lines;
    }

    public void ButtonClick()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new InvalidOperationException("脚本生成路径为空。");
            string fullPath = Path.GetFullPath(filePath);
            if (!TryGetAssetPath(fullPath, out string assetPath))
                throw new InvalidOperationException("ZMUI 脚本必须生成在当前项目的 Assets 目录内。");
            string directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(directory))
                throw new InvalidOperationException("无法解析脚本生成目录。");
            Directory.CreateDirectory(directory);
            File.WriteAllText(fullPath, scriptContent ?? string.Empty, new UTF8Encoding(false));
            if (!string.IsNullOrEmpty(pathPreferenceKey))
                EditorPrefs.SetString(pathPreferenceKey, assetPath);
            Debug.Log($"[ZMUI] Window 脚本生成完成：{fullPath}");
            AssetDatabase.Refresh();
            ZMUIMessageWindow.ShowSuccess(this, "Window 脚本生成成功",
                "代码已写入目标目录，可以继续进行界面开发。", fullPath);
            Close();
        } 
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog("生成失败", exception.Message, "确定");
        }
     }

    private static bool TryGetAssetPath(string path, out string assetPath)
    {
        assetPath = string.Empty;
        if (string.IsNullOrWhiteSpace(path)) return false;
        string fullPath = Path.GetFullPath(path).Replace('\\', '/');
        string assetsRoot = Path.GetFullPath(Application.dataPath).Replace('\\', '/').TrimEnd('/');
        if (!fullPath.StartsWith(assetsRoot + "/", StringComparison.OrdinalIgnoreCase))
            return false;
        assetPath = "Assets" + fullPath.Substring(assetsRoot.Length);
        return true;
    }
    /// <summary>
    /// 获取插入代码的下标
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public int GetInsertMethodIndex(string content)
    {
        //找到UI事件组件下面的第一个public 所在的位置 进行插入
        Regex regex = new Regex("UI组件事件");
        Match match = regex.Match(content);
        return match.Index+6;
    }
    public int GetInsertFieldIndex(string content)
    {
        //找到UI事件组件下面的第一个public 所在的位置 进行插入
        Regex regex = new Regex("自定义字段");
        Match match = regex.Match(content);
        Regex regex1 = new Regex("public");
        MatchCollection matchColltion = regex1.Matches(content);

        for (int i = 0; i < matchColltion.Count; i++)
        {
            if (matchColltion[i].Index > match.Index)
            {
                return matchColltion[i].Index;
            }
        }
        return -1;
    }
     
}
