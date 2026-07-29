using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// ZMUI 编辑器统一视觉主题。
/// 主题只负责颜色、纹理和通用控件，不读取或修改任何业务配置。
/// </summary>
internal static class ZMUIEditorTheme
{
    internal enum Preset
    {
        Nebula,
        TechnologyBlue,
        Emerald,
        ObsidianGold,
        Rose,
        DeepOcean
    }

    private const string ThemePreferenceKey = "ZMUI.Editor.ThemePreset";

    internal static Color Window { get; private set; }
    internal static Color Header { get; private set; }
    internal static Color Sidebar { get; private set; }
    internal static Color Panel { get; private set; }
    internal static Color Card { get; private set; }
    internal static Color Field { get; private set; }
    internal static Color Border { get; private set; }
    internal static Color Accent { get; private set; }
    internal static Color AccentHover { get; private set; }
    internal static Color Cyan { get; private set; }
    internal static Color Success { get; private set; }
    internal static Color Danger { get; private set; }
    internal static Color Text { get; private set; }
    internal static Color Muted { get; private set; }
    internal static Preset CurrentPreset { get; private set; }
    internal static string CurrentPresetName => PresetName(CurrentPreset);

    internal static GUIStyle HeaderTitle, PageTitle, PageSubtitle, NavItem, NavItemSelected;
    internal static GUIStyle CardBox, CardTitle, Body, Hint, Label, Badge, StatusChip;
    internal static GUIStyle Segment, SegmentSelected, ModeCard, ModeCardSelected, ModeIconBox, ModeIconBoxSelected, ModeTitle, ModeHint;
    internal static GUIStyle Input, FieldBox, IconButton, PrimaryButton, SecondaryButton, DeleteButton;
    internal static GUIStyle InfoBox, InfoText, TableBox, TableHeader, TableCell, CodeCell, CountBadge;
    internal static GUIStyle ToggleTrackOn, ToggleTrackOff, ToggleKnob, SuccessBadge;

    private static readonly List<Texture2D> Textures = new List<Texture2D>();
    private static int users;
    private static bool paletteLoaded;

    internal static void Acquire()
    {
        users++;
        LoadPalette();
        Ensure();
    }

    internal static void Release()
    {
        users = Mathf.Max(0, users - 1);
        if (users > 0) return;
        foreach (Texture2D texture in Textures)
            if (texture != null) Object.DestroyImmediate(texture);
        Textures.Clear();
        HeaderTitle = null;
    }

    internal static void SetPreset(Preset preset)
    {
        if (paletteLoaded && CurrentPreset == preset) return;
        CurrentPreset = preset;
        paletteLoaded = true;
        EditorPrefs.SetInt(ThemePreferenceKey, (int)preset);
        ApplyPalette(preset);
        RebuildStyles();
    }

    internal static string PresetName(Preset preset)
    {
        switch (preset)
        {
            case Preset.TechnologyBlue: return "科技蓝";
            case Preset.Emerald: return "翡翠青";
            case Preset.ObsidianGold: return "曜石金";
            case Preset.Rose: return "绯红玫瑰";
            case Preset.DeepOcean: return "深海青";
            default: return "星云紫";
        }
    }

    internal static void Ensure()
    {
        LoadPalette();
        if (HeaderTitle != null) return;

        HeaderTitle = TextStyle(26, FontStyle.Bold, Text, TextAnchor.MiddleLeft);
        PageTitle = TextStyle(24, FontStyle.Bold, Text, TextAnchor.MiddleLeft);
        PageSubtitle = TextStyle(13, FontStyle.Normal, Muted, TextAnchor.MiddleLeft);
        NavItem = TextStyle(13, FontStyle.Normal, new Color32(190, 196, 207, 255), TextAnchor.MiddleLeft);
        NavItem.padding = new RectOffset(52, 12, 0, 0);
        NavItemSelected = new GUIStyle(NavItem) { fontStyle = FontStyle.Bold };
        NavItemSelected.normal.textColor = Color.white;

        CardBox = Box(Rounded(32, 10, Card, Border, 1), 11);
        CardBox.padding = new RectOffset(20, 20, 16, 16);
        CardTitle = TextStyle(16, FontStyle.Bold, Text, TextAnchor.MiddleLeft);
        Body = TextStyle(12, FontStyle.Normal, new Color32(203, 208, 217, 255), TextAnchor.MiddleLeft);
        Body.wordWrap = true;
        Hint = TextStyle(11, FontStyle.Normal, Muted, TextAnchor.MiddleLeft);
        Hint.wordWrap = true;
        Label = TextStyle(12, FontStyle.Bold, new Color32(218, 222, 229, 255), TextAnchor.MiddleLeft);

        Badge = Box(Rounded(24, 7, Color.Lerp(Header, Accent, .18f), Color.Lerp(Border, Accent, .48f), 1), 8);
        Badge.alignment = TextAnchor.MiddleCenter;
        Badge.fontSize = 11;
        Badge.normal.textColor = Color.Lerp(Color.white, Accent, .48f);
        StatusChip = new GUIStyle(Badge) { fontSize = 12, fixedHeight = 34 };

        Segment = Button(
            Rounded(24, 7, new Color32(25, 28, 34, 255), Border, 1),
            Rounded(24, 7, new Color32(34, 38, 46, 255), new Color32(76, 84, 97, 255), 1),
            new Color32(212, 216, 224, 255), 12, FontStyle.Normal);
        Segment.fixedHeight = 38;
        SegmentSelected = Button(
            Rounded(24, 7, Accent, new Color32(132, 143, 255, 255), 1),
            Rounded(24, 7, AccentHover, new Color32(150, 159, 255, 255), 1),
            Color.white, 12, FontStyle.Bold);
        SegmentSelected.fixedHeight = 38;

        ModeCard = Box(Rounded(32, 10, new Color32(29, 32, 38, 255), Border, 1), 11);
        ModeCardSelected = Box(Rounded(32, 10, new Color32(31, 34, 47, 255), Accent, 2), 11);
        ModeIconBox = Box(Rounded(32, 16, new Color32(39, 43, 51, 255), new Color32(72, 79, 92, 255), 1), 12);
        ModeIconBoxSelected = Box(Rounded(32, 16, Color.Lerp(Card, Accent, .20f), Color.Lerp(Border, Accent, .62f), 1), 12);
        ModeTitle = TextStyle(14, FontStyle.Bold, Text, TextAnchor.MiddleLeft);
        ModeHint = TextStyle(11, FontStyle.Normal, Muted, TextAnchor.MiddleLeft);

        FieldBox = Box(Rounded(24, 7, Field, Border, 1), 8);
        Input = new GUIStyle(GUIStyle.none)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(11, 11, 0, 0),
            clipping = TextClipping.Clip
        };
        SetTextColors(Input, Text);
        IconButton = Button(
            Rounded(24, 7, new Color32(43, 47, 55, 255), Border, 1),
            Rounded(24, 7, new Color32(51, 57, 68, 255), new Color32(83, 91, 106, 255), 1),
            Text, 12, FontStyle.Normal);
        IconButton.fixedHeight = 34;
        PrimaryButton = Button(
            Rounded(24, 7, Accent, Accent, 0),
            Rounded(24, 7, AccentHover, AccentHover, 0),
            Color.white, 13, FontStyle.Bold);
        PrimaryButton.fixedHeight = 38;
        SecondaryButton = Button(
            Rounded(24, 7, new Color32(39, 43, 51, 255), new Color32(78, 85, 99, 255), 1),
            Rounded(24, 7, new Color32(49, 54, 64, 255), new Color32(99, 108, 125, 255), 1),
            new Color32(217, 221, 228, 255), 12, FontStyle.Normal);
        SecondaryButton.fixedHeight = 38;
        DeleteButton = Button(
            Rounded(24, 7, new Color32(43, 47, 55, 255), Border, 1),
            Rounded(24, 7, new Color32(70, 40, 45, 255), new Color32(142, 59, 67, 255), 1),
            Muted, 17, FontStyle.Normal);
        DeleteButton.fixedHeight = 34;
        DeleteButton.hover.textColor = new Color32(255, 133, 141, 255);
        DeleteButton.active.textColor = Color.white;

        InfoBox = Box(Rounded(24, 7, Color.Lerp(Field, Accent, .16f), Color.Lerp(Border, Accent, .42f), 1), 8);
        InfoText = TextStyle(11, FontStyle.Normal, Color.Lerp(Color.white, Accent, .38f), TextAnchor.MiddleLeft);
        InfoText.padding = new RectOffset(12, 12, 0, 0);
        TableBox = Box(Rounded(28, 9, new Color32(24, 27, 32, 255), Border, 1), 10);
        TableHeader = TextStyle(11, FontStyle.Bold, new Color32(190, 196, 207, 255), TextAnchor.MiddleLeft);
        TableCell = TextStyle(11, FontStyle.Normal, new Color32(219, 223, 230, 255), TextAnchor.MiddleLeft);
        CodeCell = TextStyle(11, FontStyle.Normal, Color.Lerp(Color.white, Accent, .34f), TextAnchor.MiddleLeft);
        CountBadge = new GUIStyle(Badge) { fixedHeight = 24, fontSize = 11 };

        ToggleTrackOn = Box(Rounded(32, 12, Accent, Accent, 0), 12);
        ToggleTrackOff = Box(Rounded(32, 12, new Color32(63, 68, 78, 255), new Color32(85, 91, 103, 255), 1), 12);
        // 开关圆点使用与实际绘制尺寸接近的纹理和较小九宫边距，
        // 避免 32px 纹理压缩到 18px 时圆角采样区域互相挤压产生锯齿。
        ToggleKnob = Box(Rounded(24, 12, Color.white, new Color32(218, 222, 230, 255), 1), 8);
        // 星云紫沿用设计稿最初的绿色推荐标识；其他主题使用各自的强调色，
        // 既保留已验收的默认视觉，也让新增主题拥有一致的色彩反馈。
        Color recommendation = CurrentPreset == Preset.Nebula ? Success : Accent;
        SuccessBadge = Box(Rounded(24, 7, Color.Lerp(Field, recommendation, .22f),
            Color.Lerp(Border, recommendation, .62f), 1), 8);
        SuccessBadge.alignment = TextAnchor.MiddleCenter;
        SuccessBadge.fontSize = 10;
        SetTextColors(SuccessBadge, Color.Lerp(Color.white, recommendation, .30f));
    }

    private static void LoadPalette()
    {
        if (paletteLoaded) return;
        int saved = EditorPrefs.GetInt(ThemePreferenceKey, (int)Preset.Nebula);
        CurrentPreset = System.Enum.IsDefined(typeof(Preset), saved) ? (Preset)saved : Preset.Nebula;
        paletteLoaded = true;
        ApplyPalette(CurrentPreset);
    }

    private static void ApplyPalette(Preset preset)
    {
        // 三套主题共享尺寸和层级，仅调整基底冷暖与品牌强调色，
        // 从而保证换肤不会改变已验收的“ZMUI 设计1”布局。
        switch (preset)
        {
            case Preset.TechnologyBlue:
                Window = new Color32(24, 28, 33, 255);
                Header = new Color32(19, 24, 30, 255);
                Sidebar = new Color32(22, 27, 33, 255);
                Panel = new Color32(30, 35, 42, 255);
                Card = new Color32(35, 41, 49, 255);
                Field = new Color32(21, 26, 32, 255);
                Border = new Color32(54, 67, 80, 255);
                Accent = new Color32(46, 145, 238, 255);
                AccentHover = new Color32(62, 163, 255, 255);
                Cyan = new Color32(65, 190, 240, 255);
                Success = new Color32(67, 199, 139, 255);
                break;
            case Preset.Emerald:
                Window = new Color32(23, 29, 29, 255);
                Header = new Color32(18, 25, 25, 255);
                Sidebar = new Color32(21, 28, 28, 255);
                Panel = new Color32(29, 37, 37, 255);
                Card = new Color32(34, 43, 43, 255);
                Field = new Color32(20, 27, 27, 255);
                Border = new Color32(54, 72, 70, 255);
                Accent = new Color32(52, 181, 143, 255);
                AccentHover = new Color32(64, 204, 159, 255);
                Cyan = new Color32(73, 193, 197, 255);
                Success = new Color32(91, 213, 143, 255);
                break;
            case Preset.ObsidianGold:
                Window = new Color32(27, 26, 23, 255);
                Header = new Color32(22, 21, 18, 255);
                Sidebar = new Color32(25, 24, 21, 255);
                Panel = new Color32(34, 32, 27, 255);
                Card = new Color32(41, 38, 31, 255);
                Field = new Color32(24, 23, 20, 255);
                Border = new Color32(75, 68, 53, 255);
                Accent = new Color32(211, 160, 67, 255);
                AccentHover = new Color32(232, 181, 80, 255);
                Cyan = new Color32(104, 188, 194, 255);
                Success = new Color32(91, 194, 124, 255);
                break;
            case Preset.Rose:
                Window = new Color32(29, 24, 28, 255);
                Header = new Color32(24, 19, 23, 255);
                Sidebar = new Color32(27, 22, 26, 255);
                Panel = new Color32(37, 30, 35, 255);
                Card = new Color32(44, 35, 41, 255);
                Field = new Color32(26, 21, 25, 255);
                Border = new Color32(77, 57, 69, 255);
                Accent = new Color32(212, 82, 125, 255);
                AccentHover = new Color32(232, 99, 143, 255);
                Cyan = new Color32(91, 180, 210, 255);
                Success = new Color32(76, 194, 131, 255);
                break;
            case Preset.DeepOcean:
                Window = new Color32(20, 27, 30, 255);
                Header = new Color32(16, 22, 25, 255);
                Sidebar = new Color32(18, 25, 28, 255);
                Panel = new Color32(25, 35, 39, 255);
                Card = new Color32(30, 42, 46, 255);
                Field = new Color32(17, 25, 28, 255);
                Border = new Color32(45, 70, 76, 255);
                Accent = new Color32(35, 166, 177, 255);
                AccentHover = new Color32(44, 190, 201, 255);
                Cyan = new Color32(63, 184, 221, 255);
                Success = new Color32(75, 194, 139, 255);
                break;
            default:
                Window = new Color32(25, 27, 31, 255);
                Header = new Color32(20, 23, 28, 255);
                Sidebar = new Color32(24, 27, 32, 255);
                Panel = new Color32(31, 34, 40, 255);
                Card = new Color32(37, 41, 48, 255);
                Field = new Color32(23, 26, 31, 255);
                Border = new Color32(59, 65, 76, 255);
                Accent = new Color32(103, 117, 255, 255);
                AccentHover = new Color32(120, 133, 255, 255);
                Cyan = new Color32(67, 189, 245, 255);
                Success = new Color32(69, 201, 138, 255);
                break;
        }

        Danger = new Color32(234, 91, 101, 255);
        Text = new Color32(237, 240, 245, 255);
        Muted = new Color32(146, 153, 166, 255);
    }

    private static void RebuildStyles()
    {
        foreach (Texture2D texture in Textures)
            if (texture != null) Object.DestroyImmediate(texture);
        Textures.Clear();
        HeaderTitle = null;
        Ensure();
    }

    internal static string TextField(Rect rect, string value, string placeholder = null)
    {
        GUI.Box(rect, GUIContent.none, FieldBox);
        string result = GUI.TextField(rect, value ?? string.Empty, Input);
        if (string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(placeholder))
            GUI.Label(rect, placeholder, new GUIStyle(Hint) { padding = new RectOffset(11, 8, 0, 0), alignment = TextAnchor.MiddleLeft });
        return result;
    }

    internal static bool Toggle(Rect rect, bool value)
    {
        if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) value = !value;
        GUI.Box(rect, GUIContent.none, value ? ToggleTrackOn : ToggleTrackOff);
        float knob = rect.height - 6;
        float x = value ? rect.xMax - knob - 3 : rect.x + 3;
        GUI.Box(new Rect(x, rect.y + 3, knob, knob), GUIContent.none, ToggleKnob);
        return value;
    }

    private static GUIStyle TextStyle(int size, FontStyle font, Color color, TextAnchor anchor)
    {
        return new GUIStyle(EditorStyles.label)
        {
            fontSize = size,
            fontStyle = font,
            alignment = anchor,
            normal = { textColor = color }
        };
    }

    private static GUIStyle Button(Texture2D normal, Texture2D hover, Color color, int size, FontStyle font)
    {
        var style = new GUIStyle(GUIStyle.none)
        {
            fontSize = size,
            fontStyle = font,
            alignment = TextAnchor.MiddleCenter,
            border = new RectOffset(8, 8, 8, 8)
        };
        style.normal.background = normal;
        style.hover.background = hover;
        style.active.background = hover;
        SetTextColors(style, color);
        return style;
    }

    private static GUIStyle Box(Texture2D texture, int border)
    {
        return new GUIStyle(GUIStyle.none)
        {
            border = new RectOffset(border, border, border, border),
            normal = { background = texture }
        };
    }

    private static void SetTextColors(GUIStyle style, Color color)
    {
        style.normal.textColor = color;
        style.hover.textColor = color;
        style.active.textColor = color;
        style.focused.textColor = color;
        style.onNormal.textColor = color;
        style.onHover.textColor = color;
        style.onActive.textColor = color;
        style.onFocused.textColor = color;
    }

    private static Texture2D Rounded(int size, float radius, Color fill, Color border, float borderWidth)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            alphaIsTransparency = true
        };
        const int samples = 4;
        float inverseSamples = 1f / (samples * samples);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            Color sum = Color.clear;
            int covered = 0;
            for (int sy = 0; sy < samples; sy++)
            for (int sx = 0; sx < samples; sx++)
            {
                float px = x + (sx + .5f) / samples;
                float py = y + (sy + .5f) / samples;
                if (!InsideRoundedRect(px, py, size, radius, 0)) continue;
                Color sample = borderWidth <= 0 || InsideRoundedRect(px, py, size, radius, borderWidth) ? fill : border;
                sum.r += sample.r;
                sum.g += sample.g;
                sum.b += sample.b;
                covered++;
            }
            texture.SetPixel(x, y, covered == 0
                ? Color.clear
                : new Color(sum.r / covered, sum.g / covered, sum.b / covered, covered * inverseSamples));
        }
        texture.Apply();
        Textures.Add(texture);
        return texture;
    }

    private static bool InsideRoundedRect(float x, float y, float size, float radius, float inset)
    {
        float left = inset;
        float top = inset;
        float right = size - inset;
        float bottom = size - inset;
        if (x < left || x > right || y < top || y > bottom) return false;
        float r = Mathf.Max(0, radius - inset);
        if (r <= 0) return true;
        float cx = Mathf.Clamp(x, left + r, right - r);
        float cy = Mathf.Clamp(y, top + r, bottom - r);
        float dx = x - cx;
        float dy = y - cy;
        return dx * dx + dy * dy <= r * r;
    }
}
