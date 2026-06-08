using UnityEngine;

// Walk into this door to move between the shop and the back store.
[RequireComponent(typeof(BoxCollider2D))]
public class DoorTrigger : MonoBehaviour
{
    public bool toBack; // true: shop -> back store ; false: back store -> shop

    void OnTriggerEnter2D(Collider2D other)
    {
        if (RoomManager.I == null) return;
        if (other.GetComponent<PlayerController>() == null) return;
        RoomManager.I.Go(toBack);
    }
}
