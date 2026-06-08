using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Full-screen CRT effect (scanlines + vignette) drawn on top of everything, to
// match the retro screen look of the Claude design. Auto-spawned in every scene
// by GameManager. Also shows a small build tag so you can confirm the latest code
// is running.
public class CRTOverlay : MonoBehaviour
{
    public const string Build = "build 3 — retro";

    static Sprite _crt;

    public static void Ensure()
    {
        if (FindFirstObjectByType<CRTOverlay>() != null) return;
        new GameObject("CRTOverlay").AddComponent<CRTOverlay>();
    }

    void Start()
    {
        var go = new GameObject("CRTCanvas", typeof(Canvas), typeof(CanvasScaler));
        go.transform.SetParent(transform, false);
        var cv = go.GetComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 900; // above all gameplay UI
        var sc = go.GetComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1280, 720);

        var imgGO = new GameObject("CRT", typeof(RectTransform), typeof(Image));
        imgGO.transform.SetParent(go.transform, false);
        var rt = imgGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = imgGO.GetComponent<Image>();
        img.sprite = CrtSprite();
        img.color = Color.white;
        img.raycastTarget = false;

        // build tag (confirms new code is live)
        var tagGO = new GameObject("BuildTag", typeof(RectTransform));
        tagGO.transform.SetParent(go.transform, false);
        var tm = tagGO.AddComponent<TextMeshProUGUI>();
        tm.text = "FixIT  " + Build;
        tm.fontSize = 10;
        tm.color = new Color(0.49f, 0.52f, 0.66f, 0.7f);
        tm.alignment = TextAlignmentOptions.BottomRight;
        tm.raycastTarget = false;
        if (Theme.Pixel != null) tm.font = Theme.Pixel;
        var trt = tagGO.GetComponent<RectTransform>();
        trt.anchorMin = trt.anchorMax = trt.pivot = new Vector2(1, 0);
        trt.anchoredPosition = new Vector2(-12, 8);
        trt.sizeDelta = new Vector2(320, 20);
    }

    // Bakes scanlines + a radial vignette into one cached texture.
    static Sprite CrtSprite()
    {
        if (_crt != null) return _crt;
        int w = 320, h = 180; // low-res, stretched (crisp scanlines)
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        Vector2 c = new Vector2(w * 0.5f, h * 0.5f);
        float maxD = c.magnitude;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float a = 0f;
                if (y % 2 == 0) a += 0.10f;                       // scanline
                float d = (new Vector2(x, y) - c).magnitude / maxD;
                a += Mathf.SmoothStep(0f, 0.45f, Mathf.Clamp01((d - 0.55f) / 0.45f)); // vignette
                tex.SetPixel(x, y, new Color(0f, 0f, 0f, Mathf.Clamp01(a)));
            }
        tex.Apply();
        _crt = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100);
        return _crt;
    }
}
