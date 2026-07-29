using UnityEditor;
using UnityEngine;

/// <summary>
/// ZMUI 编辑器矢量图标库。
/// 所有图标使用 Unity Handles 抗锯齿绘制，不依赖外部图片和 Unity 内置图标名称。
/// </summary>
internal static class ZMUIEditorIcons
{
    internal enum Icon
    {
        Logo,
        Mask,
        Code,
        Folder,
        Prefab,
        Namespace,
        Search,
        Link,
        UGUI,
        Palette,
        Saved,
        Book
    }

    internal static void Draw(Rect rect, Icon icon, Color color, float width = 1.8f)
    {
        if (Event.current.type != EventType.Repaint) return;
        Handles.BeginGUI();
        Color previous = Handles.color;
        Handles.color = color;

        switch (icon)
        {
            case Icon.Logo: DrawLogo(rect, width); break;
            case Icon.Mask: DrawMask(rect, width); break;
            case Icon.Code: DrawCode(rect, width); break;
            case Icon.Folder: DrawFolder(rect, width); break;
            case Icon.Prefab: DrawCube(rect, width); break;
            case Icon.Namespace: DrawNamespace(rect, width); break;
            case Icon.Search: DrawSearch(rect, width); break;
            case Icon.Link: DrawLink(rect, width); break;
            case Icon.UGUI: DrawUGUI(rect, width); break;
            case Icon.Palette: DrawPalette(rect, width); break;
            case Icon.Saved: DrawSaved(rect, width); break;
            case Icon.Book: DrawBook(rect, width); break;
        }

        Handles.color = previous;
        Handles.EndGUI();
    }

    /// <summary>
    /// 绘制组件映射表专用类型图标。颜色与内部符号均由组件类型稳定映射，
    /// 保证新增映射行时无需在窗口代码中维护独立贴图资源。
    /// </summary>
    internal static void DrawComponent(Rect rect, string componentType)
    {
        if (Event.current.type != EventType.Repaint) return;
        Color color = ComponentColor(componentType);
        Handles.BeginGUI();
        Color previous = Handles.color;
        Handles.color = color;
        Handles.DrawSolidDisc(rect.center, Vector3.forward, Mathf.Min(rect.width, rect.height) * .47f);
        Handles.color = new Color(1f, 1f, 1f, .94f);

        if (componentType != null && componentType.Contains("Text"))
        {
            Line(1.5f, P(rect, .27f, .27f), P(rect, .73f, .27f));
            Line(1.5f, P(rect, .50f, .27f), P(rect, .50f, .76f));
        }
        else if (componentType != null && (componentType.Contains("Image") || componentType.Contains("Sprite")))
        {
            Line(1.4f, P(rect, .22f, .70f), P(rect, .42f, .47f), P(rect, .55f, .60f), P(rect, .68f, .38f), P(rect, .80f, .70f));
            Circle(P(rect, .35f, .34f), rect.width * .055f, 1.2f);
        }
        else if (componentType != null && (componentType.Contains("List") || componentType.Contains("Scroll")))
        {
            for (int i = 0; i < 3; i++)
            {
                float y = .28f + i * .22f;
                Circle(P(rect, .27f, y), rect.width * .025f, 1.1f);
                Line(1.3f, P(rect, .40f, y), P(rect, .76f, y));
            }
        }
        else if (componentType == "Button")
        {
            RoundedRect(Inset(rect, rect.width * .23f), 2.5f, 1.35f);
            Line(1.25f, P(rect, .39f, .51f), P(rect, .61f, .51f));
        }
        else
        {
            RoundedRect(Inset(rect, rect.width * .25f), 2.5f, 1.35f);
        }

        Handles.color = previous;
        Handles.EndGUI();
    }

    private static void DrawLogo(Rect rect, float width)
    {
        Rect body = Inset(rect, 1.5f);
        RoundedRect(body, 4, width);
        Line(width, P(body, .08f, .28f), P(body, .92f, .28f));
        Circle(P(body, .16f, .16f), body.width * .035f, width);
        Circle(P(body, .28f, .16f), body.width * .035f, width);
        Line(width, P(body, .16f, .48f), P(body, .72f, .48f));
        Line(width, P(body, .16f, .67f), P(body, .84f, .67f));
        Line(width, P(body, .16f, .84f), P(body, .58f, .84f));
    }

    private static void DrawMask(Rect rect, float width)
    {
        Vector2 center = rect.center;
        float rx = rect.width * .42f;
        float ry = rect.height * .25f;
        Arc(center, rx, ry, 0, 360, width);
        Circle(center, rect.width * .095f, width);
        Arc(new Vector2(center.x - 2, center.y + 3), rx * .78f, ry * 1.7f, 205, 315, width);
    }

    private static void DrawCode(Rect rect, float width)
    {
        Line(width, P(rect, .38f, .18f), P(rect, .13f, .50f), P(rect, .38f, .82f));
        Line(width, P(rect, .62f, .18f), P(rect, .87f, .50f), P(rect, .62f, .82f));
        Line(width, P(rect, .57f, .08f), P(rect, .43f, .92f));
    }

    private static void DrawFolder(Rect rect, float width)
    {
        Line(width,
            P(rect, .08f, .30f), P(rect, .36f, .30f), P(rect, .45f, .18f),
            P(rect, .74f, .18f), P(rect, .82f, .30f), P(rect, .92f, .30f),
            P(rect, .86f, .82f), P(rect, .10f, .82f), P(rect, .08f, .30f));
    }

    private static void DrawBook(Rect rect, float width)
    {
        Line(width, P(rect, .10f, .18f), P(rect, .41f, .18f), P(rect, .50f, .27f),
            P(rect, .59f, .18f), P(rect, .90f, .18f), P(rect, .90f, .82f),
            P(rect, .59f, .82f), P(rect, .50f, .90f));
        Line(width, P(rect, .50f, .27f), P(rect, .50f, .90f),
            P(rect, .41f, .82f), P(rect, .10f, .82f), P(rect, .10f, .18f));
        Line(width, P(rect, .20f, .37f), P(rect, .39f, .37f));
        Line(width, P(rect, .61f, .37f), P(rect, .80f, .37f));
    }

    private static void DrawCube(Rect rect, float width)
    {
        Vector2 top = P(rect, .50f, .08f);
        Vector2 left = P(rect, .12f, .30f);
        Vector2 right = P(rect, .88f, .30f);
        Vector2 center = P(rect, .50f, .52f);
        Vector2 bottomLeft = P(rect, .12f, .70f);
        Vector2 bottom = P(rect, .50f, .92f);
        Vector2 bottomRight = P(rect, .88f, .70f);
        Line(width, top, left, center, right, top);
        Line(width, left, bottomLeft, bottom, bottomRight, right);
        Line(width, center, bottom);
    }

    private static void DrawNamespace(Rect rect, float width)
    {
        Line(width, P(rect, .34f, .10f), P(rect, .23f, .10f), P(rect, .18f, .30f),
            P(rect, .18f, .43f), P(rect, .08f, .50f), P(rect, .18f, .57f),
            P(rect, .18f, .70f), P(rect, .23f, .90f), P(rect, .34f, .90f));
        Line(width, P(rect, .66f, .10f), P(rect, .77f, .10f), P(rect, .82f, .30f),
            P(rect, .82f, .43f), P(rect, .92f, .50f), P(rect, .82f, .57f),
            P(rect, .82f, .70f), P(rect, .77f, .90f), P(rect, .66f, .90f));
    }

    private static void DrawSearch(Rect rect, float width)
    {
        Vector2 center = P(rect, .43f, .42f);
        Circle(center, rect.width * .25f, width);
        Line(width, P(rect, .61f, .60f), P(rect, .88f, .87f));
    }

    private static void DrawLink(Rect rect, float width)
    {
        Arc(P(rect, .38f, .62f), rect.width * .27f, rect.height * .19f, 110, 310, width);
        Arc(P(rect, .62f, .38f), rect.width * .27f, rect.height * .19f, -70, 130, width);
        Line(width, P(rect, .38f, .62f), P(rect, .62f, .38f));
    }

    private static void DrawUGUI(Rect rect, float width)
    {
        Circle(rect.center, rect.width * .40f, width);
        Circle(rect.center, rect.width * .09f, width);
        for (int i = 0; i < 4; i++)
        {
            float angle = i * Mathf.PI * .5f + Mathf.PI * .25f;
            Vector2 a = rect.center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * rect.width * .25f;
            Circle(a, rect.width * .025f, width);
        }
    }

    private static void DrawPalette(Rect rect, float width)
    {
        // 使用不对称轮廓和拇指孔表现调色盘，确保 16px 状态栏尺寸下仍易于识别。
        Arc(P(rect, .48f, .50f), rect.width * .40f, rect.height * .38f, 35, 350, width);
        Arc(P(rect, .48f, .50f), rect.width * .40f, rect.height * .38f, -10, 23, width);
        Circle(P(rect, .65f, .66f), rect.width * .095f, width);
        Circle(P(rect, .27f, .40f), rect.width * .035f, width);
        Circle(P(rect, .43f, .25f), rect.width * .035f, width);
        Circle(P(rect, .62f, .31f), rect.width * .035f, width);
    }

    private static void DrawSaved(Rect rect, float width)
    {
        Circle(rect.center, rect.width * .42f, width);
        Line(width, P(rect, .24f, .52f), P(rect, .43f, .70f), P(rect, .76f, .33f));
    }

    private static void RoundedRect(Rect rect, float radius, float width)
    {
        const int cornerSegments = 5;
        Vector3[] points = new Vector3[(cornerSegments + 1) * 4 + 1];
        int index = 0;
        Vector2[] centers =
        {
            new Vector2(rect.xMax - radius, rect.y + radius),
            new Vector2(rect.xMax - radius, rect.yMax - radius),
            new Vector2(rect.x + radius, rect.yMax - radius),
            new Vector2(rect.x + radius, rect.y + radius)
        };
        float[] starts = { -90, 0, 90, 180 };
        for (int corner = 0; corner < 4; corner++)
        for (int i = 0; i <= cornerSegments; i++)
        {
            float angle = (starts[corner] + i * 90f / cornerSegments) * Mathf.Deg2Rad;
            points[index++] = centers[corner] + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }
        points[index] = points[0];
        Handles.DrawAAPolyLine(width, points);
    }

    private static void Circle(Vector2 center, float radius, float width)
    {
        const int segments = 28;
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2 / segments;
            points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }
        Handles.DrawAAPolyLine(width, points);
    }

    private static void Arc(Vector2 center, float radiusX, float radiusY, float startDegrees, float endDegrees, float width)
    {
        const int segments = 28;
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Lerp(startDegrees, endDegrees, i / (float)segments) * Mathf.Deg2Rad;
            points[i] = center + new Vector2(Mathf.Cos(angle) * radiusX, Mathf.Sin(angle) * radiusY);
        }
        Handles.DrawAAPolyLine(width, points);
    }

    private static void Line(float width, params Vector2[] points)
    {
        Vector3[] vertices = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++) vertices[i] = points[i];
        Handles.DrawAAPolyLine(width, vertices);
    }

    private static Vector2 P(Rect rect, float x, float y)
    {
        return new Vector2(rect.x + rect.width * x, rect.y + rect.height * y);
    }

    private static Rect Inset(Rect rect, float amount)
    {
        return new Rect(rect.x + amount, rect.y + amount, rect.width - amount * 2, rect.height - amount * 2);
    }

    private static Color ComponentColor(string componentType)
    {
        if (componentType == "Button") return new Color32(73, 136, 238, 255);
        if (componentType != null && componentType.Contains("Text")) return new Color32(66, 176, 120, 255);
        if (componentType != null && (componentType.Contains("Image") || componentType.Contains("Sprite")))
            return new Color32(177, 91, 196, 255);
        if (componentType != null && (componentType.Contains("List") || componentType.Contains("Scroll")))
            return new Color32(222, 147, 62, 255);
        if (componentType == "Toggle") return new Color32(63, 181, 181, 255);
        if (componentType == "Slider") return new Color32(216, 105, 134, 255);
        return new Color32(111, 119, 137, 255);
    }
}
