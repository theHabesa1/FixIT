using System.Collections.Generic;
using UnityEngine;

// Manages the two rooms (shop + back store) that live in the same scene. Handles
// walking between them (camera + player teleport), the shelves, and picking up
// stock. The buyer customer stays in the shop while you fetch, so the sale can be
// completed when you return.
public class RoomManager : MonoBehaviour
{
    public static RoomManager I;

    // room geometry (back store is offset far to the right, off-screen from shop)
    public Vector2 shopCenter = new Vector2(0, 0);
    public Vector2 backCenter = new Vector2(30, 0);
    public Vector2 shopMin = new Vector2(-8.4f, -4.4f), shopMax = new Vector2(8.8f, 3.2f);
    public Vector2 backMin = new Vector2(21.6f, -4.4f), backMax = new Vector2(38.4f, 3.2f);
    public Vector2 shopSpawn = new Vector2(6f, -1f);
    public Vector2 backSpawn = new Vector2(25f, -1f);

    public List<Shelf> shelves = new List<Shelf>();
    public bool InBack { get; private set; }

    PlayerController player;
    float cooldown;

    void Awake() { I = this; }

    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        if (player) { player.areaMin = shopMin; player.areaMax = shopMax; }
        // fill shelves with something to look at before the first order
        RefreshShelves(StockGen.RandomOrder());
    }

    void Update() { if (cooldown > 0f) cooldown -= Time.deltaTime; }

    public void Go(bool toBack)
    {
        if (cooldown > 0f) return;
        InBack = toBack;

        Vector2 center = toBack ? backCenter : shopCenter;
        if (Camera.main) Camera.main.transform.position = new Vector3(center.x, center.y, -10);

        if (player == null) player = FindFirstObjectByType<PlayerController>();
        if (player)
        {
            player.areaMin = toBack ? backMin : shopMin;
            player.areaMax = toBack ? backMax : shopMax;
            Vector2 sp = toBack ? backSpawn : shopSpawn;
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb) rb.position = sp;
            player.transform.position = sp;
        }

        cooldown = 0.7f;
        Sfx.Click();
        if (toBack) ToastUI.Show("BACK STORE - click the shelf with the matching item", Theme.Green);
        else        ToastUI.Show("Back in the shop - sell to your customer (press E)", Theme.Green);
    }

    // Called when the player accepts a new order: stock the shelves so exactly one matches.
    public void AcceptOrder(Order o) => RefreshShelves(o);

    public void RefreshShelves(Order want)
    {
        if (shelves.Count == 0 || want == null) return;
        var items = StockGen.BuildStock(want, shelves.Count);
        for (int i = 0; i < shelves.Count; i++) shelves[i].Set(items[i]);
    }

    public void TryPickup(Shelf s)
    {
        if (s == null || s.item == null) return;
        var gm = GameManager.Instance;
        if (gm.carriedItem != null) { ToastUI.Show("You're already carrying an item.", Theme.Amber); return; }
        gm.carriedItem = s.item;
        Sfx.Click();
        ToastUI.Show("Picked up " + s.item.product + " (" + s.item.ValuesLine() + ") - take it to the customer.", Theme.White);
        s.MarkTaken();
    }
}
