using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Small transient on-screen message ("Picked up...", "SALE!", order details).
// Call ToastUI.Show(text, color). Self-building; lives across the scene.
public class ToastUI : MonoBehaviour
{
    static ToastUI inst;
    Image bg;
    TextMeshProUGUI label;

    public static void Show(string msg, Color color)
    {
        if (inst == null) inst = new GameObject("ToastUI").AddComponent<ToastUI>();
        inst.Display(msg, color);
    }

    void Awake()
    {
        var cv = UIFactory.CreateCanvas("ToastCanvas", 80);
        cv.transform.SetParent(transform, false); // keep canvas with this object
        bg = UIFactory.Panel(cv.transform, 0, -210, 900, 70, new Color(0.03f, 0.04f, 0.08f, 0.95f), Theme.Line);
        label = UIFactory.Label(bg.transform, "", 0, 0, 860, 60, 16, Theme.White, TextAlignmentOptions.Center, true);
        bg.gameObject.SetActive(false);
    }

    void Display(string msg, Color color)
    {
        StopAllCoroutines();
        if (label) { label.text = msg; label.color = color; }
        if (bg)
        {
            var o = bg.GetComponent<Outline>(); if (o) o.effectColor = color;
            bg.gameObject.SetActive(true);
        }
        SetAlpha(1f);
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(2.6f);
        float t = 0f;
        while (t < 0.6f) { t += Time.deltaTime; SetAlpha(1f - t / 0.6f); yield return null; }
        if (bg) bg.gameObject.SetActive(false);
    }

    void SetAlpha(float a)
    {
        if (bg) { var c = bg.color; bg.color = new Color(c.r, c.g, c.b, 0.95f * a); }
        if (label) { var c = label.color; label.color = new Color(c.r, c.g, c.b, a); }
    }
}
