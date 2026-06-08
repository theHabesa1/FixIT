using System.Collections.Generic;
using UnityEngine;

// Peculiar Feature: Finite State Machine per CS topic.
// States: Novice(0), Intermediate(1), Expert(2).
// Promote after 3 correct in a row; demote after 2 wrong in a row.
public class KnowledgeSystem : MonoBehaviour
{
    public enum MasteryLevel { Novice = 0, Intermediate = 1, Expert = 2 }

    // last promotion/demotion result, read by GameOverScreen
    public string lastChangeTopic   = "";
    public string lastChangeType    = ""; // "promoted" | "demoted" | ""
    public MasteryLevel lastFromLevel;
    public MasteryLevel lastToLevel;

    private Dictionary<string, MasteryLevel> masteryState = new Dictionary<string, MasteryLevel>();
    private Dictionary<string, int>          streak       = new Dictionary<string, int>();

    public MasteryLevel GetMastery(string topic)
    {
        if (!masteryState.ContainsKey(topic)) masteryState[topic] = MasteryLevel.Novice;
        return masteryState[topic];
    }

    public void RecordResult(string topic, bool correct)
    {
        if (!streak.ContainsKey(topic))       streak[topic]       = 0;
        if (!masteryState.ContainsKey(topic)) masteryState[topic] = MasteryLevel.Novice;

        lastChangeTopic = topic;
        lastChangeType  = "";
        lastFromLevel   = masteryState[topic];
        lastToLevel     = masteryState[topic];

        if (correct)
        {
            streak[topic]++;
            if (streak[topic] >= 3 && masteryState[topic] < MasteryLevel.Expert)
            {
                lastChangeType        = "promoted";
                lastFromLevel         = masteryState[topic];
                masteryState[topic]++;
                lastToLevel           = masteryState[topic];
                streak[topic]         = 0;
            }
        }
        else
        {
            streak[topic] = Mathf.Min(streak[topic] - 1, 0);
            if (streak[topic] <= -2 && masteryState[topic] > MasteryLevel.Novice)
            {
                lastChangeType        = "demoted";
                lastFromLevel         = masteryState[topic];
                masteryState[topic]--;
                lastToLevel           = masteryState[topic];
                streak[topic]         = 0;
            }
        }
    }

    public int GetStreakRaw(string topic)
    {
        return streak.ContainsKey(topic) ? streak[topic] : 0;
    }

    // 1-3 stars for mastery board display
    public int GetStars(string topic)
    {
        return (int)GetMastery(topic) + 1;
    }
}
