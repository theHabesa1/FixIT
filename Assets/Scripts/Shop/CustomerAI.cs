using System.Collections;
using UnityEngine;
using TMPro;

// A customer. Spawned in code by CustomerSpawner. Walks in from the door to its
// wait spot, shows a thought bubble with the broken device + a topic tag, and a
// "CLICK / E" prompt when the player is near. Starts a repair on click / E.
public class CustomerAI : MonoBehaviour
{
    public string kind;   // "student" | "office" | "grandma" | "gamer"
    public string topic;  // "logic" | "binary" | "circuits" | "ram"
    public string device; // "laptop" | "phone" | "pc" | "tower"

    public bool  isBuyer;  // true = wants to BUY (Back Store), false = repair
    public Order order;    // the product they want to buy

    public float leaveAfter = 30f;
    public Vector2 waitSpot;
    public float promptRadius = 1.6f;

    SpriteRenderer body;
    GameObject prompt;
    TextMeshPro promptTM;
    bool held;            // order taken — wait patiently for the player to return
    float leaveTimer;
    bool arrived;
    Transform player;

    static readonly Color[] LevelColors =
        { Theme.Muted, Theme.Hex("#37B6FF"), Theme.Amber };

    void Start()
    {
        leaveTimer = leaveAfter;

        body = GetComponent<SpriteRenderer>();
        if (body == null) body = gameObject.AddComponent<SpriteRenderer>();
        body.sprite = Pixels.Character(kind);
        body.sortingOrder = 20;
        transform.localScale = Vector3.one * 1.5f; // sprite is ~14px @ 12ppu

        if (GetComponent<Collider2D>() == null)
        {
            var col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.7f, 1.4f);
            col.offset = new Vector2(0f, 0.7f);
        }

        BuildShadow();
        BuildBubble();
        BuildTag();
        BuildPrompt();

        var pc = FindFirstObjectByType<PlayerController>();
        if (pc) player = pc.transform;

        StartCoroutine(WalkTo(waitSpot));
    }

    void BuildShadow()
    {
        var go = new GameObject("Shadow");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0, 0.02f, 0);
        go.transform.localScale = new Vector3(0.7f, 0.18f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = UIFactory.SolidSprite(new Color(0, 0, 0, 0.35f));
        sr.sortingOrder = 19;
    }

    void BuildBubble()
    {
        // container (uniform) so children scale independently of each other
        var bubble = new GameObject("Bubble");
        bubble.transform.SetParent(transform, false);
        bubble.transform.localPosition = new Vector3(0.1f, 1.45f, 0);

        var bgGO = new GameObject("BubbleBG");
        bgGO.transform.SetParent(bubble.transform, false);
        bgGO.transform.localScale = new Vector3(0.6f, 0.5f, 1f);
        var bg = bgGO.AddComponent<SpriteRenderer>();
        bg.sprite = UIFactory.SolidSprite(Theme.Hex("#EEF1FF"));
        bg.sortingOrder = 21;

        var icon = new GameObject("Device");
        icon.transform.SetParent(bubble.transform, false);
        icon.transform.localScale = Vector3.one * 0.5f;
        var ir = icon.AddComponent<SpriteRenderer>();
        ir.sprite = Pixels.Device(device);
        ir.sortingOrder = 22;
    }

    void BuildTag()
    {
        int lvl = 0;
        if (GameManager.Instance != null && !isBuyer)
            lvl = (int)GameManager.Instance.knowledge.GetMastery(topic);
        Color c = isBuyer ? Theme.Amber : LevelColors[Mathf.Clamp(lvl, 0, 2)];

        var go = new GameObject("Tag");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0, -0.28f, 0);
        go.transform.localScale = Vector3.one * 0.12f;
        var tm = go.AddComponent<TextMeshPro>();
        tm.text = isBuyer ? "BUY" : TopicShort(topic);
        tm.fontSize = 8;
        tm.alignment = TextAlignmentOptions.Center;
        tm.color = c;
        if (Theme.Pixel != null) tm.font = Theme.Pixel;
        tm.sortingOrder = 22;
    }

    void BuildPrompt()
    {
        prompt = new GameObject("Prompt");
        prompt.transform.SetParent(transform, false);
        prompt.transform.localPosition = new Vector3(0, 2.2f, 0);
        prompt.transform.localScale = Vector3.one * 0.14f;
        promptTM = prompt.AddComponent<TextMeshPro>();
        promptTM.text = "> CLICK / E";
        promptTM.fontSize = 8;
        promptTM.alignment = TextAlignmentOptions.Center;
        promptTM.color = Theme.Green;
        if (Theme.Pixel != null) promptTM.font = Theme.Pixel;
        promptTM.sortingOrder = 23;
        prompt.SetActive(false);
    }

    // Called when the player takes this buyer's order: wait 30-45s for them to
    // fetch the item from the back store instead of leaving.
    public void HoldForService()
    {
        held = true;
        leaveTimer = Random.Range(30f, 45f);
    }

    void Update()
    {
        if (!arrived) return;

        if (held)
        {
            if (prompt)
            {
                prompt.SetActive(true);
                if (promptTM) { promptTM.text = "WAITING " + Mathf.CeilToInt(leaveTimer) + "s"; promptTM.color = Theme.Amber; }
            }
        }
        else if (prompt && player)
        {
            float d = Vector2.Distance(player.position, transform.position);
            prompt.SetActive(d <= promptRadius);
        }

        leaveTimer -= Time.deltaTime;
        if (leaveTimer <= 0f) Destroy(gameObject);
    }

    IEnumerator WalkTo(Vector2 dest)
    {
        while (Vector2.Distance(transform.position, dest) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(transform.position, dest, 2.5f * Time.deltaTime);
            yield return null;
        }
        transform.position = dest;
        arrived = true;
    }

    void OnMouseDown()
    {
        if (!arrived) return;
        var pi = FindFirstObjectByType<PlayerInteract>();
        if (pi != null) pi.StartRepair(this);
    }

    static string TopicShort(string t) => t switch
    {
        "logic"    => "LOGIC",
        "binary"   => "BINARY",
        "circuits" => "CIRCUIT",
        "ram"      => "RAM",
        _ => t.ToUpper()
    };
}
