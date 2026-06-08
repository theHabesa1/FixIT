using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float speed = 4f;

    // Single fixed-screen shop (matches the design). Camera stays put; the player
    // is clamped to the visible play area instead of being followed.
    public bool followCamera = false;
    public Vector2 areaMin = new Vector2(-8.4f, -4.4f);
    public Vector2 areaMax = new Vector2( 8.4f,  3.2f);

    Rigidbody2D rb;
    Vector2 move;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        move.x = Input.GetAxisRaw("Horizontal");
        move.y = Input.GetAxisRaw("Vertical");
        move.Normalize();
    }

    void FixedUpdate()
    {
        Vector2 next = rb.position + move * speed * Time.fixedDeltaTime;
        next.x = Mathf.Clamp(next.x, areaMin.x, areaMax.x);
        next.y = Mathf.Clamp(next.y, areaMin.y, areaMax.y);
        rb.MovePosition(next);
    }

    void LateUpdate()
    {
        if (!followCamera || Camera.main == null) return;
        Vector3 target = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, target, 8f * Time.deltaTime);
    }
}
