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

public class ZMUIWindow : EditorWindow
{
    private UISetting mSetting;
    private SerializedObject mSerializedSetting;
    private Vector2 mScrollPos;

    // Serialized properties
    private SerializedProperty mSingMaskProp;
    private SerializedProperty mParseTypeProp;
    private SerializedProperty mGeneratorTypeProp;
    private SerializedProperty mBindPathProp;
    private SerializedProperty mFindPathProp;
    private SerializedProperty mWindowPathProp;
    private SerializedProperty mItemPathProp;
    private SerializedProperty mPrefabFolderArrProp;
    private SerializedProperty mNamespaceArrProp;

    // Cached styles (created once after skin is ready)
    private GUIStyle mSectionTitleStyle;
    private GUIStyle mSectionSubtitleStyle;
    private GUIStyle mSectionBoxStyle;
    private bool mStylesReady;

    [MenuItem("ZM/ZMUI Setting", false, 2)]
    public static void ShowWindow()
    {
        var window = GetWindow<ZMUIWindow>("ZMUI Setting");
        window.minSize = new Vector2(640, 520);
        window.Show();
    }

    private void OnEnable()
    {
        mSetting = UISetting.Instance;
        if (mSetting != null)
        {
            mSerializedSetting = new SerializedObject(mSetting);
            BindProperties();
        }
    }

    private void OnDisable()
    {
        mSetting?.Save();
    }

    private void BindProperties()
    {
        mSingMaskProp        = mSerializedSetting.FindProperty("SINGMASK_SYSTEM");
        mParseTypeProp       = mSerializedSetting.FindProperty("ParseType");
        mGeneratorTypeProp   = mSerializedSetting.FindProperty("GeneratorType");
        mBindPathProp        = mSerializedSetting.FindProperty("BindComponentGeneratorPath");
        mFindPathProp        = mSerializedSetting.FindProperty("FindComponentGeneratorPath");
        mWindowPathProp      = mSerializedSetting.FindProperty("WindowGeneratorPath");
        mItemPathProp        = mSerializedSetting.FindProperty("ItemScriptsGeneratorPath");
        mPrefabFolderArrProp = mSerializedSetting.FindProperty("WindowPrefabFolderPathArr");
        mNamespaceArrProp    = mSerializedSetting.FindProperty("UsingNameSpaceArr");
    }

    private void InitStyles()
    {
        if (mStylesReady) return;
        mStylesReady = true;

        mSectionTitleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12,
            margin = new RectOffset(0, 0, 0, 2)
        };
        mSectionSubtitleStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            wordWrap = true,
            margin = new RectOffset(0, 0, 0, 4)
        };
        mSectionBoxStyle = new GUIStyle("HelpBox")
        {
            padding = new RectOffset(12, 12, 10, 10)
        };
    }

    private void OnGUI()
    {
        InitStyles();

        if (mSetting == null || mSerializedSetting == null)
        {
            EditorGUILayout.HelpBox("未找到 UISetting 资产，请在 Resources 目录下创建 UISetting.asset", MessageType.Error);
            return;
        }

        mSerializedSetting.Update();

        DrawHeader();

        mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos);
        {
            GUILayout.Space(6);
            DrawMaskSection();
            GUILayout.Space(6);
            DrawCodeGenSection();
            GUILayout.Space(6);
            DrawPathSection();
            GUILayout.Space(6);
            DrawPrefabPathSection();
            GUILayout.Space(6);
            DrawNamespaceSection();
            GUILayout.Space(10);
        }
        EditorGUILayout.EndScrollView();

        if (mSerializedSetting.ApplyModifiedProperties())
            mSetting.Save();
    }

    // ── Header ──────────────────────────────────────────────────────────────

    private void DrawHeader()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(28));
        GUILayout.Label("⚙  ZMUI Setting",
            new GUIStyle(EditorStyles.boldLabel) { fontSize = 13, alignment = TextAnchor.MiddleLeft },
            GUILayout.ExpandWidth(true), GUILayout.Height(28));
        if (GUILayout.Button("保存设置", EditorStyles.toolbarButton, GUILayout.Width(64)))
            mSetting.Save();
        EditorGUILayout.EndHorizontal();
    }

    // ── Sections ─────────────────────────────────────────────────────────────

    private void DrawMaskSection()
    {
        BeginSection("窗口遮罩模式");
        EditorGUILayout.HelpBox(
            "True：单遮罩模式 — 多个窗口叠加时只有一个 Mask，透明度唯一\n" +
            "False：叠遮模式 — 每个窗口独立一个 Mask，透明度叠加",
            MessageType.Info);
        GUILayout.Space(4);
        mSingMaskProp.boolValue = EditorGUILayout.Toggle("启用单遮模式", mSingMaskProp.boolValue);
        EndSection();
    }

    private void DrawCodeGenSection()
    {
        BeginSection("代码自动化生成设置", "建议：名称解析 + 组件绑定（兼容性好，性能好）");

        EditorGUILayout.LabelField("组件解析方式", EditorStyles.boldLabel);
        mParseTypeProp.enumValueIndex = GUILayout.Toolbar(
            mParseTypeProp.enumValueIndex,
            new[] { "名称解析  [Button]fieldName", "标签 Tag 解析" },
            GUILayout.Height(24));

        GUILayout.Space(8);

        EditorGUILayout.LabelField("代码生成方式", EditorStyles.boldLabel);
        mGeneratorTypeProp.enumValueIndex = GUILayout.Toolbar(
            mGeneratorTypeProp.enumValueIndex,
            new[] { "组件自动查找 (Find)", "组件自动绑定 (Bind)" },
            GUILayout.Height(24));

        EndSection();
    }

    private void DrawPathSection()
    {
        BeginSection("脚本自动化生成路径配置", "自定义各类脚本的生成路径");

        mBindPathProp.stringValue = FolderPathField("组件绑定脚本路径", mBindPathProp.stringValue);
        GUILayout.Space(2);

        // 仅在"组件查找"模式下显示
        if (mGeneratorTypeProp.enumValueIndex == (int)GeneratorType.Find)
        {
            mFindPathProp.stringValue = FolderPathField("组件查找脚本路径", mFindPathProp.stringValue);
            GUILayout.Space(2);
        }

        mWindowPathProp.stringValue = FolderPathField("窗口交互脚本路径", mWindowPathProp.stringValue);
        GUILayout.Space(2);
        mItemPathProp.stringValue   = FolderPathField("Item 脚本路径",    mItemPathProp.stringValue);

        EndSection();
    }

    private void DrawPrefabPathSection()
    {
        BeginSection("窗口预制体加载路径配置", "框架根据以下路径自动计算加载路径，新增窗口无需手动配置");
        DrawStringArrayField(mPrefabFolderArrProp, "添加预制体路径", isFolder: true);
        EndSection();
    }

    private void DrawNamespaceSection()
    {
        BeginSection("自动生成脚本命名空间配置", "生成脚本时自动在顶部 using 以下命名空间");
        DrawStringArrayField(mNamespaceArrProp, "添加命名空间", isFolder: false);
        EndSection();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void BeginSection(string title, string subtitle = "")
    {
        EditorGUILayout.BeginVertical(mSectionBoxStyle);
        EditorGUILayout.LabelField(title, mSectionTitleStyle);
        if (!string.IsNullOrEmpty(subtitle))
            EditorGUILayout.LabelField(subtitle, mSectionSubtitleStyle);
        GUILayout.Space(4);
    }

    private static void EndSection()
    {
        EditorGUILayout.EndVertical();
    }

    private static string FolderPathField(string label, string path, float labelWidth = 160f)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));
        string newPath = EditorGUILayout.TextField(path, GUILayout.ExpandWidth(true));
        if (GUILayout.Button("浏览", GUILayout.Width(44)))
        {
            string selected = EditorUtility.OpenFolderPanel("选择文件夹",
                string.IsNullOrEmpty(path) ? "Assets" : path, "");
            if (!string.IsNullOrEmpty(selected))
            {
                newPath = AbsoluteToRelative(selected);
                GUI.FocusControl(null);
            }
        }
        EditorGUILayout.EndHorizontal();
        return newPath;
    }

    private static void DrawStringArrayField(SerializedProperty arrProp, string addLabel, bool isFolder)
    {
        for (int i = 0; i < arrProp.arraySize; i++)
        {
            SerializedProperty elem = arrProp.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginHorizontal();

            elem.stringValue = EditorGUILayout.TextField(elem.stringValue, GUILayout.ExpandWidth(true));

            if (isFolder && GUILayout.Button("浏览", GUILayout.Width(44)))
            {
                string selected = EditorUtility.OpenFolderPanel("选择文件夹",
                    string.IsNullOrEmpty(elem.stringValue) ? "Assets" : elem.stringValue, "");
                if (!string.IsNullOrEmpty(selected))
                {
                    elem.stringValue = AbsoluteToRelative(selected);
                    GUI.FocusControl(null);
                }
            }

            GUI.color = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("✕", GUILayout.Width(24)))
            {
                arrProp.DeleteArrayElementAtIndex(i);
                GUI.color = Color.white;
                break;
            }
            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(2);
        if (GUILayout.Button($"＋  {addLabel}", GUILayout.Height(22)))
        {
            arrProp.InsertArrayElementAtIndex(arrProp.arraySize);
            arrProp.GetArrayElementAtIndex(arrProp.arraySize - 1).stringValue = "";
        }
    }

    private static string AbsoluteToRelative(string absolutePath)
    {
        if (absolutePath.StartsWith(Application.dataPath))
            return "Assets" + absolutePath.Substring(Application.dataPath.Length);
        return absolutePath;
    }
}