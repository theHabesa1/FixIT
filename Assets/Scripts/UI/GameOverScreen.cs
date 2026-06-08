using System.Collections;
using UnityEngine;
using TMPro;

// Self-building result overlay. Call GameOverScreen.Display(success) from any
// mini-game — it builds its own canvas, applies rewards, records mastery, and
// shows the PROMOTED/DEMOTED banner. Press R (or click) to return to shop.
public class GameOverScreen : MonoBehaviour
{
    public static void Display(bool success)
    {
        var go = new GameObject("GameOverScreen");
        go.AddComponent<GameOverScreen>().Build(success);
    }

    void Build(bool success)
    {
        string topic = GameManager.Instance.currentCustomerTopic;
        int    level = GameManager.Instance.currentCustomerLevel;

        // --- update stats / FSM ---
        GameManager.Instance.knowledge.RecordResult(topic, success);
        GameManager.Instance.lastRepairSuccess = success;
        GameManager.Instance.repairsCompleted++;

        int kGain = 0, mGain = 0;
        if (success)
        {
            kGain = GameManager.KnowledgeReward[level];
            mGain = GameManager.MoneyReward[level];
            GameManager.Instance.AddKnowledge(kGain);
            GameManager.Instance.AddMoney(mGain);
        }

        if (success) Sfx.Correct(); else Sfx.Wrong();

        // --- build UI ---
        var cv = UIFactory.CreateCanvas("GameOverCanvas", 100);
        var dim = UIFactory.FullBG(cv.transform, new Color(0, 0, 0, 0.85f));
        dim.raycastTarget = true;

        var card = UIFactory.Panel(cv.transform, 0, 0, 720, 380, Theme.Panel,
            success ? Theme.Green : Theme.Red);

        Color titleColor = success ? Theme.Green : Theme.Red;
        UIFactory.Label(card.transform, success ? "REPAIR COMPLETE!" : "REPAIR FAILED",
            0, 130, 680, 60, 30, titleColor, TextAlignmentOptions.Center, true);

        string rewardLine = success
            ? "+" + kGain + " KNOWLEDGE    +$" + mGain
            : "NO REWARD";
        UIFactory.Label(card.transform, rewardLine, 0, 55, 680, 40, 22, Theme.Amber);

        // mastery change line
        string change  = GameManager.Instance.knowledge.lastChangeType;
        string lvlName = LevelName(GameManager.Instance.knowledge.GetMastery(topic));
        string topicNm = TopicName(topic);
        string masteryLine;
        Color masteryColor = Theme.White;
        if (change == "promoted") { masteryLine = topicNm + ": PROMOTED to " + lvlName + "!"; masteryColor = Theme.Green; }
        else if (change == "demoted") { masteryLine = topicNm + ": DEMOTED to " + lvlName; masteryColor = Theme.Red; }
        else masteryLine = topicNm + ": " + lvlName;
        UIFactory.Label(card.transform, masteryLine, 0, 5, 680, 36, 20, masteryColor);

        UIFactory.Label(card.transform, "PRESS  R  TO RETURN TO SHOP",
            0, -110, 680, 36, 14, Theme.Muted, TextAlignmentOptions.Center, true);

        UIFactory.Btn(card.transform, "RETURN", 0, -155, 200, 50, Theme.Green,
            () => SceneLoader.ReturnToShop());

        // promotion / demotion banner flash
        if (!string.IsNullOrEmpty(change))
        {
            Color bannerColor = change == "promoted" ? Theme.Green : Theme.Red;
            var banner = UIFactory.Panel(cv.transform, 0, 280, 1280, 70,
                Color.Lerp(Theme.BG, bannerColor, 0.25f), bannerColor);
            string txt = change == "promoted"
                ? "PROMOTED TO " + lvlName.ToUpper() + "!"
                : "DEMOTED TO " + lvlName.ToUpper();
            UIFactory.Label(banner.transform, txt, 0, 0, 1200, 50, 26, bannerColor,
                TextAlignmentOptions.Center, true);
            StartCoroutine(FlashBanner(banner.transform));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) SceneLoader.ReturnToShop();
    }

    IEnumerator FlashBanner(Transform banner)
    {
        float t = 0f;
        var rt = banner as RectTransform;
        Vector2 onScreen = new Vector2(0, 280);
        Vector2 offScreen = new Vector2(0, 420);
        // slide in
        while (t < 0.25f) { t += Time.deltaTime; rt.anchoredPosition = Vector2.Lerp(offScreen, onScreen, t / 0.25f); yield return null; }
        rt.anchoredPosition = onScreen;
        yield return new WaitForSeconds(2f);
        // slide out
        t = 0f;
        while (t < 0.25f) { t += Time.deltaTime; rt.anchoredPosition = Vector2.Lerp(onScreen, offScreen, t / 0.25f); yield return null; }
        banner.gameObject.SetActive(false);
    }

    static string TopicName(string t) => t switch
    {
        "logic"    => "Logic Gates",
        "binary"   => "Binary",
        "circuits" => "Circuits",
        "ram"      => "RAM",
        _ => t
    };

    static string LevelName(KnowledgeSystem.MasteryLevel l) => l switch
    {
        KnowledgeSystem.MasteryLevel.Novice       => "Novice",
        KnowledgeSystem.MasteryLevel.Intermediate => "Intermediate",
        KnowledgeSystem.MasteryLevel.Expert       => "Expert",
        _ => ""
    };
}
