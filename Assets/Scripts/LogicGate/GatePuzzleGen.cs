using System.Collections.Generic;
using UnityEngine;

// Generates a serialisable puzzle definition — no GameObjects, pure data.
public static class GatePuzzleGen
{
    public enum NodeType { Input, Gate, Output }

    public class PuzzleNode
    {
        public int         id;
        public NodeType    type;
        public Gate.GateType gateType;
        public List<int>   inputs = new List<int>(); // ids of upstream nodes
    }

    public class Puzzle
    {
        public List<PuzzleNode> nodes     = new List<PuzzleNode>();
        public List<int>        inputIds  = new List<int>();
        public int              outputId;
        public bool             targetOutput;
    }

    static readonly Gate.GateType[] AllGates =
    {
        Gate.GateType.AND, Gate.GateType.OR, Gate.GateType.NOT,
        Gate.GateType.NAND, Gate.GateType.NOR, Gate.GateType.XOR
    };

    public static Puzzle Generate(int level)
    {
        return level switch
        {
            0 => GenNovice(),
            1 => GenIntermediate(),
            _ => GenExpert(),
        };
    }

    // Novice: one 2-input gate
    static Puzzle GenNovice()
    {
        var p = new Puzzle();
        var a = Node(0, NodeType.Input);
        var b = Node(1, NodeType.Input);
        var gate = Node(2, NodeType.Gate, RandomGate(false)); // no NOT for 2-input
        gate.inputs.Add(0); gate.inputs.Add(1);
        var output = Node(3, NodeType.Output); output.inputs.Add(2);
        p.nodes.AddRange(new[] { a, b, gate, output });
        p.inputIds.Add(0); p.inputIds.Add(1);
        p.outputId = 3;
        p.targetOutput = Evaluate(p, RandomInputs(p));
        return p;
    }

    // Intermediate: 3 gates in a chain
    static Puzzle GenIntermediate()
    {
        var p = new Puzzle();
        // inputs: A, B, C, D
        for (int i = 0; i < 4; i++) { p.nodes.Add(Node(i, NodeType.Input)); p.inputIds.Add(i); }
        // gate1(A,B), gate2(C,D), gate3(gate1,gate2)
        var g1 = Node(4, NodeType.Gate, RandomGate(false)); g1.inputs.AddRange(new[]{0,1});
        var g2 = Node(5, NodeType.Gate, RandomGate(false)); g2.inputs.AddRange(new[]{2,3});
        var g3 = Node(6, NodeType.Gate, RandomGate(false)); g3.inputs.AddRange(new[]{4,5});
        var output = Node(7, NodeType.Output); output.inputs.Add(6);
        p.nodes.AddRange(new[] { g1, g2, g3, output });
        p.outputId = 7;
        p.targetOutput = Evaluate(p, RandomInputs(p));
        return p;
    }

    // Expert: 5 gates with branching (binary tree-ish)
    static Puzzle GenExpert()
    {
        var p = new Puzzle();
        for (int i = 0; i < 4; i++) { p.nodes.Add(Node(i, NodeType.Input)); p.inputIds.Add(i); }
        var g1 = Node(4, NodeType.Gate, RandomGate(false)); g1.inputs.AddRange(new[]{0,1});
        var g2 = Node(5, NodeType.Gate, RandomGate(true));  g2.inputs.Add(2); // NOT
        var g3 = Node(6, NodeType.Gate, RandomGate(false)); g3.inputs.AddRange(new[]{g1.id,g2.id});
        var g4 = Node(7, NodeType.Gate, RandomGate(false)); g4.inputs.AddRange(new[]{3,g1.id});
        var g5 = Node(8, NodeType.Gate, RandomGate(false)); g5.inputs.AddRange(new[]{g3.id,g4.id});
        var output = Node(9, NodeType.Output); output.inputs.Add(g5.id);
        p.nodes.AddRange(new[] { g1, g2, g3, g4, g5, output });
        p.outputId = 9;
        p.targetOutput = Evaluate(p, RandomInputs(p));
        return p;
    }

    public static bool Evaluate(Puzzle p, Dictionary<int,bool> values)
    {
        var result = new Dictionary<int, bool>(values);
        foreach (var n in p.nodes)
        {
            if (n.type == NodeType.Input) continue;
            bool a = n.inputs.Count > 0 && result.ContainsKey(n.inputs[0]) ? result[n.inputs[0]] : false;
            bool b = n.inputs.Count > 1 && result.ContainsKey(n.inputs[1]) ? result[n.inputs[1]] : false;
            // gate or output
            if (n.type == NodeType.Output) { result[n.id] = a; }
            else
            {
                result[n.id] = Gate.Eval(n.gateType, a, b);
            }
        }
        return result.ContainsKey(p.outputId) && result[p.outputId];
    }

    static Dictionary<int,bool> RandomInputs(Puzzle p)
    {
        var d = new Dictionary<int,bool>();
        foreach (int id in p.inputIds) d[id] = Random.value > 0.5f;
        return d;
    }

    static PuzzleNode Node(int id, NodeType t, Gate.GateType g = Gate.GateType.AND)
        => new PuzzleNode { id = id, type = t, gateType = g };

    static Gate.GateType RandomGate(bool allowNot)
    {
        if (allowNot) return AllGates[Random.Range(0, AllGates.Length)];
        // skip NOT (index 2)
        int[] safe = { 0, 1, 3, 4, 5 };
        return AllGates[safe[Random.Range(0, safe.Length)]];
    }
}
