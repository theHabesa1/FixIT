using UnityEngine;
using TMPro;

// A back-store shelf holding one stock item. Click it to pick the item up.
public class Shelf : MonoBehaviour
{
    public Order item;
    SpriteRenderer sr;
    TextMeshPro label;

    public void Init(SpriteRenderer spriteRenderer, TextMeshPro lbl)
    {
        sr = spriteRenderer;
        label = lbl;
    }

    public void Set(Order o)
    {
        item = o;
        if (label) label.text = o != null ? o.product + "\n" + o.ValuesLine() : "EMPTY";
        // keep the pixel art at full colour when stocked; dim when empty
        if (sr) sr.color = o != null ? Color.white : new Color(0.5f, 0.5f, 0.55f, 1f);
    }

    public void MarkTaken()
    {
        item = null;
        if (label) label.text = "TAKEN";
        if (sr) sr.color = new Color(0.45f, 0.45f, 0.5f, 1f);
    }

    void OnMouseDown()
    {
        if (RoomManager.I != null) RoomManager.I.TryPickup(this);
    }
}
