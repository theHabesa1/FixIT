using System.Collections.Generic;
using UnityEngine;

// Builds pixel-art sprites from char-grid maps (ported from the Claude design's
// pixel.jsx). Each sprite is generated once and cached. Use Pixels.Character(kind)
// for people and Pixels.Device(kind) for the device icons in thought bubbles.
public static class Pixels
{
    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    // ---- shared colours ----
    static readonly Color OUTL = Theme.Hex("#0A0C14");
    static readonly Color SK   = Theme.Hex("#F0C088");
    static readonly Color SK2  = Theme.Hex("#D99B66");
    static readonly Color GREY = Theme.Hex("#C9CDD6");

    // ---- people ----
    static readonly string[] techRows = {
        "..oooo..", ".oHHHHo.", ".oHssHo.", ".osssso.", ".oseseo.", ".osssso.",
        "..oBBo..", ".oBBBBo.", "oBBwwBBo", "oBBBBBBo", ".oBBBBo.", ".oP..Po.", ".oP..Po.", ".ob..bo.",
    };
    static readonly string[] studentRows = {
        "..oooo..", ".oHHHHo.", ".oHssHo.", ".osssso.", ".oseseo.", ".osssso.",
        "roBBBBor", "roBGGBor", "roBGGBor", "roBBBBor", ".oBBBBo.", ".oJ..Jo.", ".oJ..Jo.", ".ob..bo.",
    };
    static readonly string[] officeRows = {
        "..oooo..", ".oHHHHo.", ".oHssHo.", ".osssso.", ".oseseo.", ".osssso.",
        "..oWWo..", ".oSWWSo.", "oSStSSo.", "oSStSSo.", ".oSStSo.", ".oS..So.", ".oS..So.", ".ob..bo.",
    };
    static readonly string[] grandmaRows = {
        "..GGGG..", ".GGGGGG.", ".GossoG.", ".osssso.", ".gEgEgo.", ".osssso.",
        "..oPPo..", ".oPPPPo.", "oPPwwPPo", "oPPPPPPo", ".oPPPPo.", ".oM..Mo.", ".oM..Mo.", ".ob..bo.",
    };
    static readonly string[] gamerRows = {
        ".cHHHHc.", "CoHHHHoC", "CoHssHoC", ".osssso.", ".oseseo.", ".osssso.",
        "..oKKo..", ".oKKKKo.", "oKKHHKKo", "oKKKKKKo", ".oKKKKo.", ".oG..Go.", ".oG..Go.", ".ob..bo.",
    };

    static Dictionary<char, Color> P(params (char, string)[] entries)
    {
        var d = new Dictionary<char, Color> { { 'o', OUTL }, { 's', SK }, { 'e', OUTL } };
        foreach (var (ch, hex) in entries) d[ch] = Theme.Hex(hex);
        return d;
    }

    public static Sprite Character(string kind)
    {
        string key = "char_" + kind;
        if (cache.TryGetValue(key, out var c)) return c;
        string[] rows; Dictionary<char, Color> pal;
        switch (kind)
        {
            case "student":
                rows = studentRows; pal = P(('H', "#5A3A22"), ('B', "#D24F4F"), ('G', "#FFD23F"), ('J', "#2A3B66"), ('b', "#15181F"), ('r', "#7A4A2A")); break;
            case "office":
                rows = officeRows; pal = P(('H', "#2A2018"), ('S', "#33384A"), ('W', "#EEF1FF"), ('t', "#C0392B"), ('b', "#15181F")); break;
            case "grandma":
                rows = grandmaRows; pal = P(('G', "#C9CDD6"), ('E', "#DDFFFF"), ('g', "#AABBCC"), ('P', "#A85FB0"), ('w', "#D99B66"), ('M', "#5A4A6A"), ('b', "#15181F"));
                pal['s'] = SK2; break;
            case "gamer":
                rows = gamerRows; pal = P(('H', "#222633"), ('K', "#3A2E5A"), ('C', "#39FF14"), ('c', "#39FF14"), ('G', "#1C1F2B"), ('b', "#15181F")); break;
            default: // tech
                rows = techRows; pal = P(('H', "#5A4A3A"), ('B', "#2F6BD6"), ('w', "#F0C088"), ('P', "#23304D"), ('b', "#15181F")); break;
        }
        var sp = Build(rows, pal, new Vector2(0.5f, 0f)); // pivot at feet
        cache[key] = sp;
        return sp;
    }

    // ---- device icons ----
    public static Sprite Device(string kind)
    {
        string key = "dev_" + kind;
        if (cache.TryGetValue(key, out var c)) return c;
        string[] rows; Dictionary<char, Color> pal;
        switch (kind)
        {
            case "phone":
                rows = new[] { ".dddd.", "dCCCCd", "dCxxCd", "dCxxCd", "dCCCCd", "dCCCCd", ".dddd." };
                pal = HexMap(('d', "#2A2F40"), ('C', "#0E1422"), ('x', "#FF5A5A")); break;
            case "pc":
                rows = new[] { "ppppp.", "pSSSp.", "pSxSp.", "pSSSp.", "ppppp.", ".b.b..", "bbbbb." };
                pal = HexMap(('p', "#B9A88A"), ('S', "#26303F"), ('x', "#FF5A5A"), ('b', "#7A6F55")); break;
            case "tower":
                rows = new[] { ".tttt", "tGGGGt", "tGrGGt", "tGGGgt", "tGbGGt", "tGGGGt", ".tttt" };
                pal = HexMap(('t', "#1C1F2B"), ('G', "#11141D"), ('r', "#FF4FD8"), ('g', "#39FF14"), ('b', "#37B6FF")); break;
            default: // laptop
                rows = new[] { "ssssssss", "sCCCCCCs", "sCxCCxCs", "sCCCCCCs", "ssssssss", "gggggggg", "gggggggg" };
                pal = HexMap(('s', "#9AA3B8"), ('C', "#1A2233"), ('x', "#FF5A5A"), ('g', "#5A6580")); break;
        }
        var sp = Build(rows, pal, new Vector2(0.5f, 0.5f));
        cache[key] = sp;
        return sp;
    }

    static Dictionary<char, Color> HexMap(params (char, string)[] entries)
    {
        var d = new Dictionary<char, Color>();
        foreach (var (ch, hex) in entries) d[ch] = Theme.Hex(hex);
        return d;
    }

    // ---- furniture / props (same pixel-art treatment as the characters) ----
    public static Sprite Furniture(string name)
    {
        string key = "obj_" + name;
        if (cache.TryGetValue(key, out var c)) return c;
        string[] rows; Dictionary<char, Color> pal;
        switch (name)
        {
            case "shelf":
                rows = new[] {
                    "WWWWWWWWWWWWWWWW",
                    "WiRRiGGiBBiYYiiW",
                    "WiRRiGGiBBiYYiiW",
                    "WWWWWWWWWWWWWWWW",
                    "WiCCiMMiRRiGGiiW",
                    "WiCCiMMiRRiGGiiW",
                    "WWWWWWWWWWWWWWWW",
                    "WiBBiYYiGGiCCiiW",
                    "WiBBiYYiGGiCCiiW",
                    "WWWWWWWWWWWWWWWW",
                };
                pal = HexMap(('W', "#6B4A2A"), ('i', "#171019"), ('R', "#D24F4F"), ('G', "#39FF14"),
                             ('B', "#37B6FF"), ('Y', "#FFB300"), ('C', "#9B6DFF"), ('M', "#FF8FD0"));
                break;
            case "plant":
                rows = new[] {
                    "...GG...", "..gGGg..", ".gggggg.", "ggGggGgg",
                    ".gggggg.", "..gggg..", "...gg...", "...gg...",
                    ".pppppp.", ".pPPPPp.", ".pPPPPp.", "..pppp..",
                };
                pal = HexMap(('g', "#2E8B57"), ('G', "#3CB371"), ('p', "#B5651D"), ('P', "#8A4B15"));
                break;
            case "crate":
                rows = new[] {
                    "wwwwwwww", "wBBBBBBw", "wBwwwwBw", "wBwwwwBw",
                    "wBwwwwBw", "wBwwwwBw", "wBBBBBBw", "wwwwwwww",
                };
                pal = HexMap(('w', "#6B4A2A"), ('B', "#8A6540"));
                break;
            case "ram":
                rows = new[] {
                    "GGGGGGGGGGGGGGGG",
                    "GLLLLLLLLLLLLLLG",
                    "GKKiKKiKKiKKiKKG",
                    "GKKiKKiKKiKKiKKG",
                    "GGGGGGGGGGGGGGGG",
                    "aaaaaaaiaaaaaaaa",
                };
                pal = HexMap(('G', "#15512B"), ('L', "#1F7A3A"), ('K', "#0E1422"),
                             ('a', "#FFB300"), ('i', "#15512B"));
                break;
            default: // "desk" — wooden counter with plank seams
                rows = new[] {
                    "wwwwwwwwwwwwwwww",
                    "WWWWWWWWWWWWWWWW",
                    "WdWWWWWdWWWWWdWW",
                    "WWWWWWWWWWWWWWWW",
                    "WWWdWWWWWdWWWWWd",
                    "WWWWWWWWWWWWWWWW",
                    "wwwwwwwwwwwwwwww",
                    "DDDDDDDDDDDDDDDD",
                };
                pal = HexMap(('W', "#7A5530"), ('w', "#8A6540"), ('d', "#5E3F22"), ('D', "#3A2614"));
                break;
        }
        var sp = Build(rows, pal, new Vector2(0.5f, 0.5f));
        cache[key] = sp;
        return sp;
    }

    // Renders a char grid (top row first) into a point-filtered sprite.
    static Sprite Build(string[] rows, Dictionary<char, Color> palette, Vector2 pivot, float ppu = 12f)
    {
        int h = rows.Length, w = 0;
        foreach (var r in rows) w = Mathf.Max(w, r.Length);
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        var clear = new Color(0, 0, 0, 0);
        for (int ty = 0; ty < h; ty++)
        {
            string row = rows[h - 1 - ty]; // texture y is bottom-up; row 0 is the top
            for (int x = 0; x < w; x++)
            {
                char ch = x < row.Length ? row[x] : ' ';
                tex.SetPixel(x, ty, palette.TryGetValue(ch, out var col) ? col : clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), pivot, ppu);
    }
}
