using UnityEngine;
using TMPro;

// Self-building shop HUD: knowledge + money panel (bottom-left), an order/carry
// ticket (bottom-left, above stats), and a controls hint (bottom-center).
public class HUDController : MonoBehaviour
{
    TextMeshProUGUI knowledgeText, moneyText, ticketText;
    UnityEngine.UI.Image ticketPanel;

    void Start()
    {
        var cv = UIFactory.CreateCanvas("HUDCanvas", 10);

        var stats = UIFactory.Panel(cv.transform, -530, -300, 200, 84, Theme.Panel, Theme.Line);
        knowledgeText = UIFactory.Label(stats.transform, "KP 0", 0, 20, 180, 30, 16,
            Theme.Green, TextAlignmentOptions.Left, true);
        moneyText = UIFactory.Label(stats.transform, "$0", 0, -18, 180, 30, 16,
            Theme.Amber, TextAlignmentOptions.Left, true);

        // order / carry ticket (hidden until there is something to show)
        ticketPanel = UIFactory.Panel(cv.transform, -450, -232, 360, 40, Theme.Hex("#0A0C14"), Theme.Amber);
        ticketText = UIFactory.Label(ticketPanel.transform, "", 0, 0, 344, 34, 11,
            Theme.Amber, TextAlignmentOptions.Left, true);
        ticketPanel.gameObject.SetActive(false);

        var hintBar = UIFactory.Panel(cv.transform, 0, -332, 760, 34, Theme.Hex("#080A12"), Theme.Line);
        UIFactory.Label(hintBar.transform, "WASD / ARROWS to move   -   press E at a customer  (BUY = fetch from back store)",
            0, 0, 740, 30, 11, Theme.Muted, TextAlignmentOptions.Center, true);
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        if (knowledgeText) knowledgeText.text = "KP " + gm.knowledgePoints;
        if (moneyText)     moneyText.text     = "$" + gm.money;

        if (ticketPanel == null) return;
        if (gm.carriedItem != null)
        {
            ticketPanel.gameObject.SetActive(true);
            ticketPanel.GetComponent<UnityEngine.UI.Outline>().effectColor = Theme.Green;
            ticketText.color = Theme.Green;
            ticketText.text = "CARRYING: " + gm.carriedItem.product + "  " + gm.carriedItem.ValuesLine();
        }
        else if (gm.activeOrder != null)
        {
            ticketPanel.gameObject.SetActive(true);
            ticketPanel.GetComponent<UnityEngine.UI.Outline>().effectColor = Theme.Amber;
            ticketText.color = Theme.Amber;
            ticketText.text = "ORDER: " + gm.activeOrder.product + "  " + gm.activeOrder.ValuesLine();
        }
        else
        {
            ticketPanel.gameObject.SetActive(false);
        }
    }
}
