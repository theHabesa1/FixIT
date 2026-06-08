using UnityEngine;
using TMPro;

// Central design palette + fonts. Colors match the Claude design bundle.
public static class Theme
{
    public static Color BG     = Hex("#0D0F1A");
    public static Color BG2    = Hex("#11131F");
    public static Color Panel  = Hex("#161A2B");
    public static Color Panel2 = Hex("#1D2236");
    public static Color Line   = Hex("#2B3150");
    public static Color Green  = Hex("#39FF14");
    public static Color Purple = Hex("#9B6DFF");
    public static Color Amber  = Hex("#FFB300");
    public static Color Red    = Hex("#FF3B3B");
    public static Color White  = Hex("#F3F6FF");
    public static Color Muted  = Hex("#7C84A8");

    public static Color Hex(string h)
    {
        ColorUtility.TryParseHtmlString(h, out Color c);
        return c;
    }

    // Pixel fonts are optional. Drop TMP font assets named "PressStart2P" and
    // "VT323" into a Resources/Fonts folder and they'll be picked up automatically.
    // If absent, the default TMP font is used — the game still runs.
    static TMP_FontAsset _pixel, _term;
    static bool _pixelTried, _termTried;

    public static TMP_FontAsset Pixel
    {
        get
        {
            if (!_pixelTried) { _pixel = Resources.Load<TMP_FontAsset>("Fonts/PressStart2P"); _pixelTried = true; }
            return _pixel;
        }
    }

    public static TMP_FontAsset Term
    {
        get
        {
            if (!_termTried) { _term = Resources.Load<TMP_FontAsset>("Fonts/VT323"); _termTried = true; }
            return _term;
        }
    }
}
