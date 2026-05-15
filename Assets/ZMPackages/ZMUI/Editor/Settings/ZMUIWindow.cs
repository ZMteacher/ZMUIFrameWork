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
using System;
using UnityEditor;
using UnityEngine;

public class ZMUIWindow : EditorWindow
{
    // ── Serialized Data ───────────────────────────────────────────────────────
    private UISetting        mSetting;
    private SerializedObject mSerializedObj;

    private SerializedProperty mSingMaskProp;
    private SerializedProperty mParseTypeProp;
    private SerializedProperty mGeneratorTypeProp;
    private SerializedProperty mBindPathProp;
    private SerializedProperty mFindPathProp;
    private SerializedProperty mWindowPathProp;
    private SerializedProperty mItemPathProp;
    private SerializedProperty mPrefabFolderArrProp;
    private SerializedProperty mNamespaceArrProp;
    private SerializedProperty mComponentMappingsProp;

    // ── Layout ────────────────────────────────────────────────────────────────
    private const float kHeaderH   = 52f;
    private const float kSidebarW  = 168f;
    private const float kItemH     = 44f;
    private const float kPadding   = 16f;

    private int       mSelectedPage;
    private Vector2   mContentScroll;

    // ── Textures (released on destroy) ───────────────────────────────────────
    private Texture2D mTexAccent;
    private Texture2D mTexAccentHover;
    private Texture2D mTexSelected;
    private Texture2D mTexHover;
    private Texture2D mTexCard;
    private Texture2D mTexGreen;
    private Texture2D mTexGreenHover;
    private Texture2D mTexRed;
    private Texture2D mTexDivider;
    private Texture2D mTexTagBadge;

    // ── Styles ────────────────────────────────────────────────────────────────
    private GUIStyle mStyleMenuItem;
    private GUIStyle mStyleMenuItemSel;
    private GUIStyle mStylePageTitle;
    private GUIStyle mStylePageSub;
    private GUIStyle mStyleCard;
    private GUIStyle mStyleCardTitle;
    private GUIStyle mStyleFieldLabel;
    private GUIStyle mStyleHint;
    private GUIStyle mStyleTabLeft;
    private GUIStyle mStyleTabMid;
    private GUIStyle mStyleTabRight;
    private GUIStyle mStyleBtnBlue;
    private GUIStyle mStyleBtnGreen;
    private GUIStyle mStyleBtnRed;
    private GUIStyle mStyleBtnBrowse;
    private GUIStyle mStyleBtnFlat;     // 扁平添加按钮（同 TabBar 风格）
    private GUIStyle mStyleHeaderTitle;
    private GUIStyle mStyleBadge;
    private bool mStylesBuilt;

    // ── Colors ────────────────────────────────────────────────────────────────
    // Accent blue
    private static readonly Color kAccent      = new Color(0.243f, 0.522f, 0.957f);
    private static readonly Color kAccentHover = new Color(0.337f, 0.592f, 0.976f);
    // Dynamic (skin-aware)
    private Color CHeader   => EditorGUIUtility.isProSkin ? new Color(0.137f, 0.137f, 0.137f) : new Color(0.22f,  0.22f,  0.22f);
    private Color CSidebar  => EditorGUIUtility.isProSkin ? new Color(0.160f, 0.160f, 0.160f) : new Color(0.82f,  0.82f,  0.82f);
    private Color CContent  => EditorGUIUtility.isProSkin ? new Color(0.215f, 0.215f, 0.215f) : new Color(0.925f, 0.925f, 0.925f);
    private Color CDivider  => EditorGUIUtility.isProSkin ? new Color(0.098f, 0.098f, 0.098f) : new Color(0.65f,  0.65f,  0.65f);
    private Color CCard     => EditorGUIUtility.isProSkin ? new Color(0.247f, 0.247f, 0.247f) : new Color(0.975f, 0.975f, 0.975f);
    private Color CHover    => EditorGUIUtility.isProSkin ? new Color(0.270f, 0.270f, 0.270f) : new Color(0.76f,  0.76f,  0.76f);
    private Color CTextPri  => EditorGUIUtility.isProSkin ? new Color(0.88f,  0.88f,  0.88f)  : new Color(0.10f,  0.10f,  0.10f);
    private Color CTextSec  => EditorGUIUtility.isProSkin ? new Color(0.55f,  0.55f,  0.55f)  : new Color(0.42f,  0.42f,  0.42f);

    // ── Menu Definition ───────────────────────────────────────────────────────
    // 使用手绘彩色徽章代替 Unity 内置图标，保证在所有版本中显示一致、美观
    private static readonly (string badge, string label, string sub, Color color)[] kPages =
    {
        ("M", "遮罩模式",   "Mask System",    new Color(0.98f, 0.62f, 0.22f)),  // 橙色
        ("C", "代码生成",   "Code Generator", new Color(0.27f, 0.68f, 0.98f)),  // 天蓝
        ("P", "生成路径",   "Script Paths",   new Color(0.28f, 0.82f, 0.52f)),  // 绿色
        ("F", "预制体路径", "Prefab Paths",   new Color(0.78f, 0.42f, 0.98f)),  // 紫色
        ("N", "命名空间",   "Namespaces",     new Color(0.98f, 0.40f, 0.58f)),  // 玫红
    };

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("ZM/ZMUI Setting", false, 2)]
    public static void Open()
    {
        var w = GetWindow<ZMUIWindow>(false, "ZMUI Setting");
        w.minSize = new Vector2(740, 540);
        w.Show();
    }

    private void OnEnable()
    {
        mStylesBuilt = false;
        mSetting = UISetting.Instance;
        if (mSetting == null) return;
        mSerializedObj = new SerializedObject(mSetting);
        CacheProps();
    }

    private void OnDisable() => mSetting?.Save();

    private void OnDestroy()
    {
        DestroyTex(ref mTexAccent);      DestroyTex(ref mTexAccentHover);
        DestroyTex(ref mTexSelected);    DestroyTex(ref mTexHover);
        DestroyTex(ref mTexCard);        DestroyTex(ref mTexGreen);
        DestroyTex(ref mTexGreenHover);  DestroyTex(ref mTexRed);
        DestroyTex(ref mTexDivider);     DestroyTex(ref mTexTagBadge);
    }

    private void CacheProps()
    {
        mSingMaskProp        = mSerializedObj.FindProperty("SINGMASK_SYSTEM");
        mParseTypeProp       = mSerializedObj.FindProperty("ParseType");
        mGeneratorTypeProp   = mSerializedObj.FindProperty("GeneratorType");
        mBindPathProp        = mSerializedObj.FindProperty("BindComponentGeneratorPath");
        mFindPathProp        = mSerializedObj.FindProperty("FindComponentGeneratorPath");
        mWindowPathProp      = mSerializedObj.FindProperty("WindowGeneratorPath");
        mItemPathProp        = mSerializedObj.FindProperty("ItemScriptsGeneratorPath");
        mPrefabFolderArrProp = mSerializedObj.FindProperty("WindowPrefabFolderPathArr");
        mNamespaceArrProp    = mSerializedObj.FindProperty("UsingNameSpaceArr");
        mComponentMappingsProp = mSerializedObj.FindProperty("ComponentMappings");
    }

    // ── Style Builder ─────────────────────────────────────────────────────────
    private void EnsureStyles()
    {
        if (mStylesBuilt) return;
        mStylesBuilt = true;

        mTexAccent      = Tex(kAccent);
        mTexAccentHover = Tex(kAccentHover);
        mTexSelected    = Tex(kAccent);
        mTexHover       = Tex(CHover);
        mTexCard        = Tex(CCard);
        mTexGreen       = Tex(new Color(0.18f, 0.72f, 0.44f));
        mTexGreenHover  = Tex(new Color(0.22f, 0.82f, 0.50f));
        mTexRed         = Tex(new Color(0.82f, 0.22f, 0.22f));
        mTexDivider     = Tex(CDivider);
        mTexTagBadge    = Tex(new Color(0.28f, 0.52f, 0.88f, 0.4f));

        // Sidebar item (normal)
        mStyleMenuItem = new GUIStyle
        {
            normal    = { textColor = CTextPri },
            hover     = { background = mTexHover, textColor = CTextPri },
            padding   = new RectOffset(14, 8, 0, 0),
            alignment = TextAnchor.MiddleLeft,
            fontSize  = 12,
        };

        // Sidebar item (selected)
        mStyleMenuItemSel = new GUIStyle(mStyleMenuItem)
        {
            normal    = { background = mTexSelected, textColor = Color.white },
            hover     = { background = mTexAccentHover, textColor = Color.white },
            fontStyle = FontStyle.Bold,
        };

        // Page title
        mStylePageTitle = new GUIStyle
        {
            normal    = { textColor = CTextPri },
            fontSize  = 16,
            fontStyle = FontStyle.Bold,
            margin    = new RectOffset(0, 0, 0, 0),
        };
        mStylePageSub = new GUIStyle
        {
            normal    = { textColor = CTextSec },
            fontSize  = 11,
            margin    = new RectOffset(0, 0, 2, 0),
        };

        // Card
        mStyleCard = new GUIStyle
        {
            normal  = { background = mTexCard },
            padding = new RectOffset(14, 14, 12, 12),
            margin  = new RectOffset(0, 0, 0, 10),
            border  = new RectOffset(2, 2, 2, 2),
        };
        mStyleCardTitle = new GUIStyle
        {
            normal    = { textColor = CTextSec },
            fontSize  = 10,
            fontStyle = FontStyle.Bold,
            margin    = new RectOffset(0, 0, 0, 6),
        };

        // Field label
        mStyleFieldLabel = new GUIStyle
        {
            normal  = { textColor = CTextSec },
            fontSize = 11,
            margin  = new RectOffset(0, 0, 0, 3),
        };

        // Hint text
        mStyleHint = new GUIStyle
        {
            normal   = { textColor = CTextSec },
            fontSize = 10,
            wordWrap = true,
            margin   = new RectOffset(0, 0, 4, 0),
        };

        // Tab bar styles
        mStyleTabLeft  = new GUIStyle(EditorStyles.miniButtonLeft)  { fontSize = 11, fixedHeight = 26 };
        mStyleTabMid   = new GUIStyle(EditorStyles.miniButtonMid)   { fontSize = 11, fixedHeight = 26 };
        mStyleTabRight = new GUIStyle(EditorStyles.miniButtonRight) { fontSize = 11, fixedHeight = 26 };

        // Blue button
        mStyleBtnBlue = new GUIStyle(EditorStyles.miniButton)
        {
            normal    = { background = mTexAccent,      textColor = Color.white },
            hover     = { background = mTexAccentHover, textColor = Color.white },
            active    = { background = mTexAccent,      textColor = Color.white },
            fontStyle = FontStyle.Bold,
            fontSize  = 11,
            padding   = new RectOffset(10, 10, 4, 4),
        };

        // Green button
        mStyleBtnGreen = new GUIStyle(EditorStyles.miniButton)
        {
            normal    = { background = mTexGreen,      textColor = Color.white },
            hover     = { background = mTexGreenHover, textColor = Color.white },
            active    = { background = mTexGreen,      textColor = Color.white },
            fontStyle = FontStyle.Bold,
            fontSize  = 11,
            padding   = new RectOffset(12, 12, 5, 5),
        };

        // Red button (delete)
        mStyleBtnRed = new GUIStyle(EditorStyles.miniButton)
        {
            normal    = { background = mTexRed, textColor = Color.white },
            hover     = { background = mTexRed, textColor = Color.white },
            active    = { background = mTexRed, textColor = Color.white },
            fontStyle = FontStyle.Bold,
            fontSize  = 12,
            padding   = new RectOffset(4, 4, 3, 3),
        };

        // Browse button
        mStyleBtnBrowse = new GUIStyle(EditorStyles.miniButton)
        {
            normal  = { textColor = kAccent },
            hover   = { textColor = kAccentHover },
            fontSize = 11,
        };

        // Flat add/save button（同 TabBar 扁平风格，按下变蓝反馈）
        mStyleBtnFlat = new GUIStyle(EditorStyles.miniButton)
        {
            fontSize    = 11,
            fontStyle   = FontStyle.Normal,
            active      = { background = mTexAccent, textColor = Color.white },
        };

        // Header title
        mStyleHeaderTitle = new GUIStyle
        {
            normal    = { textColor = Color.white },
            fontSize  = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
        };

        // Badge
        mStyleBadge = new GUIStyle
        {
            normal    = { background = mTexTagBadge, textColor = new Color(0.7f, 0.85f, 1f) },
            fontSize  = 10,
            alignment = TextAnchor.MiddleCenter,
            padding   = new RectOffset(6, 6, 2, 2),
            border    = new RectOffset(3, 3, 3, 3),
        };
    }

    // ── OnGUI ─────────────────────────────────────────────────────────────────
    private void OnGUI()
    {
        EnsureStyles();

        if (mSetting == null || mSerializedObj == null)
        {
            EditorGUILayout.HelpBox("未找到 UISetting.asset，请在 Resources 目录下创建。", MessageType.Error);
            return;
        }

        mSerializedObj.Update();

        float w = position.width;
        float h = position.height;

        // ── Background planes ─────────────────────────────────────────────
        EditorGUI.DrawRect(new Rect(0,          0,       w,               kHeaderH), CHeader);
        EditorGUI.DrawRect(new Rect(0,          kHeaderH, kSidebarW,      h - kHeaderH), CSidebar);
        EditorGUI.DrawRect(new Rect(kSidebarW,  kHeaderH, w - kSidebarW,  h - kHeaderH), CContent);

        // Dividers
        EditorGUI.DrawRect(new Rect(0,         kHeaderH - 1,  w,          1), CDivider);
        EditorGUI.DrawRect(new Rect(kSidebarW, kHeaderH,      1,          h - kHeaderH), CDivider);

        // ── Panels ────────────────────────────────────────────────────────
        DrawHeader(w);
        DrawSidebar(h);
        DrawContentArea(w, h);

        if (mSerializedObj.ApplyModifiedProperties())
            mSetting.Save();
    }

    // ── Header ────────────────────────────────────────────────────────────────
    private void DrawHeader(float w)
    {
        GUILayout.BeginArea(new Rect(0, 0, w, kHeaderH));
        GUILayout.BeginHorizontal();
        GUILayout.Space(16);

        // Accent bar decoration
        var barRect = new Rect(14, (kHeaderH - 30) * 0.5f, 4, 30);
        EditorGUI.DrawRect(barRect, kAccent);
        GUILayout.Space(14);

        // Title
        GUILayout.Label("ZMUI Framework", mStyleHeaderTitle,
            GUILayout.Height(kHeaderH), GUILayout.ExpandWidth(false));
        GUILayout.Space(8);

        // Version badge
        GUILayout.Label("v 1.0.0", mStyleBadge,
            GUILayout.Height(18), GUILayout.Width(50));

        GUILayout.FlexibleSpace();

        // Save button（垂直居中）
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("  ✓  保存设置", mStyleBtnFlat,
            GUILayout.Height(26), GUILayout.Width(90)))
            mSetting.Save();
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        GUILayout.Space(14);
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    // ── Sidebar ───────────────────────────────────────────────────────────────
    private void DrawSidebar(float h)
    {
        GUILayout.BeginArea(new Rect(0, kHeaderH, kSidebarW, h - kHeaderH));
        GUILayout.Space(10);

        for (int i = 0; i < kPages.Length; i++)
        {
            bool sel = mSelectedPage == i;
            var (badge, label, sub, color) = kPages[i];

            Rect rowRect = new Rect(0, i * kItemH + 10, kSidebarW, kItemH);
            bool hover = rowRect.Contains(Event.current.mousePosition);

            // ── 选中状态：渐变卡片背景 + 左侧竖条 ──────────────────────────
            if (Event.current.type == EventType.Repaint)
            {
                if (sel)
                {
                    // 水平渐变：左侧实色 → 右侧透明（32片叠加模拟渐变）
                    const int kSlices = 32;
                    float sliceW = rowRect.width / kSlices + 1f;
                    for (int s = 0; s < kSlices; s++)
                    {
                        float t     = 1f - (float)s / kSlices;
                        float alpha = t * 0.28f;
                        EditorGUI.DrawRect(
                            new Rect(rowRect.x + s * (rowRect.width / kSlices), rowRect.y, sliceW, rowRect.height),
                            new Color(color.r, color.g, color.b, alpha));
                    }
                    // 左侧 3px 实色竖条
                    EditorGUI.DrawRect(new Rect(rowRect.x, rowRect.y, 3f, rowRect.height), color);
                }
                else if (hover)
                {
                    EditorGUI.DrawRect(rowRect, new Color(color.r, color.g, color.b, 0.08f));
                }
            }

            // ── 点击响应 ────────────────────────────────────────────────────
            if (GUI.Button(rowRect, GUIContent.none, GUIStyle.none))
                mSelectedPage = i;

            // ── 彩色徽章 + 双行文字 ────────────────────────────────────────
            if (Event.current.type == EventType.Repaint)
            {
                const float kBadgeSize = 22f;
                float badgeX = rowRect.x + 16f;
                float badgeY = rowRect.y + (kItemH - kBadgeSize) * 0.5f;
                var badgeRect = new Rect(badgeX, badgeY, kBadgeSize, kBadgeSize);
                Color badgeColor = sel ? color : Color.Lerp(color, CSidebar, 0.18f);

                EditorGUI.DrawRect(
                    new Rect(badgeRect.x + 1f, badgeRect.y + 1f, badgeRect.width, badgeRect.height),
                    new Color(0f, 0f, 0f, EditorGUIUtility.isProSkin ? 0.16f : 0.10f));
                EditorGUI.DrawRect(badgeRect, badgeColor);

                var badgeStyle = new GUIStyle
                {
                    normal    = { textColor = Color.white },
                    fontSize  = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    clipping  = TextClipping.Clip,
                };
                GUI.Label(badgeRect, badge, badgeStyle);

                float textX = badgeRect.xMax + 10f;
                float textW = rowRect.xMax - textX - 10f;
                var labelRect = new Rect(textX, rowRect.y + 6f, textW, 14f);
                var subRect   = new Rect(textX, rowRect.y + 21f, textW, 12f);

                var labelStyle = new GUIStyle
                {
                    normal    = { textColor = sel ? Color.white : CTextPri },
                    fontSize  = 12,
                    fontStyle = sel ? FontStyle.Bold : FontStyle.Normal,
                    alignment = TextAnchor.MiddleLeft,
                    clipping  = TextClipping.Clip,
                };
                var subStyle = new GUIStyle
                {
                    normal    = { textColor = sel ? new Color(1f, 1f, 1f, 0.72f) : CTextSec },
                    fontSize  = 9,
                    alignment = TextAnchor.MiddleLeft,
                    clipping  = TextClipping.Clip,
                };

                GUI.Label(labelRect, label, labelStyle);
                GUI.Label(subRect, sub, subStyle);
            }
        }

        // 底部版权文字
        GUILayout.FlexibleSpace();
        var linkStyle = new GUIStyle
        {
            normal    = { textColor = new Color(0.4f, 0.4f, 0.4f) },
            fontSize  = 9,
            alignment = TextAnchor.MiddleCenter,
        };
        GUILayout.Label("ZMteacher / ZMUIFrameWork", linkStyle, GUILayout.Height(24));
        GUILayout.EndArea();
    }

    // ── Content Area ─────────────────────────────────────────────────────────
    private void DrawContentArea(float w, float h)
    {
        float cx = kSidebarW + 1;
        float cw = w - cx;

        GUILayout.BeginArea(new Rect(cx, kHeaderH, cw, h - kHeaderH));
        mContentScroll = GUILayout.BeginScrollView(mContentScroll, false, false,
            GUIStyle.none, GUI.skin.verticalScrollbar);

        GUILayout.Space(kPadding);
        GUILayout.BeginHorizontal();
        GUILayout.Space(kPadding);
        GUILayout.BeginVertical();

        switch (mSelectedPage)
        {
            case 0: DrawPageMask();      break;
            case 1: DrawPageCodeGen();   break;
            case 2: DrawPagePaths();     break;
            case 3: DrawPagePrefabs();   break;
            case 4: DrawPageNamespace(); break;
        }

        GUILayout.Space(kPadding * 2);
        GUILayout.EndVertical();
        GUILayout.Space(kPadding);
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    // ── Pages ─────────────────────────────────────────────────────────────────

    private void DrawPageMask()
    {
        PageTitle("窗口遮罩模式", "配置多窗口叠加时的遮罩处理策略");

        BeginCard("遮罩策略");
        bool val = mSingMaskProp.boolValue;
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("启用单遮模式", mStyleFieldLabel, GUILayout.Width(110));
        val = EditorGUILayout.Toggle(val, GUILayout.Width(18));
        mSingMaskProp.boolValue = val;
        GUILayout.Space(6);
        GUI.color = val ? new Color(0.25f, 0.85f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
        GUILayout.Label(val ? "● 已启用" : "○ 已禁用",
            new GUIStyle { normal = { textColor = GUI.color }, fontSize = 11, fontStyle = FontStyle.Bold },
            GUILayout.ExpandWidth(false));
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();
        EndCard();

        BeginCard("策略说明");
        GUILayout.Label("单遮模式（推荐）", new GUIStyle { normal = { textColor = kAccent }, fontSize = 11, fontStyle = FontStyle.Bold });
        GUILayout.Label("多个窗口叠加时共用同一个 Mask，透明度唯一，视觉更统一整洁。", mStyleHint);
        GUILayout.Space(8);
        GUILayout.Label("叠遮模式", new GUIStyle { normal = { textColor = CTextSec }, fontSize = 11, fontStyle = FontStyle.Bold });
        GUILayout.Label("每个窗口拥有独立的 Mask，透明度叠加，适用于需要多层独立遮罩的场景。", mStyleHint);
        EndCard();
    }

    private void DrawPageCodeGen()
    {
        PageTitle("代码自动化生成", "配置组件解析方式与代码生成策略");

        BeginCard("组件解析方式");
        GUILayout.Label("框架如何从预制体节点中识别组件类型", mStyleHint);
        GUILayout.Space(8);
        mParseTypeProp.enumValueIndex = TabBar(
            mParseTypeProp.enumValueIndex,
            new[] { "名称解析  [Button]field", "Tag 标签解析" });
        GUILayout.Space(6);
        string[] parseHints = {
            "通过节点名称前缀识别组件，格式：[ComponentType]fieldName\n示例：[Button]btnStart  →  生成 Button btnStart 字段",
            "通过节点右上角的 Tag 识别组件类型\n示例：节点 Tag 设为 Button  →  生成 Button 字段",
        };
        GUILayout.Label(parseHints[mParseTypeProp.enumValueIndex], mStyleHint);
        EndCard();

        BeginCard("代码生成方式");
        GUILayout.Label("生成的组件引用代码类型", mStyleHint);
        GUILayout.Space(8);
        mGeneratorTypeProp.enumValueIndex = TabBar(
            mGeneratorTypeProp.enumValueIndex,
            new[] { "组件查找 (Find)", "组件绑定 (Bind)" });
        GUILayout.Space(6);
        string[] genHints = {
            "运行时通过 transform.Find() 动态查找，兼容性强，适合频繁改动结构的项目。",
            "生成序列化字段并在 Editor 直接赋值，性能更优，无运行时查找开销。（推荐）",
        };
        GUILayout.Label(genHints[mGeneratorTypeProp.enumValueIndex], mStyleHint);
        EndCard();

        DrawTagMappingReadOnly(mParseTypeProp.enumValueIndex == 0);
    }

    // 组件映射关系只读展示（isNameMode=true 时 Key 加 [] 包裹）
    private void DrawTagMappingReadOnly(bool isNameMode)
    {
        // ── 公共样式 ─────────────────────────────────────────────────────────
        var headerStyle = new GUIStyle
        {
            normal    = { textColor = CTextSec },
            fontSize  = 10,
            fontStyle = FontStyle.Bold,
            padding   = new RectOffset(4, 0, 2, 4),
        };
        var arrowStyle = new GUIStyle
        {
            normal    = { textColor = CTextSec },
            fontSize  = 11,
            alignment = TextAnchor.MiddleCenter,
        };
        var idxStyle = new GUIStyle
        {
            normal    = { textColor = new Color(kAccent.r, kAccent.g, kAccent.b, 0.5f) },
            fontSize  = 10,
            alignment = TextAnchor.MiddleCenter,
        };
        var inputStyle = new GUIStyle(EditorStyles.label)
        {
            normal   = { textColor = CTextSec },
            fontSize = 11,
            padding  = new RectOffset(4, 0, 0, 0),
        };
        var codeKeyStyle = new GUIStyle(EditorStyles.label)
        {
            normal   = { textColor = CTextSec },
            fontSize = 11,
            padding  = new RectOffset(2, 0, 0, 0),
        };
        var codeTypeStyle = new GUIStyle(EditorStyles.label)
        {
            normal    = { textColor = kAccent },
            fontSize  = 11,
            fontStyle = FontStyle.Bold,
            padding   = new RectOffset(2, 0, 0, 0),
        };
        var codeFieldStyle = new GUIStyle(EditorStyles.label)
        {
            normal   = { textColor = CTextPri },
            fontSize = 11,
            padding  = new RectOffset(2, 0, 0, 0),
        };

        // 每种组件对应的示例字段名（按默认 ComponentType 匹配）
        var exampleNames = new System.Collections.Generic.Dictionary<string, string>
        {
            { "Text",          "title"    },
            { "Image",         "icon"     },
            { "RawImage",      "avatar"   },
            { "Button",        "confirm"  },
            { "InputField",    "input"    },
            { "Toggle",        "check"    },
            { "Slider",        "progress" },
            { "Scrollbar",     "scroll"   },
            { "Dropdown",      "option"   },
            { "Canvas",        "canvas"   },
            { "Panel",         "panel"    },
            { "ScrollRect",    "list"     },
            { "LoopListView2", "loop"     },
            { "Transform",     "node"     },
            { "RectTransform", "rect"     },
            { "GameObject",    "item"     },
        };

        if (isNameMode)
        {
            // ── 名称解析：只读展示 ────────────────────────────────────────────
            BeginCard("组件映射关系（名称解析）");
            GUILayout.Label("节点命名格式：[Key]fieldName，生成字段示例如下", mStyleHint);
            GUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("#",           headerStyle, GUILayout.Width(20));
            GUILayout.Label("节点命名",    headerStyle, GUILayout.Width(130));
            GUILayout.Label("",            headerStyle, GUILayout.Width(22));
            GUILayout.Label("生成字段",    headerStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            var divR = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(divR, CDivider);
            GUILayout.Space(4);

            var mappings = GeneratorConfig.GetMappings();
            for (int i = 0; i < mappings.Length; i++)
            {
                var rowRect = GUILayoutUtility.GetRect(0, 22, GUILayout.ExpandWidth(true));
                if (i % 2 == 0)
                    EditorGUI.DrawRect(rowRect, new Color(kAccent.r, kAccent.g, kAccent.b, 0.04f));

                string key      = mappings[i].Key;
                string compType = mappings[i].ComponentType;
                string example  = exampleNames.TryGetValue(compType, out var n) ? n : "name";
                string inputTxt = $"[{key}]{example}";
                string field    = $"{example}{compType}";

                float x = rowRect.x;
                GUI.Label(new Rect(x, rowRect.y, 20,  rowRect.height), $"{i + 1}", idxStyle);   x += 20;
                GUI.Label(new Rect(x, rowRect.y, 130, rowRect.height), inputTxt,   inputStyle); x += 130;
                GUI.Label(new Rect(x, rowRect.y, 22,  rowRect.height), "→",        arrowStyle); x += 22;
                float kwW   = 44f;
                GUI.Label(new Rect(x, rowRect.y, kwW, rowRect.height), "public", codeKeyStyle); x += kwW;
                float typeW = GUI.skin.label.CalcSize(new GUIContent(compType)).x + 6f;
                GUI.Label(new Rect(x, rowRect.y, typeW, rowRect.height), compType, codeTypeStyle); x += typeW;
                GUI.Label(new Rect(x, rowRect.y, rowRect.width - x + rowRect.x, rowRect.height), field, codeFieldStyle);
            }
            GUILayout.Space(4);
            EndCard();
        }
        else
        {
            // ── Tag 解析：Key 可编辑，ComponentType 只读 ───────────────────────
            // 若映射表为空，先用默认值填充
            if (mComponentMappingsProp.arraySize == 0)
            {
                foreach (var def in GeneratorConfig.DefaultMappings)
                {
                    mComponentMappingsProp.InsertArrayElementAtIndex(mComponentMappingsProp.arraySize);
                    var e = mComponentMappingsProp.GetArrayElementAtIndex(mComponentMappingsProp.arraySize - 1);
                    e.FindPropertyRelative("Key").stringValue           = def.Key;
                    e.FindPropertyRelative("ComponentType").stringValue = def.ComponentType;
                }
            }

            BeginCard("Tag 映射配置");
            GUILayout.Label("配置每种组件类型对应的 Tag 名，节点 Tag 设为该值即可被框架识别", mStyleHint);
            GUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("#",               headerStyle, GUILayout.Width(20));
            GUILayout.Label("Tag 名（可编辑）", headerStyle, GUILayout.Width(120));
            GUILayout.Label("",                headerStyle, GUILayout.Width(22));
            GUILayout.Label("生成字段预览",    headerStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            var divR2 = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(divR2, CDivider);
            GUILayout.Space(4);

            for (int i = 0; i < mComponentMappingsProp.arraySize; i++)
            {
                var entry    = mComponentMappingsProp.GetArrayElementAtIndex(i);
                var keyProp  = entry.FindPropertyRelative("Key");
                var typeProp = entry.FindPropertyRelative("ComponentType");

                var rowRect = GUILayoutUtility.GetRect(0, 22, GUILayout.ExpandWidth(true));
                if (i % 2 == 0)
                    EditorGUI.DrawRect(rowRect, new Color(kAccent.r, kAccent.g, kAccent.b, 0.04f));

                string compType = typeProp.stringValue;
                string example  = exampleNames.TryGetValue(compType, out var n2) ? n2 : "name";
                string field    = $"{example}{compType}";

                float x = rowRect.x;

                // 序号
                GUI.Label(new Rect(x, rowRect.y, 20, rowRect.height), $"{i + 1}", idxStyle);
                x += 20;

                // 可编辑 Key（Tag 名）
                keyProp.stringValue = EditorGUI.TextField(
                    new Rect(x, rowRect.y + 2, 116, rowRect.height - 4), keyProp.stringValue);
                x += 120;

                // →
                GUI.Label(new Rect(x, rowRect.y, 22, rowRect.height), "→", arrowStyle);
                x += 22;

                // public Type fieldNameType（只读预览）
                float kwW   = 44f;
                GUI.Label(new Rect(x, rowRect.y, kwW, rowRect.height), "public", codeKeyStyle); x += kwW;
                float typeW = GUI.skin.label.CalcSize(new GUIContent(compType)).x + 6f;
                GUI.Label(new Rect(x, rowRect.y, typeW, rowRect.height), compType, codeTypeStyle); x += typeW;
                GUI.Label(new Rect(x, rowRect.y, rowRect.width - x + rowRect.x, rowRect.height), field, codeFieldStyle);
            }

            GUILayout.Space(4);
            if (GUILayout.Button("重置 Tag 为默认值", mStyleBtnFlat, GUILayout.Height(26)))
            {
                mComponentMappingsProp.ClearArray();
                foreach (var def in GeneratorConfig.DefaultMappings)
                {
                    mComponentMappingsProp.InsertArrayElementAtIndex(mComponentMappingsProp.arraySize);
                    var e = mComponentMappingsProp.GetArrayElementAtIndex(mComponentMappingsProp.arraySize - 1);
                    e.FindPropertyRelative("Key").stringValue           = def.Key;
                    e.FindPropertyRelative("ComponentType").stringValue = def.ComponentType;
                }
            }
            GUILayout.Space(4);
            EndCard();
        }
    }

    private void DrawPagePaths()
    {
        PageTitle("脚本生成路径", "配置各类脚本文件的输出目录");

        BeginCard("路径配置");
        PathRow("组件绑定脚本", mBindPathProp);
        if (mGeneratorTypeProp.enumValueIndex == (int)GeneratorType.Find)
        {
            Divider();
            PathRow("组件查找脚本", mFindPathProp);
        }
        Divider();
        PathRow("窗口交互脚本", mWindowPathProp);
        Divider();
        PathRow("Item 脚本",    mItemPathProp);
        EndCard();
    }

    private void DrawPagePrefabs()
    {
        PageTitle("窗口预制体路径", "框架自动扫描以下目录中的 Prefab 作为窗口资源");
        BeginCard("预制体目录列表");
        ArrayField(mPrefabFolderArrProp, isFolder: true,
            emptyHint: "尚未配置任何预制体路径，点击下方按钮添加",
            addLabel:  "添加预制体路径");
        EndCard();
    }

    private void DrawPageNamespace()
    {
        PageTitle("命名空间配置", "生成脚本时自动在文件顶部 using 以下命名空间");
        BeginCard("命名空间列表");
        ArrayField(mNamespaceArrProp, isFolder: false,
            emptyHint: "尚未配置任何命名空间，点击下方按钮添加",
            addLabel:  "添加命名空间");
        EndCard();
    }

    // ── Component Helpers ─────────────────────────────────────────────────────

    private void PageTitle(string title, string sub)
    {
        var rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(36));
        if (Event.current.type == EventType.Repaint)
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 2, 4, 32), kAccent);
        GUILayout.Space(14);
        EditorGUILayout.BeginVertical();
        GUILayout.Label(title, mStylePageTitle);
        GUILayout.Label(sub,   mStylePageSub);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(14);
    }

    private void BeginCard(string cardTitle = "")
    {
        EditorGUILayout.BeginVertical(mStyleCard);
        if (!string.IsNullOrEmpty(cardTitle))
        {
            GUILayout.Label(cardTitle.ToUpper(), mStyleCardTitle);
            var r = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(r, CDivider);
            GUILayout.Space(8);
        }
    }

    private static void EndCard() => EditorGUILayout.EndVertical();

    private void Divider()
    {
        GUILayout.Space(6);
        var r = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(r, CDivider);
        GUILayout.Space(6);
    }

    private int TabBar(int current, string[] options)
    {
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < options.Length; i++)
        {
            bool sel = current == i;
            var style = i == 0 ? mStyleTabLeft : (i == options.Length - 1 ? mStyleTabRight : mStyleTabMid);
            GUI.backgroundColor = sel ? kAccent : Color.white;
            GUI.contentColor    = sel ? Color.white : CTextPri;
            if (GUILayout.Button(options[i], style))
                current = i;
            GUI.backgroundColor = Color.white;
            GUI.contentColor    = Color.white;
        }
        EditorGUILayout.EndHorizontal();
        return current;
    }

    private void PathRow(string label, SerializedProperty prop)
    {
        GUILayout.Label(label, mStyleFieldLabel);
        GUILayout.Space(2);
        EditorGUILayout.BeginHorizontal();
        prop.stringValue = EditorGUILayout.TextField(prop.stringValue, GUILayout.ExpandWidth(true));
        if (GUILayout.Button(EditorGUIUtility.IconContent("d_FolderOpened Icon"), mStyleBtnBrowse, GUILayout.Width(26), GUILayout.Height(20)))
        {
            string sel = EditorUtility.OpenFolderPanel("选择目录",
                string.IsNullOrEmpty(prop.stringValue) ? "Assets" : prop.stringValue, "");
            if (!string.IsNullOrEmpty(sel))
            {
                prop.stringValue = ToRelative(sel);
                GUI.FocusControl(null);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void ArrayField(SerializedProperty arr, bool isFolder, string emptyHint, string addLabel)
    {
        if (arr.arraySize == 0)
        {
            GUILayout.Label(emptyHint, mStyleHint);
            GUILayout.Space(6);
        }

        // 用 deleteIndex 记录待删行，循环结束后再删，避免 break 导致 Begin/End 不匹配
        int deleteIndex = -1;

        for (int i = 0; i < arr.arraySize; i++)
        {
            var elem = arr.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginHorizontal();

            // Row index badge
            var idxStyle = new GUIStyle
            {
                normal    = { textColor = kAccent },
                fontSize  = 10,
                alignment = TextAnchor.MiddleCenter,
            };
            GUILayout.Label($"{i + 1}", idxStyle, GUILayout.Width(18));

            elem.stringValue = EditorGUILayout.TextField(elem.stringValue, GUILayout.ExpandWidth(true));

            if (isFolder && GUILayout.Button(EditorGUIUtility.IconContent("d_FolderOpened Icon"), mStyleBtnBrowse, GUILayout.Width(26), GUILayout.Height(20)))
            {
                string sel = EditorUtility.OpenFolderPanel("选择目录",
                    string.IsNullOrEmpty(elem.stringValue) ? "Assets" : elem.stringValue, "");
                if (!string.IsNullOrEmpty(sel))
                {
                    elem.stringValue = ToRelative(sel);
                    GUI.FocusControl(null);
                }
            }

            if (GUILayout.Button("✕", mStyleBtnFlat, GUILayout.Width(24), GUILayout.Height(20)))
                deleteIndex = i;

            // EndHorizontal 必须在 break/return 之前调用，保证 Begin/End 配对
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3);
        }

        // 循环结束后统一执行删除
        if (deleteIndex >= 0)
            arr.DeleteArrayElementAtIndex(deleteIndex);

        GUILayout.Space(4);
        if (GUILayout.Button($"＋  {addLabel}", mStyleBtnFlat, GUILayout.Height(26)))
        {
            arr.InsertArrayElementAtIndex(arr.arraySize);
            arr.GetArrayElementAtIndex(arr.arraySize - 1).stringValue = "";
        }
    }

    // ── Utils ─────────────────────────────────────────────────────────────────

    private static string ToRelative(string abs)
    {
        return abs.StartsWith(Application.dataPath)
            ? "Assets" + abs.Substring(Application.dataPath.Length)
            : abs;
    }

    private static Texture2D Tex(Color c)
    {
        var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }

    private static void DestroyTex(ref Texture2D t)
    {
        if (t != null) { DestroyImmediate(t); t = null; }
    }
}
