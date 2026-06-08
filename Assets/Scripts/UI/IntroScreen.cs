using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Title / intro screen shown once when the game first starts. Pauses the game
// (timeScale 0) until the player presses Enter / Space / clicks. Built in code
// and added by ShopBootstrap; shows only on the first load of the session.
public class IntroScreen : MonoBehaviour
{
    static bool shown;
    TextMeshProUGUI startLabel;
    bool dismissed;

    public static void ShowOnce()
    {
        if (shown) return;
        shown = true;
        new GameObject("IntroScreen").AddComponent<IntroScreen>();
    }

    void Start()
    {
        var cv = UIFactory.CreateCanvas("IntroCanvas", 500);
        cv.transform.SetParent(transform, false); // so destroying this object clears it
        UIFactory.FullBG(cv.transform, Theme.Hex("#05060A")).raycastTarget = true;

        UIFactory.Panel(cv.transform, 0, -10, 920, 470, new Color(0.05f, 0.06f, 0.10f, 0.65f), Theme.Line);

        // logo
        UIFactory.Label(cv.transform, "<color=#39FF14>Fix</color><color=#9B6DFF>IT</color>",
            0, 155, 920, 110, 72, Color.white, TextAlignmentOptions.Center, true);
        UIFactory.Label(cv.transform, "RETRO REPAIR SHOP", 0, 92, 920, 30, 16, Theme.Muted, TextAlignmentOptions.Center, true);

        // technician sprite
        var techGO = new GameObject("Tech", typeof(RectTransform), typeof(Image));
        techGO.transform.SetParent(cv.transform, false);
        var trt = techGO.GetComponent<RectTransform>();
        trt.anchorMin = trt.anchorMax = trt.pivot = new Vector2(0.5f, 0.5f);
        trt.anchoredPosition = new Vector2(0, 18);
        trt.sizeDelta = new Vector2(80, 140);
        var ti = techGO.GetComponent<Image>();
        ti.sprite = Pixels.Character("tech");
        ti.preserveAspect = true;
        ti.raycastTarget = false;

        string how =
            "FIX broken devices by solving CS puzzles.\n" +
            "SELL hardware: take an order, fetch the right\n" +
            "part from the back store, and deliver it.\n" +
            "Build MASTERY, earn MONEY and KNOWLEDGE.";
        UIFactory.Label(cv.transform, how, 0, -120, 840, 130, 18, Theme.White, TextAlignmentOptions.Center);
        UIFactory.Label(cv.transform, "WASD / ARROWS move    -    E interact    -    R back",
            0, -188, 840, 26, 12, Theme.Muted, TextAlignmentOptions.Center, true);
        startLabel = UIFactory.Label(cv.transform, "PRESS  ENTER  OR  CLICK  TO  START",
            0, -235, 840, 30, 16, Theme.Green, TextAlignmentOptions.Center, true);

        Time.timeScale = 0f; // pause until the player starts
    }

    void Update()
    {
        if (startLabel) startLabel.enabled = Mathf.FloorToInt(Time.unscaledTime * 2f) % 2 == 0;

        if (dismissed) return;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)
            || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            dismissed = true;
            Time.timeScale = 1f;
            Sfx.Click();
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // safety: never leave the game paused
        if (!dismissed) Time.timeScale = 1f;
    }
}
