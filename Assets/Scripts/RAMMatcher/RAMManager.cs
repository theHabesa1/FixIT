using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// RAM Slot Matcher mini-game (matches the Claude design): tap a stick in the tray,
// then tap a slot to install it. Mirror the channels (A1=B1, A2=B2) by TYPE/SIZE/
// SPEED and fill all 4 slots, then SUBMIT.
public class RAMManager : MonoBehaviour
{
    class Stick
    {
        public string type; public int size, speed;
        public string Key => type + "_" + size + "_" + speed;
        public RectTransform rt; public Outline border;
    }

    static readonly string[] SlotIds = { "A1", "A2", "B1", "B2" };

    Canvas cv;
    readonly List<Stick> sticks = new List<Stick>();
    readonly Dictionary<string, Stick> slotOf = new Dictionary<string, Stick>(); // slotId -> stick
    readonly Dictionary<string, Vector2> slotCenter = new Dictionary<string, Vector2>();
    readonly Dictionary<string, Image> slotPanel = new Dictionary<string, Image>();
    Stick selected;
    Dictionary<string, bool> marks;
    int attempts = 3;
    bool done;

    TextMeshProUGUI feedbackText, attemptsText;

    void Start()
    {
        int level = GameManager.Instance != null ? GameManager.Instance.currentCustomerLevel : 0;

        cv = UIFactory.CreateCanvas("RamCanvas");
        UIFactory.FullBG(cv.transform, Theme.Hex("#07140D"));
        UIFactory.Header(cv.transform, "MEMORY", level, Theme.Green);
        UIFactory.MasteryCorner(cv.transform, "ram");

        attemptsText = UIFactory.Label(cv.transform, "", -300, 250, 300, 24, 10, Theme.Amber, TextAlignmentOptions.Left, true);

        BuildSlots();
        BuildReferenceCard();
        BuildSticks(level);

        feedbackText = UIFactory.Label(cv.transform, "Tap a stick, then tap a slot.",
            0, -250, 1000, 30, 15, Theme.Muted);
        UIFactory.Btn(cv.transform, "SUBMIT BUILD", 0, -300, 240, 50, Theme.Green, OnSubmit);
        UIFactory.Btn(cv.transform, "R: SHOP", 540, -300, 150, 44, Theme.Muted, OnReturn);

        Relayout();
        RefreshAttempts();
    }

    void BuildSlots()
    {
        // motherboard backing behind the DIMM slots
        var board = UIFactory.Panel(cv.transform, 0, 100, 780, 250, Theme.Hex("#0C2A18"), Theme.Hex("#1C4A30"));
        UIFactory.Label(board.transform, "MOTHERBOARD", -300, 102, 220, 20, 9,
            Theme.Hex("#2E6E45"), TextAlignmentOptions.Left, true);

        float[] xs = { -250, -90, 90, 250 };
        for (int i = 0; i < 4; i++)
        {
            string id = SlotIds[i];
            string ch = id.Substring(0, 1);
            float x = xs[i], y = 100;
            slotCenter[id] = new Vector2(x, y);

            UIFactory.Label(cv.transform, id, x, y + 122, 120, 24, 12, Theme.White, TextAlignmentOptions.Center, true);
            var panel = UIFactory.Panel(cv.transform, x, y, 128, 200, Theme.Hex("#06140C"), Theme.Hex("#1C4A30"));
            slotPanel[id] = panel;

            // DIMM retention clips (top corners)
            var l1 = UIFactory.Disc(panel.transform, -52, 92, 14, Theme.Hex("#C9CDD6")); l1.raycastTarget = false;
            var l2 = UIFactory.Disc(panel.transform, 52, 92, 14, Theme.Hex("#C9CDD6")); l2.raycastTarget = false;

            UIFactory.Label(panel.transform, "EMPTY", 0, 0, 120, 24, 9, Theme.Hex("#2E4A38"), TextAlignmentOptions.Center, true);
            UIFactory.Label(cv.transform, "CHANNEL " + ch, x, y - 122, 120, 20, 8,
                ch == "A" ? Theme.Hex("#37B6FF") : Theme.Hex("#FF8FD0"), TextAlignmentOptions.Center, true);

            string cid = id;
            var btn = panel.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() => OnSlotClicked(cid));
        }
    }

    void BuildReferenceCard()
    {
        var card = UIFactory.Panel(cv.transform, 510, 90, 230, 250, Theme.Panel, Theme.Purple);
        UIFactory.Label(card.transform, "DUAL-CHANNEL", 0, 102, 210, 24, 11, Theme.Purple, TextAlignmentOptions.Center, true);
        string[,] rows =
        {
            { "Channel A", "A1, A2" },
            { "Channel B", "B1, B2" },
            { "Mirror",    "A1=B1  A2=B2" },
            { "Match by",  "TYPE/SIZE/SPEED" },
            { "Fill",      "all 4 slots" },
        };
        for (int i = 0; i < rows.GetLength(0); i++)
        {
            float y = 60 - i * 34;
            UIFactory.Label(card.transform, rows[i, 0], -8, y, 200, 22, 13, Theme.Muted, TextAlignmentOptions.Left);
            UIFactory.Label(card.transform, rows[i, 1], 8, y, 200, 22, 13, Theme.White, TextAlignmentOptions.Right);
        }
        UIFactory.Label(card.transform, "Tap a stick, then a slot to place it.",
            0, -108, 210, 40, 12, Theme.Muted, TextAlignmentOptions.Center);
    }

    void BuildSticks(int level)
    {
        var specs = new List<(string, int, int)>();
        if (level == 0)
            specs.AddRange(new[] { ("DDR4", 8, 3200), ("DDR4", 8, 3200), ("DDR4", 16, 3600), ("DDR4", 16, 3600) });
        else if (level == 1)
            specs.AddRange(new[] { ("DDR4", 8, 3200), ("DDR4", 8, 3200), ("DDR4", 8, 2666), ("DDR4", 8, 2666) });
        else
            specs.AddRange(new[] { ("DDR4", 16, 3200), ("DDR4", 16, 3200), ("DDR4", 16, 3600), ("DDR4", 16, 3600), ("DDR5", 16, 3200) });

        for (int i = specs.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (specs[i], specs[j]) = (specs[j], specs[i]); }

        foreach (var s in specs)
        {
            var st = new Stick { type = s.Item1, size = s.Item2, speed = s.Item3 };
            var chip = UIFactory.Panel(cv.transform, 0, 0, 130, 58, Color.white, Theme.Hex("#1C4A30"));
            chip.sprite = Pixels.Furniture("ram"); // pixel-art RAM module
            chip.color = Color.white;
            st.rt = chip.rectTransform;
            st.border = chip.GetComponent<Outline>();

            // dark plate so the spec text stays readable over the PCB
            var plate = UIFactory.Panel(chip.transform, 0, 6, 108, 30, new Color(0f, 0f, 0f, 0.55f));
            plate.raycastTarget = false;
            UIFactory.Label(plate.transform, st.type, 0, 8, 104, 18, 10, Theme.Hex("#BFFFB0"), TextAlignmentOptions.Center, true);
            UIFactory.Label(plate.transform, st.size + "GB  " + st.speed, 0, -8, 104, 18, 9, Theme.White, TextAlignmentOptions.Center, true);

            var btn = chip.gameObject.AddComponent<Button>();
            var captured = st;
            btn.onClick.AddListener(() => OnStickClicked(captured));
            sticks.Add(st);
        }
    }

    string SlotOfStick(Stick s)
    {
        foreach (var kv in slotOf) if (kv.Value == s) return kv.Key;
        return null;
    }

    void OnStickClicked(Stick s)
    {
        if (done) return;
        marks = null;
        selected = (selected == s) ? null : s;
        Sfx.Click();
        Relayout();
    }

    void OnSlotClicked(string slotId)
    {
        if (done) return;
        marks = null;
        slotOf.TryGetValue(slotId, out var occupant);

        if (selected != null)
        {
            // remove selected from its current slot (if any)
            string from = SlotOfStick(selected);
            if (from != null) slotOf.Remove(from);
            // occupant (if different) goes back to tray automatically (not in slotOf)
            if (occupant != null && occupant != selected) { /* freed */ }
            slotOf[slotId] = selected;
            selected = null;
            Sfx.Click();
        }
        else if (occupant != null)
        {
            selected = occupant; // pick up
            Sfx.Click();
        }
        Relayout();
    }

    void Relayout()
    {
        // tray = sticks not in any slot, laid out along the bottom
        var tray = new List<Stick>();
        foreach (var s in sticks) if (SlotOfStick(s) == null) tray.Add(s);
        float startX = -((tray.Count - 1) * 140) * 0.5f;
        for (int i = 0; i < tray.Count; i++)
            tray[i].rt.anchoredPosition = new Vector2(startX + i * 140, -150);

        foreach (var kv in slotOf)
            kv.Value.rt.anchoredPosition = slotCenter[kv.Key];

        // borders + slot panel states
        foreach (var s in sticks)
            if (s.border) s.border.effectColor = (s == selected) ? Theme.Green : (SlotOfStick(s) != null ? Theme.Purple : Theme.Hex("#1C4A30"));
        foreach (var id in SlotIds)
        {
            Color c = Theme.Line;
            if (marks != null) c = marks[id] ? Theme.Green : Theme.Red;
            else if (slotOf.ContainsKey(id)) c = Theme.Purple;
            var o = slotPanel[id].GetComponent<Outline>();
            if (o) o.effectColor = c;
        }
    }

    void OnSubmit()
    {
        if (done) return;
        foreach (var id in SlotIds)
            if (!slotOf.ContainsKey(id)) { feedbackText.text = "Fill all 4 slots first!"; feedbackText.color = Theme.Amber; return; }

        bool row1 = slotOf["A1"].Key == slotOf["B1"].Key;
        bool row2 = slotOf["A2"].Key == slotOf["B2"].Key;
        marks = new Dictionary<string, bool> { { "A1", row1 }, { "B1", row1 }, { "A2", row2 }, { "B2", row2 } };
        Relayout();

        if (row1 && row2)
        {
            done = true;
            feedbackText.text = "PERFECT INSTALLATION!";
            feedbackText.color = Theme.Green;
            GameOverScreen.Display(true);
        }
        else
        {
            attempts--;
            Sfx.Wrong();
            RefreshAttempts();
            if (attempts <= 0)
            {
                done = true;
                feedbackText.text = "WRONG CONFIGURATION!";
                feedbackText.color = Theme.Red;
                GameOverScreen.Display(false);
            }
            else
            {
                feedbackText.text = "Not matched - " + attempts + " attempt(s) left.";
                feedbackText.color = Theme.Amber;
                StartCoroutine(ClearMarks());
            }
        }
    }

    IEnumerator ClearMarks()
    {
        yield return new WaitForSeconds(0.9f);
        if (!done) { marks = null; Relayout(); }
    }

    void RefreshAttempts()
    {
        string s = "";
        for (int i = 0; i < 3; i++) s += i < attempts ? "*" : "-";
        if (attemptsText) attemptsText.text = "ATTEMPTS:  " + s;
    }

    void OnReturn() => SceneLoader.ReturnToShop();
    void Update() { if (Input.GetKeyDown(KeyCode.R)) OnReturn(); }
}
