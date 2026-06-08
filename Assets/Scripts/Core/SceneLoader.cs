using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [Header("Fade")]
    public Image fadePanel; // black Image covering whole screen, alpha 0 normally

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public static void LoadScene(string sceneName)
    {
        if (Instance != null) Instance.StartCoroutine(Instance.FadeAndLoad(sceneName));
        else SceneManager.LoadScene(sceneName);
    }

    public static void ReturnToShop()
    {
        LoadScene("RepairShop");
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        // fade to black
        if (fadePanel != null)
        {
            float t = 0f;
            Color c = fadePanel.color;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                c.a = Mathf.Clamp01(t / 0.35f);
                fadePanel.color = c;
                yield return null;
            }
        }

        SceneManager.LoadScene(sceneName);

        yield return null; // wait one frame for scene to init

        // fade in
        if (fadePanel != null)
        {
            float t = 0f;
            Color c = fadePanel.color;
            c.a = 1f;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                c.a = Mathf.Clamp01(1f - t / 0.35f);
                fadePanel.color = c;
                yield return null;
            }
        }
    }
}
