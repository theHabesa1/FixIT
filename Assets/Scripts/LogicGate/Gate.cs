using UnityEngine;
using TMPro;

public class Gate : MonoBehaviour
{
    public enum GateType { AND, OR, NOT, NAND, NOR, XOR, XNOR }

    public GateType gateType;
    public TextMeshPro label;

    void Start()
    {
        if (label) label.text = gateType.ToString();
    }

    public bool Evaluate(bool a, bool b) => Eval(gateType, a, b);

    // Pure static evaluation — usable without a GameObject.
    public static bool Eval(GateType gateType, bool a, bool b)
    {
        return gateType switch
        {
            GateType.AND  => a && b,
            GateType.OR   => a || b,
            GateType.NOT  => !a,
            GateType.NAND => !(a && b),
            GateType.NOR  => !(a || b),
            GateType.XOR  => a ^ b,
            GateType.XNOR => !(a ^ b),
            _             => false,
        };
    }
}
