using System.Collections;
using UnityEngine;

// Procedural sound — generates tones at runtime so no audio assets are required.
public class Sfx : MonoBehaviour
{
    public static Sfx I;
    AudioSource src;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        src = gameObject.GetComponent<AudioSource>();
        if (src == null) src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
    }

    public static void Click()   { if (I) I.src.PlayOneShot(I.Tone(740f, 0.06f, 0.25f, true)); }
    public static void Toggle()  { if (I) I.src.PlayOneShot(I.Tone(523f, 0.06f, 0.25f, true)); }
    public static void Correct() { if (I) I.StartCoroutine(I.Arp(new[] { 660f, 880f, 1320f }, 0.09f)); }
    public static void Wrong()   { if (I) I.src.PlayOneShot(I.Tone(120f, 0.32f, 0.35f, true)); }

    IEnumerator Arp(float[] freqs, float step)
    {
        foreach (float f in freqs)
        {
            src.PlayOneShot(Tone(f, step + 0.05f, 0.3f, false));
            yield return new WaitForSeconds(step);
        }
    }

    AudioClip Tone(float freq, float dur, float vol, bool square)
    {
        int rate = 44100;
        int n = Mathf.Max(1, (int)(rate * dur));
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / rate;
            float env = Mathf.Exp(-4f * t / dur);            // decay envelope
            float s = Mathf.Sin(2f * Mathf.PI * freq * t);
            if (square) s = Mathf.Sign(s);
            data[i] = s * vol * env;
        }
        var clip = AudioClip.Create("tone", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
