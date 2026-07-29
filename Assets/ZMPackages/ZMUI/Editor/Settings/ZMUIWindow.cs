using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// ZMUI 配置中心。
/// 该类只负责窗口状态与页面编排，视觉资源由 <see cref="ZMUIEditorTheme"/> 管理，
/// 配置持久化仍由 UISetting 负责，避免编辑器表现层侵入运行时数据。
/// </summary>
public sealed class ZMUIWindow : EditorWindow
{
    private enum Page
    {
        Mask,
        CodeGenerator,
        ScriptPaths,
        PrefabPaths,
        Namespaces,
        Manual
    }

    private readonly struct NavigationItem
    {
        internal readonly string Label;
        internal readonly ZMUIEditorIcons.Icon Icon;

        internal NavigationItem(string label, ZMUIEditorIcons.Icon icon)
        {
            Label = label;
            Icon = icon;
        }
    }

    private static readonly NavigationItem[] Navigation =
    {
        new NavigationItem("遮罩策略", ZMUIEditorIcons.Icon.Mask),
        new NavigationItem("代码生成", ZMUIEditorIcons.Icon.Code),
        new NavigationItem("生成路径", ZMUIEditorIcons.Icon.Folder),
        new NavigationItem("预制体路径", ZMUIEditorIcons.Icon.Prefab),
        new NavigationItem("命名空间", ZMUIEditorIcons.Icon.Namespace),
        new NavigationItem("使用手册", ZMUIEditorIcons.Icon.Book)
    };

    private static readonly Dictionary<string, string> ExampleNames = new Dictionary<string, string>
    {
        { "Text", "title" },
        { "Image", "icon" },
        { "RawImage", "avatar" },
        { "Button", "confirm" },
        { "InputField", "input" },
        { "Toggle", "check" },
        { "Slider", "progress" },
        { "Scrollbar", "scroll" },
        { "Dropdown", "option" },
        { "Canvas", "canvas" },
        { "Panel", "panel" },
        { "ScrollRect", "list" },
        { "LoopListView2", "role" },
        { "Transform", "node" },
        { "RectTransform", "rect" },
        { "GameObject", "item" }
    };

    private const float HeaderHeight = 72f;
    private const float SidebarWidth = 220f;
    private const float NavigationHeight = 52f;
    private const float ContentPadding = 22f;
    private const string ApiDocumentationUrl = "https://www.zm-doc.com/ZMUI/";

    [SerializeField] private Page selectedPage = Page.CodeGenerator;
    [SerializeField] private Vector2 scrollPosition;

    private UISetting setting;
    private SerializedObject serializedSetting;
    private SerializedProperty singleMask;
    private SerializedProperty parseType;
    private SerializedProperty generatorType;
    private SerializedProperty bindPath;
    private SerializedProperty findPath;
    private SerializedProperty windowPath;
    private SerializedProperty itemPath;
    private SerializedProperty prefabPaths;
    private SerializedProperty namespaces;
    private SerializedProperty componentMappings;
    private bool themeAcquired;
    private double savedFeedbackUntil;

    [MenuItem("ZM/ZMUI Setting", false, 300)]
    public static void Open()
    {
        ZMUIWindow window = GetWindow<ZMUIWindow>();
        window.titleContent = new GUIContent("ZMUI", EditorGUIUtility.IconContent("Canvas Icon").image);
        window.minSize = new Vector2(920, 650);
        window.Show();
    }

    private void OnEnable()
    {
        if (!themeAcquired)
        {
            ZMUIEditorTheme.Acquire();
            themeAcquired = true;
        }
        titleContent = new GUIContent("ZMUI", EditorGUIUtility.IconContent("Canvas Icon").image);
        wantsMouseMove = true;
        BindSetting();
    }

    private void OnDisable()
    {
        SaveSetting(false);
        if (!themeAcquired) return;
        ZMUIEditorTheme.Release();
        themeAcquired = false;
    }

    private void BindSetting()
    {
        setting = UISetting.Instance;
        if (setting == null)
        {
            serializedSetting = null;
            return;
        }

        serializedSetting = new SerializedObject(setting);
        singleMask = serializedSetting.FindProperty("SINGMASK_SYSTEM");
        parseType = serializedSetting.FindProperty("ParseType");
        generatorType = serializedSetting.FindProperty("GeneratorType");
        bindPath = serializedSetting.FindProperty("BindComponentGeneratorPath");
        findPath = serializedSetting.FindProperty("FindComponentGeneratorPath");
        windowPath = serializedSetting.FindProperty("WindowGeneratorPath");
        itemPath = serializedSetting.FindProperty("ItemScriptsGeneratorPath");
        prefabPaths = serializedSetting.FindProperty("WindowPrefabFolderPathArr");
        namespaces = serializedSetting.FindProperty("UsingNameSpaceArr");
        componentMappings = serializedSetting.FindProperty("ComponentMappings");
    }

    private void OnGUI()
    {
        ZMUIEditorTheme.Ensure();
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), ZMUIEditorTheme.Window);

        if (setting == null || serializedSetting == null)
        {
            DrawMissingSetting();
            return;
        }

        serializedSetting.Update();
        DrawHeader();
        DrawSidebar();
        DrawContent();

        if (serializedSetting.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(setting);
            savedFeedbackUntil = 0;
        }

        if (EditorApplication.timeSinceStartup < savedFeedbackUntil)
            Repaint();
    }

    private void DrawMissingSetting()
    {
        Rect panel = new Rect((position.width - 520) * .5f, (position.height - 190) * .5f, 520, 190);
        GUI.Box(panel, GUIContent.none, ZMUIEditorTheme.CardBox);
        GUI.Label(new Rect(panel.x + 24, panel.y + 22, panel.width - 48, 28), "未找到 ZMUI 配置", ZMUIEditorTheme.PageTitle);
        GUI.Label(new Rect(panel.x + 24, panel.y + 62, panel.width - 48, 44),
            "请在 Resources 目录中创建 UISetting.asset，然后重新打开配置中心。",
            ZMUIEditorTheme.Body);
        if (GUI.Button(new Rect(panel.xMax - 144, panel.yMax - 58, 120, 38), "重新加载", ZMUIEditorTheme.PrimaryButton))
            BindSetting();
    }

    private void DrawHeader()
    {
        Rect header = new Rect(0, 0, position.width, HeaderHeight);
        EditorGUI.DrawRect(header, ZMUIEditorTheme.Header);
        EditorGUI.DrawRect(new Rect(0, header.yMax - 1, header.width, 1), ZMUIEditorTheme.Border);

        Rect logo = new Rect(24, 18, 36, 36);
        GUI.Box(logo, GUIContent.none, ZMUIEditorTheme.Badge);
        ZMUIEditorIcons.Draw(new Rect(31, 25, 22, 22), ZMUIEditorIcons.Icon.Logo,
            new Color32(133, 148, 255, 255), 1.6f);
        GUI.Label(new Rect(74, 14, 260, 44), "ZMUI 配置中心", ZMUIEditorTheme.HeaderTitle);
        GUI.Label(new Rect(340, 25, 62, 24), "v1.0.0", ZMUIEditorTheme.Badge);

        float right = position.width - 22;
        DrawHeaderChip(ref right, 124,
            EditorApplication.timeSinceStartup < savedFeedbackUntil ? "配置已保存" : "配置已同步",
            ZMUIEditorIcons.Icon.Saved, ZMUIEditorTheme.Success);
        DrawHeaderChip(ref right, 112,
            generatorType.enumValueIndex == (int)GeneratorType.Bind ? "Bind 模式" : "Find 模式",
            generatorType.enumValueIndex == (int)GeneratorType.Bind ? ZMUIEditorIcons.Icon.Link : ZMUIEditorIcons.Icon.Search,
            ZMUIEditorTheme.Accent);
        DrawHeaderChip(ref right, 78, "UGUI", ZMUIEditorIcons.Icon.UGUI, ZMUIEditorTheme.Cyan);
        DrawThemeChip(ref right);
    }

    private void DrawThemeChip(ref float right)
    {
        const float width = 128f;
        Rect rect = new Rect(right - width, 19, width, 34);
        Color color = ZMUIEditorTheme.Accent;
        Color previous = GUI.color;
        GUI.color = Color.Lerp(Color.white, color, .25f);
        GUI.Box(rect, GUIContent.none, ZMUIEditorTheme.StatusChip);
        GUI.color = previous;

        ZMUIEditorIcons.Draw(new Rect(rect.x + 11, rect.y + 9, 16, 16),
            ZMUIEditorIcons.Icon.Palette, color, 1.5f);
        GUI.Label(new Rect(rect.x + 33, rect.y, rect.width - 49, rect.height),
            ZMUIEditorTheme.CurrentPresetName,
            new GUIStyle(ZMUIEditorTheme.Body)
            {
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = color }
            });
        GUI.Label(new Rect(rect.xMax - 20, rect.y, 12, rect.height), "▾",
            new GUIStyle(ZMUIEditorTheme.Body)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = color }
            });

        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
        if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            ShowThemeMenu(rect);
        right = rect.x - 10;
    }

    private void ShowThemeMenu(Rect anchor)
    {
        var menu = new GenericMenu();
        foreach (ZMUIEditorTheme.Preset preset in Enum.GetValues(typeof(ZMUIEditorTheme.Preset)))
        {
            ZMUIEditorTheme.Preset captured = preset;
            menu.AddItem(
                new GUIContent(ZMUIEditorTheme.PresetName(preset)),
                ZMUIEditorTheme.CurrentPreset == preset,
                () =>
                {
                    ZMUIEditorTheme.SetPreset(captured);
                    Repaint();
                });
        }
        menu.DropDown(anchor);
    }

    private static void DrawHeaderChip(ref float right, float width, string text, ZMUIEditorIcons.Icon icon, Color color)
    {
        Rect rect = new Rect(right - width, 19, width, 34);
        Color previous = GUI.color;
        GUI.color = Color.Lerp(Color.white, color, .25f);
        GUI.Box(rect, GUIContent.none, ZMUIEditorTheme.StatusChip);
        GUI.color = previous;
        ZMUIEditorIcons.Draw(new Rect(rect.x + 11, rect.y + 9, 16, 16), icon, color, 1.5f);
        GUI.Label(new Rect(rect.x + 33, rect.y, rect.width - 39, rect.height), text,
            new GUIStyle(ZMUIEditorTheme.Body) { alignment = TextAnchor.MiddleLeft, normal = { textColor = color } });
        right = rect.x - 10;
    }

    private void DrawSidebar()
    {
        Rect sidebar = new Rect(0, HeaderHeight, SidebarWidth, position.height - HeaderHeight);
        EditorGUI.DrawRect(sidebar, ZMUIEditorTheme.Sidebar);
        EditorGUI.DrawRect(new Rect(sidebar.xMax - 1, sidebar.y, 1, sidebar.height), ZMUIEditorTheme.Border);

        for (int i = 0; i < Navigation.Length; i++)
        {
            Rect row = new Rect(12, HeaderHeight + 16 + i * (NavigationHeight + 8), SidebarWidth - 24, NavigationHeight);
            bool selected = selectedPage == (Page)i;
            bool hovered = row.Contains(Event.current.mousePosition);
            if (selected)
            {
                GUI.Box(row, GUIContent.none, ZMUIEditorTheme.ModeCardSelected);
                EditorGUI.DrawRect(new Rect(row.x, row.y + 8, 3, row.height - 16), ZMUIEditorTheme.Accent);
            }
            else if (hovered)
            {
                EditorGUI.DrawRect(row, new Color(1, 1, 1, .035f));
                Repaint();
            }

            ZMUIEditorIcons.Draw(new Rect(row.x + 16, row.y + 15, 22, 22), Navigation[i].Icon,
                selected ? new Color32(145, 157, 255, 255) : new Color32(170, 177, 189, 255));
            GUI.Label(row, Navigation[i].Label, selected ? ZMUIEditorTheme.NavItemSelected : ZMUIEditorTheme.NavItem);
            if (GUI.Button(row, GUIContent.none, GUIStyle.none))
            {
                selectedPage = (Page)i;
                scrollPosition = Vector2.zero;
                GUI.FocusControl(null);
            }
        }

        GUI.Label(new Rect(18, position.height - 48, SidebarWidth - 36, 24),
            "ZMteacher · ZMUI Framework",
            new GUIStyle(ZMUIEditorTheme.Hint) { alignment = TextAnchor.MiddleCenter, fontSize = 10 });
    }

    private void DrawContent()
    {
        Rect content = new Rect(SidebarWidth + 1, HeaderHeight, position.width - SidebarWidth - 1, position.height - HeaderHeight);
        GUILayout.BeginArea(content);
        using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar))
        {
            scrollPosition = scroll.scrollPosition;
            GUILayout.Space(ContentPadding);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(ContentPadding);
                using (new EditorGUILayout.VerticalScope())
                {
                    switch (selectedPage)
                    {
                        case Page.Mask: DrawMaskPage(); break;
                        case Page.CodeGenerator: DrawCodeGeneratorPage(); break;
                        case Page.ScriptPaths: DrawScriptPathsPage(); break;
                        case Page.PrefabPaths: DrawPrefabPathsPage(); break;
                        case Page.Namespaces: DrawNamespacesPage(); break;
                        case Page.Manual: DrawManualPage(); break;
                    }
                    GUILayout.Space(24);
                }
                GUILayout.Space(ContentPadding);
            }
        }
        GUILayout.EndArea();
    }

    private static void DrawPageTitle(string title, string subtitle)
    {
        GUILayout.Label(title, ZMUIEditorTheme.PageTitle, GUILayout.Height(31));
        GUILayout.Label(subtitle, ZMUIEditorTheme.PageSubtitle, GUILayout.Height(20));
        GUILayout.Space(14);
    }

    private static void BeginCard(string title, string subtitle = null)
    {
        EditorGUILayout.BeginVertical(ZMUIEditorTheme.CardBox);
        GUILayout.Label(title, ZMUIEditorTheme.CardTitle, GUILayout.Height(24));
        if (!string.IsNullOrEmpty(subtitle))
            GUILayout.Label(subtitle, ZMUIEditorTheme.Hint, GUILayout.Height(18));
        GUILayout.Space(9);
    }

    private static void EndCard(float gap = 10)
    {
        EditorGUILayout.EndVertical();
        GUILayout.Space(gap);
    }

    private static void DrawManualPage()
    {
        using (new EditorGUILayout.HorizontalScope(GUILayout.Height(54)))
        {
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Label("使用手册", ZMUIEditorTheme.PageTitle, GUILayout.Height(31));
                GUILayout.Label("ZMUI 配置、组件解析与脚本生成工作流程",
                    ZMUIEditorTheme.PageSubtitle, GUILayout.Height(20));
            }
            GUILayout.FlexibleSpace();
            GUILayout.Space(12);
            if (GUILayout.Button("打开 API 文档", ZMUIEditorTheme.PrimaryButton,
                    GUILayout.Width(146), GUILayout.Height(38)))
                Application.OpenURL(ApiDocumentationUrl);
        }
        GUILayout.Space(14);

        BeginCard("快速开始", "推荐首次接入时按以下顺序完成");
        using (new EditorGUILayout.HorizontalScope())
        {
            DrawManualStep("01", "选择策略", "配置遮罩、解析与绑定模式");
            GUILayout.Space(10);
            DrawManualStep("02", "设置目录", "确认四类脚本与 Prefab 路径");
            GUILayout.Space(10);
            DrawManualStep("03", "标记节点", "按名称或 Tag 声明 UI 组件");
            GUILayout.Space(10);
            DrawManualStep("04", "生成代码", "预览并写入目标脚本");
        }
        EndCard(12);

        using (new EditorGUILayout.HorizontalScope())
        {
            DrawManualCard("配置中心",
                "• 遮罩策略：推荐使用叠遮模式。\n\n" +
                "• 组件解析：默认推荐名称解析。\n\n" +
                "• 组件绑定：Bind 生成数据组件，Find 生成查找代码。\n\n" +
                "• 修改配置后点击底部保存，确保 UISetting 持久化。");
            GUILayout.Space(12);
            DrawManualCard("脚本生成快捷键",
                "Shift + B  生成组件数据脚本\n\n" +
                "Shift + U  生成组件查找脚本\n\n" +
                "Shift + V  生成 Window 表现层脚本\n\n" +
                "Shift + I  生成 Item 脚本");
        }
        GUILayout.Space(12);
        using (new EditorGUILayout.HorizontalScope())
        {
            DrawManualCard("推荐生成流程",
                "1. 在 Hierarchy 中选择窗口根节点。\n\n" +
                "2. 按当前解析策略标记需要绑定的子节点。\n\n" +
                "3. 先生成组件数据或组件查找脚本。\n\n" +
                "4. 再生成 Window 脚本并确认 UI 事件。\n\n" +
                "5. 在预览窗口检查代码和输出路径后生成。");
            GUILayout.Space(12);
            DrawManualCard("目录与更新规则",
                "• 所有生成脚本必须位于当前项目 Assets 目录内。\n\n" +
                "• 选择新目录后会保留原脚本文件名。\n\n" +
                "• 已有 Window 只插入缺失的字段和事件方法。\n\n" +
                "• 组件数据脚本会按最新路径执行自动挂载。\n\n" +
                "• 建议将自动生成文件纳入版本控制。");
        }
    }

    private static void DrawManualStep(string number, string title, string description)
    {
        using (new EditorGUILayout.VerticalScope(ZMUIEditorTheme.InfoBox,
                   GUILayout.Height(82), GUILayout.ExpandWidth(true)))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(8);
                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Label(number, new GUIStyle(ZMUIEditorTheme.CardTitle)
                    {
                        normal = { textColor = ZMUIEditorTheme.Accent }
                    }, GUILayout.Height(20));
                    GUILayout.Label(title, ZMUIEditorTheme.Label, GUILayout.Height(20));
                    GUILayout.Label(description, ZMUIEditorTheme.Hint, GUILayout.Height(28));
                }
                GUILayout.Space(4);
            }
        }
    }

    private static void DrawManualCard(string title, string content)
    {
        using (new EditorGUILayout.VerticalScope(ZMUIEditorTheme.CardBox,
                   GUILayout.MinHeight(210), GUILayout.ExpandWidth(true)))
        {
            GUILayout.Label(title, ZMUIEditorTheme.CardTitle, GUILayout.Height(24));
            GUILayout.Space(8);
            GUILayout.Label(content, ZMUIEditorTheme.Body, GUILayout.ExpandHeight(true));
        }
    }

    private void DrawMaskPage()
    {
        DrawPageTitle("窗口遮罩策略", "配置多窗口叠加时的遮罩处理方式");

        BeginCard("遮罩策略", "控制弹窗叠加时 Mask 的创建与透明度表现");
        Rect row = GUILayoutUtility.GetRect(0, 62, GUILayout.ExpandWidth(true));
        GUI.Label(new Rect(row.x, row.y + 5, row.width - 120, 24), "启用单遮模式", ZMUIEditorTheme.ModeTitle);
        GUI.Label(new Rect(row.x, row.y + 29, row.width - 120, 20), "多个窗口共用一个遮罩，视觉统一且减少层级开销", ZMUIEditorTheme.ModeHint);
        singleMask.boolValue = ZMUIEditorTheme.Toggle(new Rect(row.xMax - 52, row.y + 14, 48, 24), singleMask.boolValue);
        EndCard();

        BeginCard("策略说明");
        if (DrawStrategyCard("单遮模式", "多个窗口叠加时共用同一个 Mask，透明度唯一。",
                singleMask.boolValue, false))
            singleMask.boolValue = true;
        GUILayout.Space(8);
        if (DrawStrategyCard("叠遮模式", "每个窗口拥有独立 Mask，层次表现更自然，适合多数弹窗场景。",
                !singleMask.boolValue, true))
            singleMask.boolValue = false;
        EndCard();

        DrawSaveActions();
    }

    private static bool DrawStrategyCard(string title, string description, bool selected, bool recommended)
    {
        Rect rect = GUILayoutUtility.GetRect(0, 64, GUILayout.ExpandWidth(true));
        GUI.Box(rect, GUIContent.none, selected ? ZMUIEditorTheme.ModeCardSelected : ZMUIEditorTheme.ModeCard);
        GUI.Label(new Rect(rect.x + 16, rect.y + 9, rect.width - 120, 22), title, ZMUIEditorTheme.ModeTitle);
        if (recommended)
            GUI.Label(new Rect(rect.xMax - 78, rect.y + 10, 62, 22), "推荐", ZMUIEditorTheme.SuccessBadge);
        GUI.Label(new Rect(rect.x + 16, rect.y + 33, rect.width - 32, 20), description, ZMUIEditorTheme.ModeHint);
        return GUI.Button(rect, GUIContent.none, GUIStyle.none);
    }

    private void DrawCodeGeneratorPage()
    {
        DrawPageTitle("代码自动化生成", "配置组件解析方式与代码生成策略");

        BeginCard("组件解析方式", "框架如何从预制体节点中识别组件类型");
        parseType.enumValueIndex = DrawSegments(parseType.enumValueIndex, "名称解析", "Tag 标签解析", true);
        GUILayout.Space(10);
        Rect info = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
        GUI.Box(info, GUIContent.none, ZMUIEditorTheme.InfoBox);
        GUI.Label(info,
            parseType.enumValueIndex == (int)ParseType.Name
                ? "ⓘ   [Button]btnStart   →   Button btnStart"
                : "ⓘ   Tag: Button   →   Button button",
            ZMUIEditorTheme.InfoText);
        EndCard();

        BeginCard("代码生成方式");
        Rect modes = GUILayoutUtility.GetRect(0, 92, GUILayout.ExpandWidth(true));
        float gap = 14;
        float cardWidth = (modes.width - gap) * .5f;
        Rect findRect = new Rect(modes.x, modes.y, cardWidth, modes.height);
        Rect bindRect = new Rect(findRect.xMax + gap, modes.y, cardWidth, modes.height);
        DrawGeneratorMode(findRect, GeneratorType.Find, ZMUIEditorIcons.Icon.Search,
            "组件查找 Find", "运行时动态查找");
        DrawGeneratorMode(bindRect, GeneratorType.Bind, ZMUIEditorIcons.Icon.Link,
            "组件绑定 Bind", "Editor 直接绑定 · 零运行时查找");
        EndCard();

        DrawMappings();
        DrawSaveActions(true);
    }

    private int DrawSegments(int current, string first, string second, bool recommendFirst = false)
    {
        Rect rect = GUILayoutUtility.GetRect(0, 38, GUILayout.ExpandWidth(true));
        float half = (rect.width - 6) * .5f;
        Rect left = new Rect(rect.x, rect.y, half, rect.height);
        Rect right = new Rect(left.xMax + 6, rect.y, half, rect.height);
        if (GUI.Button(left, first, current == 0 ? ZMUIEditorTheme.SegmentSelected : ZMUIEditorTheme.Segment)) current = 0;
        if (GUI.Button(right, second, current == 1 ? ZMUIEditorTheme.SegmentSelected : ZMUIEditorTheme.Segment)) current = 1;
        if (recommendFirst)
            GUI.Label(new Rect(left.xMax - 68, left.y + 8, 56, 22), "推荐", ZMUIEditorTheme.SuccessBadge);
        return current;
    }

    private void DrawGeneratorMode(Rect rect, GeneratorType mode, ZMUIEditorIcons.Icon icon, string title, string description)
    {
        bool selected = generatorType.enumValueIndex == (int)mode;
        GUI.Box(rect, GUIContent.none, selected ? ZMUIEditorTheme.ModeCardSelected : ZMUIEditorTheme.ModeCard);
        Rect iconBox = new Rect(rect.x + 18, rect.y + 17, 48, 48);
        GUI.Box(iconBox, GUIContent.none, selected ? ZMUIEditorTheme.ModeIconBoxSelected : ZMUIEditorTheme.ModeIconBox);
        ZMUIEditorIcons.Draw(new Rect(iconBox.x + 9, iconBox.y + 9, 30, 30), icon,
            selected ? ZMUIEditorTheme.Accent : ZMUIEditorTheme.Muted, 2.2f);
        GUI.Label(new Rect(rect.x + 78, rect.y + 18, rect.width - 100, 24), title, ZMUIEditorTheme.ModeTitle);
        GUI.Label(new Rect(rect.x + 78, rect.y + 48, rect.width - 96, 20), description, ZMUIEditorTheme.ModeHint);
        if (mode == GeneratorType.Bind)
            GUI.Label(new Rect(rect.xMax - 74, rect.y + 15, 56, 22), "推荐", ZMUIEditorTheme.SuccessBadge);
        if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            generatorType.enumValueIndex = (int)mode;
    }

    private void DrawMappings()
    {
        EnsureDefaultMappings();
        string title = parseType.enumValueIndex == (int)ParseType.Name ? "组件映射预览" : "Tag 映射配置";
        EditorGUILayout.BeginVertical(ZMUIEditorTheme.CardBox);
        using (new EditorGUILayout.HorizontalScope(GUILayout.Height(24)))
        {
            GUILayout.Label(title, ZMUIEditorTheme.CardTitle, GUILayout.Height(24));
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{componentMappings.arraySize} 项映射", ZMUIEditorTheme.CountBadge,
                GUILayout.Width(104), GUILayout.Height(24));
        }
        GUILayout.Space(9);

        Rect table = GUILayoutUtility.GetRect(0, 34 + componentMappings.arraySize * 34, GUILayout.ExpandWidth(true));
        GUI.Box(table, GUIContent.none, ZMUIEditorTheme.TableBox);
        DrawTableHeader(new Rect(table.x + 14, table.y + 3, table.width - 28, 30));

        for (int i = 0; i < componentMappings.arraySize; i++)
        {
            SerializedProperty entry = componentMappings.GetArrayElementAtIndex(i);
            SerializedProperty key = entry.FindPropertyRelative("Key");
            string type = entry.FindPropertyRelative("ComponentType").stringValue;
            string example = ExampleNames.TryGetValue(type, out string field) ? field : "item";
            Rect row = new Rect(table.x + 12, table.y + 34 + i * 34, table.width - 24, 34);
            if (i % 2 == 0) EditorGUI.DrawRect(row, new Color(1, 1, 1, .025f));
            DrawMappingRow(row, key, type, example);
        }
        EditorGUILayout.EndVertical();
        GUILayout.Space(10);
    }

    private static void DrawTableHeader(Rect rect)
    {
        float first = rect.width * .34f;
        float second = rect.width * .34f;
        GUI.Label(new Rect(rect.x + 8, rect.y, first - 8, rect.height), "节点前缀", ZMUIEditorTheme.TableHeader);
        GUI.Label(new Rect(rect.x + first, rect.y, second, rect.height), "组件类型", ZMUIEditorTheme.TableHeader);
        GUI.Label(new Rect(rect.x + first + second, rect.y, rect.width - first - second, rect.height), "生成字段", ZMUIEditorTheme.TableHeader);
    }

    private void DrawMappingRow(Rect rect, SerializedProperty key, string componentType, string example)
    {
        float first = rect.width * .34f;
        float second = rect.width * .34f;
        bool nameMode = parseType.enumValueIndex == (int)ParseType.Name;
        if (nameMode)
        {
            GUI.Label(new Rect(rect.x + 8, rect.y, first - 12, rect.height),
                $"[{key.stringValue}]{example}", ZMUIEditorTheme.TableCell);
        }
        else
        {
            Rect field = new Rect(rect.x + 5, rect.y + 4, first - 14, 26);
            key.stringValue = ZMUIEditorTheme.TextField(field, key.stringValue, "请输入 Tag");
        }
        Rect componentIcon = new Rect(rect.x + first, rect.y + 9, 16, 16);
        ZMUIEditorIcons.DrawComponent(componentIcon, componentType);
        GUI.Label(new Rect(rect.x + first + 23, rect.y, second - 23, rect.height), componentType, ZMUIEditorTheme.TableCell);
        GUI.Label(new Rect(rect.x + first + second, rect.y, rect.width - first - second, rect.height),
            $"{example}{componentType}", ZMUIEditorTheme.CodeCell);
    }

    private void DrawScriptPathsPage()
    {
        DrawPageTitle("脚本生成路径", "配置各类自动生成脚本的输出目录");
        BeginCard("路径配置", "建议所有路径保持在 Assets 目录内，便于版本管理");
        DrawPathRow("组件绑定脚本", bindPath, "Assets/Scripts/AutoGenerate/BindComponent");
        if (generatorType.enumValueIndex == (int)GeneratorType.Find)
            DrawPathRow("组件查找脚本", findPath, "Assets/Scripts/AutoGenerate/FindComponent");
        DrawPathRow("窗口交互脚本", windowPath, "Assets/Scripts/AutoGenerate/Window");
        DrawPathRow("Item 脚本", itemPath, "Assets/Scripts/AutoGenerate/Item");
        EndCard();
        DrawSaveActions();
    }

    private static void DrawPathRow(string label, SerializedProperty property, string placeholder)
    {
        GUILayout.Label(label, ZMUIEditorTheme.Label, GUILayout.Height(20));
        Rect row = GUILayoutUtility.GetRect(0, 36, GUILayout.ExpandWidth(true));
        Rect field = new Rect(row.x, row.y, row.width - 44, 34);
        property.stringValue = ZMUIEditorTheme.TextField(field, property.stringValue, placeholder);
        Rect folder = new Rect(field.xMax + 8, row.y, 36, 34);
        if (GUI.Button(folder, EditorGUIUtility.IconContent("Folder Icon"), ZMUIEditorTheme.IconButton))
        {
            string selected = EditorUtility.OpenFolderPanel("选择目录", ToAbsolute(property.stringValue), string.Empty);
            if (!string.IsNullOrEmpty(selected)) property.stringValue = ToProjectPath(selected);
            GUI.FocusControl(null);
        }
        GUILayout.Space(8);
    }

    private void DrawPrefabPathsPage()
    {
        DrawPageTitle("窗口预制体路径", "框架会自动扫描以下目录中的 Prefab 作为窗口资源");
        BeginCard("预制体目录列表", "可配置多个业务模块目录，生成器会按顺序扫描");
        DrawArray(prefabPaths, true, "请选择预制体目录…", "添加预制体路径");
        EndCard();
        DrawSaveActions();
    }

    private void DrawNamespacesPage()
    {
        DrawPageTitle("命名空间配置", "生成脚本时自动在文件顶部引用以下命名空间");
        BeginCard("命名空间列表", "例如 UnityEngine.UI、TMPro 或项目业务命名空间");
        DrawArray(namespaces, false, "请输入命名空间…", "添加命名空间");
        EndCard();
        DrawSaveActions();
    }

    private static void DrawArray(SerializedProperty array, bool folderPicker, string placeholder, string addLabel)
    {
        int removeIndex = -1;
        for (int i = 0; i < array.arraySize; i++)
        {
            SerializedProperty element = array.GetArrayElementAtIndex(i);
            Rect row = GUILayoutUtility.GetRect(0, 42, GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(row.x, row.y, 28, 34), $"{i + 1:D2}",
                new GUIStyle(ZMUIEditorTheme.CodeCell) { alignment = TextAnchor.MiddleCenter });

            float buttonsWidth = folderPicker ? 84 : 42;
            Rect field = new Rect(row.x + 34, row.y, row.width - 34 - buttonsWidth, 34);
            element.stringValue = ZMUIEditorTheme.TextField(field, element.stringValue, placeholder);
            float x = field.xMax + 7;
            if (folderPicker)
            {
                if (GUI.Button(new Rect(x, row.y, 34, 34), EditorGUIUtility.IconContent("Folder Icon"), ZMUIEditorTheme.IconButton))
                {
                    string selected = EditorUtility.OpenFolderPanel("选择目录", ToAbsolute(element.stringValue), string.Empty);
                    if (!string.IsNullOrEmpty(selected)) element.stringValue = ToProjectPath(selected);
                    GUI.FocusControl(null);
                }
                x += 41;
            }
            if (GUI.Button(new Rect(x, row.y, 34, 34), "×", ZMUIEditorTheme.DeleteButton))
                removeIndex = i;
        }

        if (removeIndex >= 0) array.DeleteArrayElementAtIndex(removeIndex);
        GUILayout.Space(4);
        if (GUILayout.Button("+  " + addLabel, ZMUIEditorTheme.SecondaryButton, GUILayout.Height(38)))
        {
            array.InsertArrayElementAtIndex(array.arraySize);
            array.GetArrayElementAtIndex(array.arraySize - 1).stringValue = string.Empty;
        }
    }

    private void DrawSaveActions(bool showReset = false)
    {
        GUILayout.Space(4);
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (showReset && GUILayout.Button("恢复默认", ZMUIEditorTheme.SecondaryButton, GUILayout.Width(140), GUILayout.Height(38)))
                ResetMappings();
            if (showReset) GUILayout.Space(10);
            if (GUILayout.Button("保存配置", ZMUIEditorTheme.PrimaryButton, GUILayout.Width(160), GUILayout.Height(38)))
                SaveSetting(true);
        }
    }

    private void EnsureDefaultMappings()
    {
        if (componentMappings.arraySize > 0) return;
        foreach (ComponentMapping mapping in GeneratorConfig.DefaultMappings)
        {
            componentMappings.InsertArrayElementAtIndex(componentMappings.arraySize);
            SerializedProperty entry = componentMappings.GetArrayElementAtIndex(componentMappings.arraySize - 1);
            entry.FindPropertyRelative("Key").stringValue = mapping.Key;
            entry.FindPropertyRelative("ComponentType").stringValue = mapping.ComponentType;
        }
    }

    private void ResetMappings()
    {
        componentMappings.ClearArray();
        EnsureDefaultMappings();
        GUI.FocusControl(null);
    }

    private void SaveSetting(bool feedback)
    {
        if (setting == null || serializedSetting == null) return;
        serializedSetting.ApplyModifiedProperties();
        setting.Save();
        if (!feedback) return;
        savedFeedbackUntil = EditorApplication.timeSinceStartup + 1.5;
        ShowNotification(new GUIContent("ZMUI 配置已保存"));
        Repaint();
    }

    private static string ToAbsolute(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return Application.dataPath;
        if (System.IO.Path.IsPathRooted(path)) return path;
        string projectRoot = System.IO.Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
        return System.IO.Path.GetFullPath(System.IO.Path.Combine(projectRoot, path));
    }

    private static string ToProjectPath(string absolute)
    {
        string normalized = absolute.Replace('\\', '/');
        string dataPath = Application.dataPath.Replace('\\', '/');
        return normalized.StartsWith(dataPath, StringComparison.OrdinalIgnoreCase)
            ? "Assets" + normalized.Substring(dataPath.Length)
            : normalized;
    }
}
