using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// One node on the circuit grid (UI-based, circular). Built by CircuitManager.
public class Node : MonoBehaviour
{
    public int  gridX, gridY;
    public bool blocked;
    public bool isSource, isDest;
    public Vector2 uiPos;

    public List<Node> neighbours = new List<Node>();
    public Image image;

    public enum NodeState { Normal, Blocked, Source, Dest, Path, Correct, Error }

    public void SetVisual(NodeState s)
    {
        if (!image) return;
        image.color = s switch
        {
            NodeState.Blocked => Theme.Hex("#243044"),
            NodeState.Source  => Theme.Amber,
            NodeState.Dest    => Theme.Hex("#AAB4C8"),
            NodeState.Path    => Theme.Hex("#37B6FF"),
            NodeState.Correct => Color.white,
            NodeState.Error   => Theme.Red,
            _                 => Theme.Hex("#AAB4C8"),
        };
        // path/active nodes grow a little
        float scale = (s == NodeState.Path || s == NodeState.Correct) ? 1.4f : 1f;
        image.rectTransform.localScale = Vector3.one * scale;
    }
}
