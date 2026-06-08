using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int knowledgePoints = 0;
    public int money = 120;
    public int repairsCompleted = 0;

    // per-topic scores (0-9 scale)
    public Dictionary<string, int> topicScores = new Dictionary<string, int>();

    // set by PlayerInteract before loading a mini-game scene
    public string currentCustomerTopic = "logic";
    public int    currentCustomerLevel  = 0; // 0=Novice,1=Intermediate,2=Expert

    // set by mini-game before returning to shop
    public bool lastRepairSuccess = false;

    // Back Store sales loop
    public Order activeOrder;   // the order the player has accepted (shown on the ticket)
    public Order carriedItem;   // the item the player physically picked up from a shelf

    KnowledgeSystem _knowledge;
    public KnowledgeSystem knowledge =>
        _knowledge != null ? _knowledge : (_knowledge = GetComponent<KnowledgeSystem>());

    // Auto-creates the persistent manager before any scene loads, so you can
    // press Play on ANY scene and everything works — no manual setup needed.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("GameManager");
        go.AddComponent<KnowledgeSystem>();
        go.AddComponent<GameManager>();
        go.AddComponent<SceneLoader>();
        go.AddComponent<Sfx>();

        // CRT overlay on top of every scene (retro screen look)
        SceneManager.sceneLoaded += (s, m) => CRTOverlay.Ensure();
        CRTOverlay.Ensure();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddKnowledge(int v) { knowledgePoints += v; }
    public void AddMoney(int v)     { money += v; }
    public void LosePoints(int v)   { knowledgePoints = Mathf.Max(0, knowledgePoints - v); }
    public int  GetKnowledge()      { return knowledgePoints; }

    public void UpdateTopicScore(string topic, int delta)
    {
        if (!topicScores.ContainsKey(topic)) topicScores[topic] = 0;
        topicScores[topic] = Mathf.Clamp(topicScores[topic] + delta, 0, 9);
    }

    // Rewards matching the design: [Novice, Intermediate, Expert]
    public static readonly int[] KnowledgeReward = { 12, 24, 40 };
    public static readonly int[] MoneyReward      = { 15, 30, 55 };
}
