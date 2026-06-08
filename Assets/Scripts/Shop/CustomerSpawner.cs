using UnityEngine;

// Spawns customers in code (no prefab needed). Fills the wait spots and
// periodically replaces customers that leave.
public class CustomerSpawner : MonoBehaviour
{
    public float spawnInterval = 8f;
    public Vector2[] waitSpots;

    static readonly string[] Kinds  = { "student", "office", "grandma", "gamer" };
    static readonly string[] Topics = { "logic", "binary", "circuits", "ram" };

    float timer;

    void Start()
    {
        // fill all spots immediately so the shop isn't empty
        if (waitSpots != null)
            foreach (var spot in waitSpots) Spawn(spot);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval) { timer = 0f; TrySpawnFreeSpot(); }
    }

    void TrySpawnFreeSpot()
    {
        if (waitSpots == null) return;
        var existing = FindObjectsByType<CustomerAI>(FindObjectsSortMode.None);
        foreach (var spot in waitSpots)
        {
            bool occupied = false;
            foreach (var c in existing)
                if (Vector2.Distance(c.waitSpot, spot) < 0.5f) { occupied = true; break; }
            if (!occupied) { Spawn(spot); return; }
        }
    }

    void Spawn(Vector2 spot)
    {
        var go = new GameObject("Customer");
        // enter from the left door, then walk to spot
        go.transform.position = new Vector3(-9f, spot.y, 0f);
        var ai = go.AddComponent<CustomerAI>();
        ai.kind     = Kinds[Random.Range(0, Kinds.Length)];
        ai.waitSpot = spot;

        // ~40% of customers are buyers (Back Store sale); the rest are repairs.
        if (Random.value < 0.4f)
        {
            ai.isBuyer = true;
            ai.order   = StockGen.RandomOrder();
            ai.device  = StockGen.Icon(ai.order.product);
            ai.topic   = "buy";
        }
        else
        {
            ai.topic  = Topics[Random.Range(0, Topics.Length)];
            ai.device = KindToDevice(ai.kind);
        }
    }

    static string KindToDevice(string kind) => kind switch
    {
        "student" => "laptop",
        "office"  => "phone",
        "grandma" => "pc",
        "gamer"   => "tower",
        _ => "device"
    };
}
