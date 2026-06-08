using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Binary Decoder mini-game (matches the Claude design): a corrupted terminal
// shows a binary/hex number in big glowing digits; type the decimal value on the
// keypad (or keyboard) before you run out of lives.
public class BinaryManager : MonoBehaviour
{
    BinaryPuzzleGen.Puzzle puzzle;
    string playerInput = "";
    int    lives = 3;
    bool   done;

    TextMeshProUGUI inputDisplay, feedbackText, caret;
    Image inputBox;
    readonly List<Image> lifeDiscs = new List<Image>();
    RectTransform numberRow;

    void Start()
    {
        int level = GameManager.Instance != null ? GameManager.Instance.currentCustomerLevel : 0;
        puzzle = BinaryPuzzleGen.Generate(level);

        var cv = UIFactory.CreateCanvas("BinaryCanvas");
        UIFactory.FullBG(cv.transform, Color.black);

        UIFactory.Header(cv.transform, "BINARY", level, Theme.Green);
        UIFactory.MasteryCorner(cv.transform, "binary");
        BuildLives(cv.transform);

        // terminal flavour line + blinking caret
        UIFactory.Label(cv.transform, "> ERROR: Data packet corrupted. Decode to repair.",
            -8, 250, 1000, 30, 20, Theme.Green, TextAlignmentOptions.Center);

        BuildBigNumber(cv.transform);
        UIFactory.Label(cv.transform, "CONVERT TO DECIMAL", 0, 78, 600, 30, 16, Theme.Hex("#BFFFB0"), TextAlignmentOptions.Center, true);

        // amber readout box
        inputBox = UIFactory.Panel(cv.transform, 0, 18, 280, 60, Theme.Hex("#281C00"), Theme.Amber);
        inputDisplay = UIFactory.Label(inputBox.transform, "_", 0, 0, 260, 48, 30, Theme.Amber, TextAlignmentOptions.Center, true);

        BuildNumberPad(cv.transform);

        feedbackText = UIFactory.Label(cv.transform, "", 0, -300, 1000, 30, 16, Theme.White);
        UIFactory.Btn(cv.transform, "R: SHOP", 540, -300, 150, 44, Theme.Muted, OnReturn);

        StartCoroutine(BlinkCaret());
        RefreshLives();
        RefreshInput();
    }

    void BuildBigNumber(Transform parent)
    {
        string s = puzzle.displayString;
        var rowGO = new GameObject("BigNumber", typeof(RectTransform));
        rowGO.transform.SetParent(parent, false);
        numberRow = rowGO.GetComponent<RectTransform>();
        numberRow.anchorMin = numberRow.anchorMax = numberRow.pivot = new Vector2(0.5f, 0.5f);
        numberRow.anchoredPosition = new Vector2(0, 158);
        numberRow.sizeDelta = new Vector2(1100, 90);

        float cell = Mathf.Min(64, 900f / s.Length);
        float startX = -(s.Length - 1) * cell * 0.5f;
        for (int i = 0; i < s.Length; i++)
        {
            char ch = s[i];
            bool hot = ch != '0';
            UIFactory.Label(numberRow, ch.ToString(), startX + i * cell, 0, cell, 90,
                cell * 0.85f, hot ? Theme.Green : Theme.Hex("#1F6B14"), TextAlignmentOptions.Center, true);
        }
    }

    void BuildLives(Transform parent)
    {
        var lbl = UIFactory.Label(parent, "LIVES", 360, 280, 90, 24, 9, Theme.Muted, TextAlignmentOptions.Right, true);
        for (int i = 0; i < 3; i++)
        {
            var d = UIFactory.Disc(parent, 430 + i * 36, 282, 22, Theme.Red);
            lifeDiscs.Add(d);
        }
    }

    void BuildNumberPad(Transform parent)
    {
        float baseX = 0, baseY = -120, gx = 90, gy = 70;
        for (int i = 0; i < 9; i++)
        {
            int col = i % 3, row = i / 3;
            float x = baseX + (col - 1) * gx;
            float y = baseY - row * gy + gy;
            int digit = i + 1;
            UIFactory.Btn(parent, digit.ToString(), x, y, 78, 60, Theme.Line, () => AppendDigit(digit));
        }
        UIFactory.Btn(parent, "CLR", baseX - gx, baseY - 2 * gy + gy, 78, 60, Theme.Amber, OnClear);
        UIFactory.Btn(parent, "0",   baseX,       baseY - 2 * gy + gy, 78, 60, Theme.Line, () => AppendDigit(0));
        UIFactory.Btn(parent, "OK",  baseX + gx,  baseY - 2 * gy + gy, 78, 60, Theme.Green, OnSubmit);
    }

    public void AppendDigit(int d)
    {
        if (done) return;
        if (playerInput.Length < 5) playerInput += d.ToString();
        Sfx.Click();
        RefreshInput();
    }

    void OnClear() { playerInput = ""; RefreshInput(); }

    void OnSubmit()
    {
        if (done || playerInput.Length == 0) return;
        if (!int.TryParse(playerInput, out int guess)) return;

        if (guess == puzzle.correctAnswer)
        {
            done = true;
            if (inputBox) inputBox.GetComponent<Outline>().effectColor = Theme.Green;
            if (inputDisplay) inputDisplay.color = Theme.Green;
            GameOverScreen.Display(true);
        }
        else
        {
            lives--;
            playerInput = "";
            RefreshLives(); RefreshInput();
            Sfx.Wrong();
            StartCoroutine(ShakeNumber());
            if (lives <= 0)
            {
                done = true;
                feedbackText.text = "FAILED! Answer was " + puzzle.correctAnswer;
                feedbackText.color = Theme.Red;
                GameOverScreen.Display(false);
            }
            else
            {
                feedbackText.text = "WRONG! " + lives + " attempt(s) left.";
                feedbackText.color = Theme.Amber;
            }
        }
    }

    IEnumerator ShakeNumber()
    {
        if (!numberRow) yield break;
        Vector2 home = numberRow.anchoredPosition;
        for (int i = 0; i < 6; i++)
        {
            numberRow.anchoredPosition = home + new Vector2((i % 2 == 0 ? 1 : -1) * 8, 0);
            yield return new WaitForSeconds(0.04f);
        }
        numberRow.anchoredPosition = home;
    }

    IEnumerator BlinkCaret()
    {
        while (!done)
        {
            if (inputDisplay) inputDisplay.text = playerInput.Length > 0 ? playerInput : "_";
            yield return new WaitForSeconds(0.5f);
            if (inputDisplay && playerInput.Length == 0) inputDisplay.text = " ";
            yield return new WaitForSeconds(0.5f);
        }
    }

    void OnReturn() => SceneLoader.ReturnToShop();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) { OnReturn(); return; }
        if (done) return;
        for (int d = 0; d <= 9; d++)
            if (Input.GetKeyDown(KeyCode.Alpha0 + d) || Input.GetKeyDown(KeyCode.Keypad0 + d))
                AppendDigit(d);
        if (Input.GetKeyDown(KeyCode.Backspace) && playerInput.Length > 0)
        { playerInput = playerInput.Substring(0, playerInput.Length - 1); RefreshInput(); }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) OnSubmit();
    }

    void RefreshInput() { if (inputDisplay) inputDisplay.text = playerInput.Length > 0 ? playerInput : "_"; }

    void RefreshLives()
    {
        for (int i = 0; i < lifeDiscs.Count; i++)
            if (lifeDiscs[i]) lifeDiscs[i].color = i < lives ? Theme.Red : Theme.Hex("#3A2030");
    }
}
