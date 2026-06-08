using UnityEngine;

// Visual wire connecting two gate UI positions (uses a LineRenderer).
[RequireComponent(typeof(LineRenderer))]
public class Wire : MonoBehaviour
{
    public Transform from;
    public Transform to;

    LineRenderer lr;

    void Awake() { lr = GetComponent<LineRenderer>(); }

    void Update()
    {
        if (from && to)
        {
            lr.SetPosition(0, from.position);
            lr.SetPosition(1, to.position);
        }
    }
}
