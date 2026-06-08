using UnityEngine;
using TMPro;

// Self-building wall-mounted mastery board, centered on the back wall like the
// design: a wide horizontal panel with one column per topic (name / stars / level).
public class MasteryBoard : MonoBehaviour
{
    static readonly (string key, string label)[] Topics =
    {
        ("logic",    "LOGIC"),
        ("binary",   "BINARY"),
        ("circuits", "CIRCUITS"),
        ("ram",      "MEMORY"),
    };

    static readonly string[] LevelNames  = { "NOVICE", "INTERMEDIATE", "EXPERT" };
    static readonly Color[]   LevelColors = { Theme.Muted, default, Theme.Amber };

    TextMeshProUGUI[] starLabels;
    TextMeshProUGUI[] levelLabels;

    void Start()
    {
        LevelColors[1] = Theme.Hex("#37B6FF");

        var cv = UIFactory.CreateCanvas("MasteryCanvas", 10);

        const float W = 760, H = 92;
        float px = 150, py = 258;
        var panel = UIFactory.Panel(cv.transform, px, py, W, H, Theme.Panel, Theme.Line);

        // purple title tab on the top-left edge
        var tab = UIFactory.Panel(panel.transform, -W / 2 + 70, H / 2 + 2, 140, 20, Theme.Purple);
        UIFactory.Label(tab.transform, "MASTERY BOARD", 0, 0, 140, 20, 8,
            Theme.Hex("#10121E"), TextAlignmentOptions.Center, true);

        starLabels  = new TextMeshProUGUI[Topics.Length];
        levelLabels = new TextMeshProUGUI[Topics.Length];
        float colW = W / Topics.Length;
        for (int i = 0; i < Topics.Length; i++)
        {
            float cx = -W / 2 + colW * (i + 0.5f);
            UIFactory.Label(panel.transform, Topics[i].label, cx, 22, colW - 8, 20, 9,
                Theme.White, TextAlignmentOptions.Center, true);
            starLabels[i] = UIFactory.Label(panel.transform, "---", cx, 0, colW - 8, 22, 16,
                Theme.Amber, TextAlignmentOptions.Center);
            levelLabels[i] = UIFactory.Label(panel.transform, "NOVICE", cx, -24, colW - 8, 18, 7,
                Theme.Muted, TextAlignmentOptions.Center, true);

            // divider between columns
            if (i < Topics.Length - 1)
                UIFactory.Panel(panel.transform, -W / 2 + colW * (i + 1), 0, 2, H - 16, Theme.Hex("#20243A"));
        }
        Refresh();
    }

    void Update()
    {
        if (Time.frameCount % 30 == 0) Refresh();
    }

    void Refresh()
    {
        if (GameManager.Instance == null || starLabels == null) return;
        for (int i = 0; i < Topics.Length; i++)
        {
            var lvl = GameManager.Instance.knowledge.GetMastery(Topics[i].key);
            int li = Mathf.Clamp((int)lvl, 0, 2);
            if (starLabels[i]) starLabels[i].text = StarString(li + 1);
            if (levelLabels[i]) { levelLabels[i].text = LevelNames[li]; levelLabels[i].color = LevelColors[li]; }
        }
    }

    static string StarString(int filled)
    {
        string r = "";
        for (int i = 0; i < 3; i++) r += i < filled ? "*" : "-";
        return r;
    }
}
