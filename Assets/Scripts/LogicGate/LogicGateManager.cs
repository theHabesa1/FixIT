using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Logic Gate mini-game (matches the Claude design): input switches on the left,
// gate chips wired together with glowing wires, a round output LED on the right.
// Toggle inputs so the live output matches the TARGET, then SUBMIT.
public class LogicGateManager : MonoBehaviour
{
    GatePuzzleGen.Puzzle puzzle;
    readonly Dictionary<int, bool> inputValues = new Dictionary<int, bool>();
    readonly Dictionary<int, Vector2> pos = new Dictionary<int, Vector2>();
    int attempts = 3;
    bool done;

    readonly List<(Image img, int src)> wires = new List<(Image, int)>();
    readonly Dictionary<int, Outline> gateOutline = new Dictionary<int, Outline>();
    readonly Dictionary<int, TextMeshProUGUI> gateLabel = new Dictionary<int, TextMeshProUGUI>();
    readonly Dictionary<int, TextMeshProUGUI> inputLabel = new Dictionary<int, TextMeshProUGUI>();
    readonly Dictionary<int, Outline> inputOutline = new Dictionary<int, Outline>();

    Image ledGlow, led;
    TextMeshProUGUI ledText, attemptsText, matchText;
    static readonly Color On = Theme.Green, Off = Theme.Hex("#41496E"), Dim = Theme.Hex("#2A3352");

    void Start()
    {
        int level = GameManager.Instance != null ? GameManager.Instance.currentCustomerLevel : 0;
        puzzle = GatePuzzleGen.Generate(level);
        foreach (int id in puzzle.inputIds) inputValues[id] = false;

        var cv = UIFactory.CreateCanvas("LogicCanvas");
        UIFactory.FullBG(cv.transform, Theme.Hex("#0C0E18"));
        UIFactory.Header(cv.transform, "LOGIC GATES", level, Theme.Purple);
        UIFactory.MasteryCorner(cv.transform, "logic");

        // target box
        var target = UIFactory.Panel(cv.transform, 0, 268, 360, 50, Theme.Hex("#0A0C14"), Theme.Green);
        UIFactory.Label(target.transform, "TARGET OUTPUT:", -40, 0, 240, 40, 13, Theme.Green, TextAlignmentOptions.Center, true);
        UIFactory.Label(target.transform, puzzle.targetOutput ? "1" : "0", 120, 0, 60, 40, 22, Theme.White, TextAlignmentOptions.Center, true);

        attemptsText = UIFactory.Label(cv.transform, "", 0, 222, 400, 24, 10, Theme.Amber, TextAlignmentOptions.Center, true);
        matchText = UIFactory.Label(cv.transform, "", 0, -250, 400, 24, 11, Theme.Green, TextAlignmentOptions.Center, true);

        Layout();
        BuildWires(cv.transform);
        BuildNodes(cv.transform);
        BuildLED(cv.transform);

        UIFactory.Btn(cv.transform, "SUBMIT REPAIR", 0, -300, 260, 50, Theme.Green, OnSubmit);
        UIFactory.Btn(cv.transform, "R: SHOP", 540, -300, 150, 44, Theme.Muted, OnReturn);

        Refresh();
    }

    // ---- layout: inputs on the left, gates in dependency layers ----
    void Layout()
    {
        var inputs = puzzle.inputIds;
        float top = 150, bottom = -170;
        for (int i = 0; i < inputs.Count; i++)
        {
            float t = inputs.Count == 1 ? 0.5f : i / (float)(inputs.Count - 1);
            pos[inputs[i]] = new Vector2(-520, Mathf.Lerp(top, bottom, t));
        }

        // depth of each gate node
        var depth = new Dictionary<int, int>();
        int Depth(int id)
        {
            var n = puzzle.nodes.Find(x => x.id == id);
            if (n.type == GatePuzzleGen.NodeType.Input) return 0;
            if (depth.ContainsKey(id)) return depth[id];
            int d = 0;
            foreach (int s in n.inputs) d = Mathf.Max(d, Depth(s));
            return depth[id] = d + 1;
        }
        var gates = puzzle.nodes.FindAll(n => n.type == GatePuzzleGen.NodeType.Gate);
        foreach (var g in gates) Depth(g.id);
        int maxD = 1;
        foreach (var kv in depth) maxD = Mathf.Max(maxD, kv.Value);

        var byLayer = new Dictionary<int, List<int>>();
        foreach (var g in gates)
        {
            int d = depth[g.id];
            if (!byLayer.ContainsKey(d)) byLayer[d] = new List<int>();
            byLayer[d].Add(g.id);
        }
        foreach (var kv in byLayer)
        {
            float x = Mathf.Lerp(-250, 330, maxD == 1 ? 0.5f : (kv.Key - 1) / (float)(maxD - 1));
            var list = kv.Value;
            for (int i = 0; i < list.Count; i++)
            {
                float t = list.Count == 1 ? 0.5f : i / (float)(list.Count - 1);
                pos[list[i]] = new Vector2(x, Mathf.Lerp(120, -150, t));
            }
        }
    }

    void BuildWires(Transform parent)
    {
        foreach (var n in puzzle.nodes)
        {
            if (n.type != GatePuzzleGen.NodeType.Gate) continue;
            foreach (int src in n.inputs)
            {
                if (!pos.ContainsKey(src) || !pos.ContainsKey(n.id)) continue;
                var img = UIFactory.Line(parent, pos[src] + new Vector2(48, 0), pos[n.id] - new Vector2(48, 0), 4, Dim);
                wires.Add((img, src));
            }
        }
    }

    void BuildNodes(Transform parent)
    {
        // input switches
        int i = 0;
        foreach (int id in puzzle.inputIds)
        {
            Vector2 p = pos[id];
            char name = (char)('A' + i++);
            var sw = UIFactory.Panel(parent, p.x, p.y, 84, 60, Theme.Hex("#0E1422"), Theme.Line);
            var outline = sw.GetComponent<Outline>();
            inputOutline[id] = outline;
            UIFactory.Label(sw.transform, name.ToString(), 0, 16, 80, 20, 9, Theme.Muted, TextAlignmentOptions.Center, true);
            var vlbl = UIFactory.Label(sw.transform, "0", 0, -8, 80, 30, 20, Off, TextAlignmentOptions.Center, true);
            inputLabel[id] = vlbl;

            var btn = sw.gameObject.AddComponent<Button>();
            int cid = id;
            btn.onClick.AddListener(() =>
            {
                if (done) return;
                Sfx.Toggle();
                inputValues[cid] = !inputValues[cid];
                Refresh();
            });
        }

        // gate chips
        foreach (var n in puzzle.nodes)
        {
            if (n.type != GatePuzzleGen.NodeType.Gate) continue;
            Vector2 p = pos[n.id];
            var chip = UIFactory.Panel(parent, p.x, p.y, 92, 64, Theme.Hex("#0E1422"), Off);
            gateOutline[n.id] = chip.GetComponent<Outline>();
            var lbl = UIFactory.Label(chip.transform, n.gateType.ToString(), 0, 0, 88, 40, 13, Off, TextAlignmentOptions.Center, true);
            gateLabel[n.id] = lbl;
        }
    }

    void BuildLED(Transform parent)
    {
        Vector2 lp = pos.ContainsKey(puzzle.outputId) ? pos[puzzle.outputId] : Vector2.zero;
        // output node follows its single input gate's y
        var outNode = puzzle.nodes.Find(x => x.id == puzzle.outputId);
        if (outNode != null && outNode.inputs.Count > 0 && pos.ContainsKey(outNode.inputs[0]))
            lp = pos[outNode.inputs[0]];
        Vector2 ledPos = new Vector2(540, lp.y);

        // wire to LED
        var w = UIFactory.Line(parent, lp + new Vector2(48, 0), ledPos - new Vector2(40, 0), 4, Dim);
        if (outNode != null && outNode.inputs.Count > 0) wires.Add((w, outNode.inputs[0]));

        UIFactory.Label(parent, "OUTPUT", ledPos.x, ledPos.y + 55, 160, 24, 11, Theme.White, TextAlignmentOptions.Center, true);
        ledGlow = UIFactory.Disc(parent, ledPos.x, ledPos.y, 96, new Color(1, 0, 0, 0.25f));
        led = UIFactory.Disc(parent, ledPos.x, ledPos.y, 64, Theme.Red);
        ledText = UIFactory.Label(parent, "0", ledPos.x, ledPos.y, 64, 40, 20, Theme.Hex("#0A0C14"), TextAlignmentOptions.Center, true);
    }

    Dictionary<int, bool> EvalAll()
    {
        var r = new Dictionary<int, bool>(inputValues);
        foreach (var n in puzzle.nodes)
        {
            if (n.type == GatePuzzleGen.NodeType.Input) continue;
            bool a = n.inputs.Count > 0 && r.TryGetValue(n.inputs[0], out var va) && va;
            bool b = n.inputs.Count > 1 && r.TryGetValue(n.inputs[1], out var vb) && vb;
            r[n.id] = n.type == GatePuzzleGen.NodeType.Output ? a : Gate.Eval(n.gateType, a, b);
        }
        return r;
    }

    void Refresh()
    {
        var v = EvalAll();

        foreach (int id in puzzle.inputIds)
        {
            bool on = inputValues[id];
            if (inputLabel[id]) { inputLabel[id].text = on ? "1" : "0"; inputLabel[id].color = on ? On : Off; }
            if (inputOutline[id]) inputOutline[id].effectColor = on ? On : Theme.Line;
        }
        foreach (var kv in gateOutline)
        {
            bool on = v.TryGetValue(kv.Key, out var b) && b;
            if (kv.Value) kv.Value.effectColor = on ? On : Off;
            if (gateLabel[kv.Key]) gateLabel[kv.Key].color = on ? On : Off;
        }
        foreach (var (img, src) in wires)
            if (img) img.color = v.TryGetValue(src, out var b) && b ? On : Dim;

        bool output = v.TryGetValue(puzzle.outputId, out var ov) && ov;
        if (led) led.color = output ? On : Theme.Red;
        if (ledGlow) ledGlow.color = output ? new Color(0.22f, 1f, 0.08f, 0.3f) : new Color(1, 0, 0, 0.25f);
        if (ledText) ledText.text = output ? "1" : "0";

        bool matched = output == puzzle.targetOutput;
        if (matchText) matchText.text = matched && !done ? "MATCH  -  press SUBMIT" : "";
        if (attemptsText) attemptsText.text = "ATTEMPTS:  " + Diamonds(attempts);
    }

    static string Diamonds(int n)
    {
        string s = "";
        for (int i = 0; i < 3; i++) s += i < n ? "*" : "-";
        return s;
    }

    void OnSubmit()
    {
        if (done) return;
        var v = EvalAll();
        bool output = v.TryGetValue(puzzle.outputId, out var ov) && ov;
        if (output == puzzle.targetOutput)
        {
            done = true;
            GameOverScreen.Display(true);
        }
        else
        {
            attempts--;
            Sfx.Wrong();
            StartCoroutine(FlashRed());
            Refresh();
            if (attempts <= 0) { done = true; GameOverScreen.Display(false); }
        }
    }

    IEnumerator FlashRed()
    {
        for (int i = 0; i < 3; i++)
        {
            if (led) led.color = Theme.Red;
            if (ledGlow) ledGlow.color = new Color(1, 0, 0, 0.5f);
            yield return new WaitForSeconds(0.08f);
            Refresh();
            yield return new WaitForSeconds(0.08f);
        }
    }

    void OnReturn() => SceneLoader.ReturnToShop();
    void Update() { if (Input.GetKeyDown(KeyCode.R)) OnReturn(); }
}
