using UnityEngine;
using TMPro;

// Builds the entire Repair Shop scene in code to match the Claude design: fixed
// single-screen view, back wall + neon sign + mastery board, checkered floor,
// entrance door, waiting bench, a tool-covered repair desk, the pixel-art player,
// and walk-in customers. Add this one component to an empty GameObject and Play.
public class ShopBootstrap : MonoBehaviour
{
    void Start()
    {
        SetupCamera();
        BuildFloor();
        BuildBackWall();
        BuildDoor();
        BuildBench();
        BuildDesk();
        BuildPlayer();
        BuildCustomers();

        // self-building UI
        gameObject.AddComponent<HUDController>();
        gameObject.AddComponent<MasteryBoard>();

        BuildNeonSign();

        // back store room (second room in the same scene) + doors
        var rm = gameObject.AddComponent<RoomManager>();
        BuildBackStoreDoor(rm);
        BuildBackStoreRoom(rm);

        // title screen (first load of the session only)
        IntroScreen.ShowOnce();
    }

    void BuildBackStoreDoor(RoomManager rm)
    {
        // door on the right wall of the shop -> back store
        MakeDoor("ToBackStore", new Vector2(rm.shopMax.x, 1.2f), "BACK STORE", Theme.Amber, true);
    }

    void BuildBackStoreRoom(RoomManager rm)
    {
        Vector2 c = rm.backCenter;

        // floor
        var floor = new GameObject("BackFloor");
        var fsr = floor.AddComponent<SpriteRenderer>();
        fsr.sprite = UIFactory.CheckerSprite(Theme.Hex("#1A1410"), Theme.Hex("#141019"), 8);
        fsr.drawMode = SpriteDrawMode.Tiled;
        fsr.size = new Vector2(20, 12);
        fsr.sortingOrder = -10;
        floor.transform.position = new Vector3(c.x, c.y - 0.6f, 0);

        // back wall
        var wall = new GameObject("BackRoomWall");
        var wsr = wall.AddComponent<SpriteRenderer>();
        wsr.sprite = UIFactory.SolidSprite(Theme.Hex("#0C0E18"));
        wsr.sortingOrder = -5;
        wall.transform.position = new Vector3(c.x, c.y + 4.3f, 0);
        wall.transform.localScale = new Vector3(20, 1.6f, 1);

        // title
        var title = new GameObject("BackTitle");
        title.transform.position = new Vector3(c.x, c.y + 4.2f, 0);
        title.transform.localScale = Vector3.one * 0.3f;
        var ttm = title.AddComponent<TextMeshPro>();
        ttm.text = "BACK STORE";
        ttm.fontSize = 14; ttm.alignment = TextAlignmentOptions.Center; ttm.color = Theme.Amber;
        if (Theme.Pixel != null) ttm.font = Theme.Pixel;
        ttm.sortingOrder = 10;

        // shelves: 2 rows x 3 columns
        float[] sxs = { c.x - 4.5f, c.x, c.x + 4.5f };
        float[] sys = { c.y + 1.4f, c.y - 1.8f };
        for (int r = 0; r < 2; r++)
            for (int col = 0; col < 3; col++)
                rm.shelves.Add(MakeShelf(new Vector2(sxs[col], sys[r])));

        // floor clutter (crates + a plant) for flavour
        Decor(Pixels.Furniture("crate"), new Vector3(c.x - 7.2f, c.y - 3.4f, 0), 1.3f, 3);
        Decor(Pixels.Furniture("crate"), new Vector3(c.x - 6.3f, c.y - 3.6f, 0), 1.0f, 3);
        Decor(Pixels.Furniture("crate"), new Vector3(c.x + 6.6f, c.y - 3.5f, 0), 1.2f, 3);
        Decor(Pixels.Furniture("plant"), new Vector3(c.x + 7.6f, c.y - 3.2f, 0), 1.6f, 3);

        // exit door (left of back room) -> shop
        MakeDoor("ToShop", new Vector2(rm.backMin.x, 1.2f), "EXIT", Theme.Green, false);
    }

    Shelf MakeShelf(Vector2 p)
    {
        var go = new GameObject("Shelf");
        go.transform.position = new Vector3(p.x, p.y, 0);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Pixels.Furniture("shelf");
        sr.sortingOrder = 2;
        go.transform.localScale = new Vector3(2.8f, 2.2f, 1);

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        // dark plate behind the label for readability over the boxes
        var plate = new GameObject("Plate");
        plate.transform.SetParent(go.transform, false);
        plate.transform.localPosition = new Vector3(0, -0.34f, 0);
        plate.transform.localScale = new Vector3(0.94f, 0.2f, 1);
        var psr = plate.AddComponent<SpriteRenderer>();
        psr.sprite = UIFactory.SolidSprite(new Color(0.03f, 0.04f, 0.07f, 0.92f));
        psr.sortingOrder = 4;

        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        lblGO.transform.localPosition = new Vector3(0, -0.34f, 0);
        lblGO.transform.localScale = new Vector3(1f / 2.8f, 1f / 2.2f, 1) * 0.16f;
        var tm = lblGO.AddComponent<TextMeshPro>();
        tm.fontSize = 7; tm.alignment = TextAlignmentOptions.Center; tm.color = Theme.White;
        tm.rectTransform.sizeDelta = new Vector2(90, 44);
        if (Theme.Pixel != null) tm.font = Theme.Pixel;
        tm.sortingOrder = 6;

        var shelf = go.AddComponent<Shelf>();
        shelf.Init(sr, tm);
        return shelf;
    }

    void MakeDoor(string name, Vector2 p, string label, Color color, bool toBack)
    {
        var go = new GameObject(name);
        go.transform.position = new Vector3(p.x, p.y, 0);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = UIFactory.SolidSprite(Theme.Hex("#0E1A16"));
        sr.sortingOrder = -3;
        go.transform.localScale = new Vector3(0.6f, 2.6f, 1);

        // glowing frame edge
        var edge = new GameObject("Edge");
        edge.transform.SetParent(go.transform, false);
        edge.transform.localScale = new Vector3(0.15f, 1f, 1);
        var esr = edge.AddComponent<SpriteRenderer>();
        esr.sprite = UIFactory.SolidSprite(color);
        esr.sortingOrder = -2;

        // trigger (separate object so its size is independent of the door's scale)
        var trig = new GameObject(name + "Trigger");
        trig.transform.position = new Vector3(p.x, p.y, 0);
        var bc = trig.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(1.4f, 3f);
        var dt = trig.AddComponent<DoorTrigger>();
        dt.toBack = toBack;

        var lblGO = new GameObject("DoorLabel");
        lblGO.transform.position = new Vector3(p.x, p.y + 1.7f, 0);
        lblGO.transform.localScale = Vector3.one * 0.16f;
        var tm = lblGO.AddComponent<TextMeshPro>();
        tm.text = label;
        tm.fontSize = 7; tm.alignment = TextAlignmentOptions.Center; tm.color = color;
        tm.rectTransform.sizeDelta = new Vector2(90, 20);
        if (Theme.Pixel != null) tm.font = Theme.Pixel;
        tm.sortingOrder = 6;
    }

    void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera");
            cam = go.AddComponent<Camera>();
            go.tag = "MainCamera";
            go.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Theme.Hex("#05060A");
        cam.transform.position = new Vector3(0, 0, -10);
    }

    void BuildFloor()
    {
        var go = new GameObject("Floor");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = UIFactory.CheckerSprite(Theme.Hex("#191C2A"), Theme.Hex("#13151F"), 8);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(40, 24);
        sr.sortingOrder = -10;
        go.transform.position = new Vector3(0, -0.6f, 0);
    }

    void BuildBackWall()
    {
        var wall = new GameObject("BackWall");
        var sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = UIFactory.SolidSprite(Theme.Hex("#0C0E18"));
        sr.sortingOrder = -5;
        wall.transform.position = new Vector3(0, 4.3f, 0);
        wall.transform.localScale = new Vector3(20, 1.6f, 1);

        // thin highlight line along the bottom edge of the wall
        var line = new GameObject("WallTrim");
        var lr = line.AddComponent<SpriteRenderer>();
        lr.sprite = UIFactory.SolidSprite(Theme.Hex("#05060A"));
        lr.sortingOrder = -4;
        line.transform.position = new Vector3(0, 3.5f, 0);
        line.transform.localScale = new Vector3(20, 0.08f, 1);
    }

    void BuildDoor()
    {
        var frame = new GameObject("DoorFrame");
        var fr = frame.AddComponent<SpriteRenderer>();
        fr.sprite = UIFactory.SolidSprite(Theme.Hex("#0E1A16"));
        fr.sortingOrder = -4;
        frame.transform.position = new Vector3(-8.4f, 0.4f, 0);
        frame.transform.localScale = new Vector3(0.5f, 2.8f, 1);

        var edge = new GameObject("DoorEdge");
        var er = edge.AddComponent<SpriteRenderer>();
        er.sprite = UIFactory.SolidSprite(Theme.Green);
        er.sortingOrder = -3;
        edge.transform.position = new Vector3(-8.15f, 0.4f, 0);
        edge.transform.localScale = new Vector3(0.06f, 2.8f, 1);

        var lbl = new GameObject("DoorLabel");
        lbl.transform.position = new Vector3(-7.8f, 0.4f, 0);
        lbl.transform.localScale = Vector3.one * 0.16f;
        var tm = lbl.AddComponent<TextMeshPro>();
        tm.text = "E\nN\nT\nR\nA\nN\nC\nE";
        tm.fontSize = 7;
        tm.alignment = TextAlignmentOptions.Center;
        tm.color = Theme.Green;
        if (Theme.Pixel != null) tm.font = Theme.Pixel;
        tm.sortingOrder = -3;
    }

    void BuildBench()
    {
        var bench = new GameObject("Bench");
        var sr = bench.AddComponent<SpriteRenderer>();
        sr.sprite = Pixels.Furniture("desk"); // plank wood texture
        sr.sortingOrder = 2;
        bench.transform.position = new Vector3(-5.4f, -3.9f, 0);
        bench.transform.localScale = new Vector3(3.4f, 0.4f, 1);
    }

    void BuildDesk()
    {
        var desk = new GameObject("RepairDesk");
        var sr = desk.AddComponent<SpriteRenderer>();
        sr.sprite = Pixels.Furniture("desk");
        sr.sortingOrder = 8;
        desk.transform.position = new Vector3(2.5f, -1.4f, 0);
        desk.transform.localScale = new Vector3(3.2f, 1.5f, 1);
        desk.AddComponent<BoxCollider2D>();
        desk.AddComponent<RepairDesk>();

        // tools on the desk
        Prop(desk.transform, "Toolbox",    -1.05f, 0.6f, 0.7f, 0.4f, "#2A3040", 9);
        Prop(desk.transform, "MonitorGlow", 0.95f, 0.62f, 0.62f, 0.42f, "#39FF14", 8);
        Prop(desk.transform, "Monitor",     0.95f, 0.6f, 0.55f, 0.35f, "#1A2233", 9);
        Prop(desk.transform, "Chip",        0.2f, 0.55f, 0.25f, 0.3f, "#5A6580", 9);

        // a couple of plants to soften the room
        Decor(Pixels.Furniture("plant"), new Vector3(-7.6f, -3.4f, 0), 1.6f, 3);
        Decor(Pixels.Furniture("plant"), new Vector3(7.8f, -3.4f, 0), 1.6f, 3);
        Decor(Pixels.Furniture("crate"), new Vector3(6.6f, -3.6f, 0), 1.1f, 3);
    }

    // A simple decorative sprite (no collider).
    void Decor(Sprite sprite, Vector3 pos, float scale, int order)
    {
        var go = new GameObject("Decor");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
    }

    // Adds a child sprite in the desk's LOCAL space (desk is non-uniformly scaled,
    // so use small local sizes).
    void Prop(Transform parent, string name, float lx, float ly, float w, float h, string hex, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(lx / 3.2f, ly / 1.5f, 0); // undo parent scale roughly
        go.transform.localScale = new Vector3(w / 3.2f, h / 1.5f, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = UIFactory.SolidSprite(Theme.Hex(hex));
        sr.sortingOrder = order;
    }

    void BuildPlayer()
    {
        var player = new GameObject("Player");
        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = Pixels.Character("tech");
        sr.sortingOrder = 30;
        player.transform.position = new Vector3(0f, -3f, 0);
        player.transform.localScale = Vector3.one * 1.5f;

        // shadow
        var shadow = new GameObject("Shadow");
        shadow.transform.SetParent(player.transform, false);
        shadow.transform.localPosition = new Vector3(0, 0.02f, 0);
        shadow.transform.localScale = new Vector3(0.7f, 0.18f, 1f);
        var shr = shadow.AddComponent<SpriteRenderer>();
        shr.sprite = UIFactory.SolidSprite(new Color(0, 0, 0, 0.4f));
        shr.sortingOrder = 29;

        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        var col = player.AddComponent<CircleCollider2D>();
        col.radius = 0.35f;
        col.offset = new Vector2(0, 0.45f);
        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerInteract>();
    }

    void BuildCustomers()
    {
        var spawnerGO = new GameObject("CustomerSpawner");
        var spawner = spawnerGO.AddComponent<CustomerSpawner>();
        spawner.spawnInterval = 10f;
        spawner.waitSpots = new[]
        {
            new Vector2(-5.5f, 1.3f),
            new Vector2(-5.5f, -0.4f),
            new Vector2(-3.6f, -3.4f),
        };
    }

    void BuildNeonSign()
    {
        // green frame behind a dark plate, with "FixIT" (green Fix + purple IT)
        var frame = new GameObject("NeonFrame");
        var frr = frame.AddComponent<SpriteRenderer>();
        frr.sprite = UIFactory.SolidSprite(Theme.Green);
        frr.sortingOrder = 6;
        frame.transform.position = new Vector3(-6.6f, 4.2f, 0);
        frame.transform.localScale = new Vector3(2.6f, 0.95f, 1);

        var plate = new GameObject("NeonPlate");
        var pr = plate.AddComponent<SpriteRenderer>();
        pr.sprite = UIFactory.SolidSprite(Theme.Hex("#080C0A"));
        pr.sortingOrder = 7;
        plate.transform.position = new Vector3(-6.6f, 4.2f, 0);
        plate.transform.localScale = new Vector3(2.45f, 0.78f, 1);

        var go = new GameObject("NeonSign");
        go.transform.position = new Vector3(-6.6f, 4.2f, 0);
        go.transform.localScale = Vector3.one * 0.32f;
        var tm = go.AddComponent<TextMeshPro>();
        tm.text = "<color=#39FF14>Fix</color><color=#9B6DFF>IT</color>";
        tm.fontSize = 12;
        tm.alignment = TextAlignmentOptions.Center;
        if (Theme.Pixel != null) tm.font = Theme.Pixel;
        tm.sortingOrder = 8;
    }
}
