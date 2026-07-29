#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// ZMUI 编辑器统一结果提示窗口。
/// 仅负责结果呈现，与具体代码生成业务保持解耦。
/// </summary>
internal sealed class ZMUIMessageWindow : EditorWindow
{
    private const float WindowWidth = 460f;
    private const float WindowHeight = 286f;
    private const double FadeDuration = .16d;

    private string messageTitle;
    private string message;
    private string targetPath;
    private double openedAt;
    [NonSerialized] private GUIStyle pathStyle;
    [NonSerialized] private bool themeAcquired;

    internal static void ShowSuccess(EditorWindow owner, string title, string description, string path)
    {
        Rect ownerPosition = owner != null ? owner.position : default;
        // 调用方会立即关闭生成预览窗口，因此在下一次 Editor 更新中创建独立 Popup。
        EditorApplication.delayCall += () =>
        {
            var window = CreateInstance<ZMUIMessageWindow>();
            window.messageTitle = title;
            window.message = description;
            window.targetPath = path;
            window.openedAt = EditorApplication.timeSinceStartup;
            window.position = CenterOver(ownerPosition);
            window.ShowPopup();
            window.Focus();
        };
    }

    private static Rect CenterOver(Rect ownerPosition)
    {
        bool hasOwner = ownerPosition.width > 0f && ownerPosition.height > 0f;
        Rect anchor = hasOwner
            ? ownerPosition
            : new Rect(Screen.currentResolution.width * .5f,
                Screen.currentResolution.height * .5f, 0, 0);
        return new Rect(anchor.x + (anchor.width - WindowWidth) * .5f,
            anchor.y + (anchor.height - WindowHeight) * .5f,
            WindowWidth, WindowHeight);
    }

    private void OnEnable()
    {
        // 域重载期间 OnEnable 早于 EditorStyles 初始化，主题延迟到首次 OnGUI 创建。
        themeAcquired = false;
        pathStyle = null;
    }

    private void OnDisable()
    {
        if (themeAcquired)
            ZMUIEditorTheme.Release();
        themeAcquired = false;
        pathStyle = null;
    }

    private void OnGUI()
    {
        EnsureTheme();
        EnsureLocalStyles();
        float alpha = Mathf.Clamp01((float)((EditorApplication.timeSinceStartup - openedAt) /
                                             FadeDuration));
        Color previousColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, alpha);

        Rect bounds = new Rect(1, 1, position.width - 2, position.height - 2);
        GUI.Box(bounds, GUIContent.none, ZMUIEditorTheme.CardBox);
        DrawContent(bounds);

        GUI.color = previousColor;
        if (alpha < 1f) Repaint();
    }

    private void EnsureTheme()
    {
        if (themeAcquired) return;
        ZMUIEditorTheme.Acquire();
        themeAcquired = true;
    }

    private void DrawContent(Rect bounds)
    {
        Rect iconBox = new Rect(bounds.x + 24, bounds.y + 24, 46, 46);
        GUI.Box(iconBox, GUIContent.none, ZMUIEditorTheme.ModeIconBoxSelected);
        ZMUIEditorIcons.Draw(new Rect(iconBox.x + 10, iconBox.y + 10, 26, 26),
            ZMUIEditorIcons.Icon.Saved, ZMUIEditorTheme.Success, 2f);

        GUI.Label(new Rect(iconBox.xMax + 14, bounds.y + 22, 330, 26),
            string.IsNullOrEmpty(messageTitle) ? "生成成功" : messageTitle,
            ZMUIEditorTheme.CardTitle);
        GUI.Label(new Rect(iconBox.xMax + 14, bounds.y + 49, 330, 20),
            string.IsNullOrEmpty(message) ? "脚本已成功写入目标位置。" : message,
            ZMUIEditorTheme.Hint);

        Rect pathBox = new Rect(bounds.x + 24, bounds.y + 91, bounds.width - 48, 88);
        GUI.Box(pathBox, GUIContent.none, ZMUIEditorTheme.FieldBox);
        GUI.Label(new Rect(pathBox.x + 12, pathBox.y + 7, pathBox.width - 24, 16),
            "生成位置", ZMUIEditorTheme.Hint);
        EditorGUI.SelectableLabel(new Rect(pathBox.x + 12, pathBox.y + 26,
                pathBox.width - 24, 54),
            string.IsNullOrEmpty(targetPath) ? "未提供路径" : targetPath, pathStyle);

        EditorGUI.DrawRect(new Rect(bounds.x + 24, bounds.yMax - 76, bounds.width - 48, 1),
            ZMUIEditorTheme.Border);
        Rect revealButton = new Rect(bounds.x + 116, bounds.yMax - 58, 176, 38);
        Rect doneButton = new Rect(revealButton.xMax + 10, revealButton.y, 118, 38);
        if (GUI.Button(revealButton, "在资源管理器中显示", ZMUIEditorTheme.SecondaryButton) &&
            !string.IsNullOrEmpty(targetPath))
            EditorUtility.RevealInFinder(targetPath);
        if (GUI.Button(doneButton, "完成", ZMUIEditorTheme.PrimaryButton))
            Close();
    }

    private void EnsureLocalStyles()
    {
        if (pathStyle != null) return;
        pathStyle = new GUIStyle(ZMUIEditorTheme.Body)
        {
            alignment = TextAnchor.UpperLeft,
            wordWrap = true,
            clipping = TextClipping.Clip,
            fontSize = 11,
            padding = new RectOffset(0, 0, 2, 2)
        };
        Color pathColor = ZMUIEditorTheme.Text;
        pathStyle.normal.textColor = pathColor;
        pathStyle.hover.textColor = pathColor;
        pathStyle.active.textColor = pathColor;
        pathStyle.focused.textColor = pathColor;
        pathStyle.normal.background = null;
        pathStyle.hover.background = null;
        pathStyle.active.background = null;
        pathStyle.focused.background = null;
    }
}
#endif
