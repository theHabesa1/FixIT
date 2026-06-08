using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// Helpers that build all UI in code. Reference resolution 1280x720 (matches design).
// Everything anchors to screen center; positions are offsets from center (+y = up).
public static class UIFactory
{
    public static Canvas CreateCanvas(string name = "UICanvas", int sortOrder = 0)
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var cv = go.GetComponent<Canvas>();
        cv.renderMode  = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = sortOrder;
        var sc = go.GetComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1280, 720);
        sc.matchWidthOrHeight = 0.5f;
        EnsureEventSystem();
        return cv;
    }

    public static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    static RectTransform Center(GameObject go, float x, float y, float w, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
        return rt;
    }

    public static Image FullBG(Transform parent, Color color)
    {
        var go = new GameObject("BG", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }

    public static Image Panel(Transform parent, float x, float y, float w, float h, Color fill, Color? border = null)
    {
        var go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Center(go, x, y, w, h);
        var img = go.GetComponent<Image>();
        img.color = fill;
        if (border.HasValue)
        {
            var o = go.AddComponent<Outline>();
            o.effectColor = border.Value;
            o.effectDistance = new Vector2(2, -2);
        }
        return img;
    }

    public static TextMeshProUGUI Label(Transform parent, string text, float x, float y, float w, float h,
        float size, Color color, TextAlignmentOptions align = TextAlignmentOptions.Center, bool pixel = false)
    {
        var go = new GameObject("Text", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        var f = pixel ? Theme.Pixel : Theme.Term;
        if (f != null) t.font = f;
        Center(go, x, y, w, h);
        return t;
    }

    public static Button Btn(Transform parent, string label, float x, float y, float w, float h,
        Color accent, Action onClick, bool pixel = true)
    {
        var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Center(go, x, y, w, h);

        var img = go.GetComponent<Image>();
        img.color = Theme.Panel2;

        var o = go.AddComponent<Outline>();
        o.effectColor = accent;
        o.effectDistance = new Vector2(2, -2);

        var btn = go.GetComponent<Button>();
        var cb = btn.colors;
        cb.normalColor      = Theme.Panel2;
        cb.highlightedColor = Color.Lerp(Theme.Panel2, accent, 0.35f);
        cb.pressedColor     = Color.Lerp(Theme.Panel2, Color.black, 0.4f);
        cb.selectedColor    = Theme.Panel2;
        btn.colors = cb;

        Label(go.transform, label, 0, 0, w, h, Mathf.Min(h * 0.45f, 18f), accent,
            TextAlignmentOptions.Center, pixel);

        btn.onClick.AddListener(() => { Sfx.Click(); onClick?.Invoke(); });
        return btn;
    }

    // A solid-colour sprite for world-space objects (player, customers, floor).
    public static Sprite SolidSprite(Color c)
    {
        var tex = new Texture2D(4, 4);
        var px = new Color[16];
        for (int i = 0; i < 16; i++) px[i] = c;
        tex.SetPixels(px);
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
    }

    // Checkerboard tile sprite for the shop floor.
    public static Sprite CheckerSprite(Color a, Color b, int cell = 8)
    {
        int size = cell * 2;
        var tex = new Texture2D(size, size);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                bool dark = (x / cell + y / cell) % 2 == 0;
                tex.SetPixel(x, y, dark ? a : b);
            }
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    // Filled anti-aliased circle (for LEDs / nodes / bulbs).
    static Sprite _circle;
    public static Sprite Circle()
    {
        if (_circle != null) return _circle;
        int s = 64; float r = s * 0.5f;
        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                float a = Mathf.Clamp01(r - d);            // 1px soft edge
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        tex.Apply();
        _circle = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 64);
        return _circle;
    }

    // A circular UI Image at (x,y) with given diameter and colour.
    public static Image Disc(Transform parent, float x, float y, float d, Color color)
    {
        var go = new GameObject("Disc", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Center(go, x, y, d, d);
        var img = go.GetComponent<Image>();
        img.sprite = Circle();
        img.color = color;
        return img;
    }

    // A thin straight line between two points in a centred canvas (for wires / paths).
    public static Image Line(Transform parent, Vector2 a, Vector2 b, float thickness, Color color)
    {
        var go = new GameObject("Line", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        Vector2 mid = (a + b) * 0.5f;
        Vector2 dir = b - a;
        rt.anchoredPosition = mid;
        rt.sizeDelta = new Vector2(dir.magnitude, thickness);
        rt.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        img.raycastTarget = false;
        return img;
    }

    // Standard mini-game header: glowing topic title (top-left) + a level badge.
    public static void Header(Transform parent, string topicLabel, int level, Color accent)
    {
        Label(parent, topicLabel, -500, 322, 420, 32, 16, accent, TextAlignmentOptions.Left, true);
        string[] names = { "NOVICE", "INTERMEDIATE", "EXPERT" };
        Color[] cols   = { Theme.Muted, Theme.Hex("#37B6FF"), Theme.Amber };
        int l = Mathf.Clamp(level, 0, 2);
        var badge = Panel(parent, -300, 322, 150, 26, Theme.Hex("#0A0C14"), cols[l]);
        Label(badge.transform, names[l], 0, 0, 150, 26, 9, cols[l], TextAlignmentOptions.Center, true);
    }

    // Mastery stars (top-right) for a topic.
    public static void MasteryCorner(Transform parent, string topicKey)
    {
        var box = Panel(parent, 500, 322, 200, 30, Theme.Hex("#080A12"), Theme.Line);
        Label(box.transform, "MASTERY", -55, 0, 90, 26, 9, Theme.Muted, TextAlignmentOptions.Center, true);
        int stars = 1;
        if (GameManager.Instance != null)
            stars = (int)GameManager.Instance.knowledge.GetMastery(topicKey) + 1;
        string s = "";
        for (int i = 0; i < 3; i++) s += i < stars ? "*" : "-";
        Label(box.transform, s, 55, 0, 90, 26, 16, Theme.Amber, TextAlignmentOptions.Center);
    }
}
