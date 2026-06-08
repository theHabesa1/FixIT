using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Circuit Path Tracer mini-game (matches the Claude design): click adjacent nodes
// from the BATTERY to the BULB, routing around blocked components, before the
// power drains. The path is drawn as a glowing blue trace.
public class CircuitManager : MonoBehaviour
{
    Node[,] grid;
    Node source, dest;
    readonly List<Node> drawnPath = new List<Node>();
    bool done;
    int w, h, obstacleCount;
    float time, maxTime;

    Transform lineLayer;
    readonly List<GameObject> pathLines = new List<GameObject>();
    TextMeshProUGUI feedbackText, powerLabel;
    Image powerFill, bulb, bulbGlow;

    void Start()
    {
        int level = GameManager.Instance != null ? GameManager.Instance.currentCustomerLevel : 0;
        (w, h, obstacleCount, maxTime) = level == 0 ? (5, 4, 3, 40f)
                                        : level == 1 ? (6, 5, 7, 38f)
                                                     : (7, 5, 11, 34f);
        time = maxTime;

        var cv = UIFactory.CreateCanvas("CircuitCanvas");
        UIFactory.FullBG(cv.transform, Theme.Hex("#08160E"));
        UIFactory.Header(cv.transform, "CIRCUITS", level, Theme.Green);
        UIFactory.MasteryCorner(cv.transform, "circuits");

        BuildPowerBar(cv.transform);

        lineLayer = UIFactory.Panel(cv.transform, 0, 0, 1, 1, new Color(0, 0, 0, 0)).transform;
        ((Image)lineLayer.GetComponent<Image>()).raycastTarget = false;

        var gridParent = UIFactory.Panel(cv.transform, 0, 0, 1, 1, new Color(0, 0, 0, 0));
        gridParent.raycastTarget = false;
        BuildGrid(gridParent.transform);
        BuildEndpoints(cv.transform);

        feedbackText = UIFactory.Label(cv.transform, "Click adjacent nodes from BATTERY to BULB.",
            0, -250, 1000, 30, 15, Theme.Muted);
        UIFactory.Btn(cv.transform, "RESET PATH", -120, -300, 180, 46, Theme.Amber, OnReset);
        UIFactory.Btn(cv.transform, "R: SHOP", 120, -300, 160, 46, Theme.Muted, OnReturn);
    }

    void BuildPowerBar(Transform parent)
    {
        powerLabel = UIFactory.Label(parent, "POWER DRAINING", 0, 240, 360, 22, 9, Theme.Muted, TextAlignmentOptions.Center, true);
        var bg = UIFactory.Panel(parent, 0, 218, 360, 16, Theme.Hex("#0A140D"), Theme.Line);
        powerFill = UIFactory.Panel(bg.transform, 0, 0, 356, 12, Theme.Amber);
        powerFill.type = Image.Type.Filled;
        powerFill.fillMethod = Image.FillMethod.Horizontal;
        powerFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        powerFill.fillAmount = 1f;
    }

    void BuildGrid(Transform parent)
    {
        grid = new Node[w, h];
        float cell = Mathf.Min(78f, 900f / w, 360f / h);
        float dotSize = cell * 0.28f;
        float ox = -(w - 1) * cell * 0.5f;
        float oy = -(h - 1) * cell * 0.5f - 10;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Vector2 p = new Vector2(ox + x * cell, oy + y * cell);
                // transparent hit area (large) with a small visible dot inside
                var hitImg = UIFactory.Panel(parent, p.x, p.y, cell * 0.7f, cell * 0.7f, new Color(0, 0, 0, 0));
                var btn = hitImg.gameObject.AddComponent<Button>();
                var disc = UIFactory.Disc(hitImg.transform, 0, 0, dotSize, Theme.Hex("#AAB4C8"));
                disc.raycastTarget = false;

                var n = hitImg.gameObject.AddComponent<Node>();
                n.gridX = x; n.gridY = y; n.image = disc; n.uiPos = p;
                grid[x, y] = n;
                Node captured = n;
                btn.onClick.AddListener(() => OnNodeClicked(captured));
                n.SetVisual(Node.NodeState.Normal);
            }

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (x > 0)     grid[x, y].neighbours.Add(grid[x - 1, y]);
                if (x < w - 1) grid[x, y].neighbours.Add(grid[x + 1, y]);
                if (y > 0)     grid[x, y].neighbours.Add(grid[x, y - 1]);
                if (y < h - 1) grid[x, y].neighbours.Add(grid[x, y + 1]);
            }

        source = grid[0, h / 2];
        dest   = grid[w - 1, h / 2];
        source.isSource = true; dest.isDest = true;

        PlaceObstacles();
    }

    void BuildEndpoints(Transform parent)
    {
        // battery (amber box with +)
        var bat = UIFactory.Panel(parent, source.uiPos.x, source.uiPos.y, 46, 56, Theme.Hex("#1A1F12"), Theme.Amber);
        bat.raycastTarget = false;
        UIFactory.Label(bat.transform, "+", 0, 0, 46, 56, 26, Theme.Amber, TextAlignmentOptions.Center, true);

        // bulb (disc that lights up)
        bulbGlow = UIFactory.Disc(parent, dest.uiPos.x, dest.uiPos.y, 70, new Color(1, 1, 1, 0f));
        bulbGlow.raycastTarget = false;
        bulb = UIFactory.Disc(parent, dest.uiPos.x, dest.uiPos.y, 44, Theme.Hex("#22301F"));
        bulb.raycastTarget = false;
    }

    void PlaceObstacles()
    {
        var candidates = new List<Node>();
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (!grid[x, y].isSource && !grid[x, y].isDest) candidates.Add(grid[x, y]);

        int placed = 0, guard = 0;
        while (placed < obstacleCount && candidates.Count > 0 && guard++ < 300)
        {
            int idx = Random.Range(0, candidates.Count);
            Node n = candidates[idx];
            var saved = new List<Node>(n.neighbours);
            foreach (var nb in saved) nb.neighbours.Remove(n);
            n.neighbours.Clear();
            n.blocked = true;

            if (PathExists(source, dest))
            {
                n.SetVisual(Node.NodeState.Blocked);
                // chunky component look (square chip instead of a dot)
                n.image.sprite = UIFactory.SolidSprite(Theme.Hex("#243044"));
                n.image.rectTransform.sizeDelta = new Vector2(38, 30);
                candidates.RemoveAt(idx);
                placed++;
            }
            else
            {
                n.blocked = false;
                foreach (var nb in saved) { n.neighbours.Add(nb); nb.neighbours.Add(n); }
                candidates.RemoveAt(idx);
            }
        }
    }

    bool PathExists(Node a, Node b)
    {
        var seen = new HashSet<Node> { a };
        var stack = new Stack<Node>();
        stack.Push(a);
        while (stack.Count > 0)
        {
            var cur = stack.Pop();
            if (cur == b) return true;
            foreach (var nb in cur.neighbours)
                if (!nb.blocked && seen.Add(nb)) stack.Push(nb);
        }
        return false;
    }

    public void OnNodeClicked(Node n)
    {
        if (done || n.blocked) return;

        if (drawnPath.Count == 0)
        {
            if (!n.isSource) { feedbackText.text = "Start at the BATTERY!"; feedbackText.color = Theme.Amber; return; }
            drawnPath.Add(n); n.SetVisual(Node.NodeState.Path);
            RedrawPath(); return;
        }

        if (drawnPath.Count > 1 && drawnPath[drawnPath.Count - 2] == n)
        {
            var removed = drawnPath[drawnPath.Count - 1];
            drawnPath.RemoveAt(drawnPath.Count - 1);
            removed.SetVisual(removed.isSource ? Node.NodeState.Source : Node.NodeState.Normal);
            RedrawPath(); return;
        }

        Node last = drawnPath[drawnPath.Count - 1];
        if (!last.neighbours.Contains(n) || drawnPath.Contains(n)) return;

        Sfx.Click();
        drawnPath.Add(n);
        n.SetVisual(Node.NodeState.Path);
        RedrawPath();

        if (n.isDest) Finish();
    }

    void RedrawPath()
    {
        foreach (var l in pathLines) Destroy(l);
        pathLines.Clear();
        for (int i = 1; i < drawnPath.Count; i++)
        {
            var img = UIFactory.Line(lineLayer, drawnPath[i - 1].uiPos, drawnPath[i].uiPos, 8,
                done ? Color.white : Theme.Hex("#37B6FF"));
            pathLines.Add(img.gameObject);
        }
    }

    void Finish()
    {
        bool valid = PathValidator.ValidatePath(drawnPath, source, dest);
        done = true;
        foreach (var n in drawnPath) n.SetVisual(valid ? Node.NodeState.Correct : Node.NodeState.Error);
        RedrawPath();
        if (valid)
        {
            if (bulb) bulb.color = Color.white;
            if (bulbGlow) bulbGlow.color = new Color(1, 0.95f, 0.6f, 0.5f);
        }
        feedbackText.text = valid ? "CIRCUIT LIVE!" : "INVALID PATH!";
        feedbackText.color = valid ? Theme.Green : Theme.Red;
        GameOverScreen.Display(valid);
    }

    void OnReset()
    {
        if (done) return;
        foreach (var n in drawnPath)
            n.SetVisual(n.isSource ? Node.NodeState.Source : Node.NodeState.Normal);
        drawnPath.Clear();
        RedrawPath();
        feedbackText.text = "Click adjacent nodes from BATTERY to BULB.";
        feedbackText.color = Theme.Muted;
    }

    void OnReturn() => SceneLoader.ReturnToShop();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) { OnReturn(); return; }
        if (done) return;

        time -= Time.deltaTime;
        float frac = Mathf.Clamp01(time / maxTime);
        if (powerFill)
        {
            powerFill.fillAmount = frac;
            powerFill.color = frac <= 0.2f ? Theme.Red : Theme.Amber;
        }
        if (powerLabel)
        {
            bool crit = frac <= 0.2f;
            powerLabel.text = crit ? "! POWER CRITICAL" : "POWER DRAINING";
            powerLabel.color = crit ? Theme.Red : Theme.Muted;
        }
        if (time <= 0f)
        {
            done = true;
            feedbackText.text = "OUT OF POWER!";
            feedbackText.color = Theme.Red;
            GameOverScreen.Display(false);
        }
    }
}
