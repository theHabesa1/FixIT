using UnityEngine;

public static class BinaryPuzzleGen
{
    public class Puzzle
    {
        public int    correctAnswer;
        public string displayString; // binary or hex text shown to player
        public bool   isHex;
        public int    bitDepth;
    }

    public static Puzzle Generate(int level)
    {
        int bitDepth = level == 0 ? 4 : 8;
        bool isHex   = level == 2;
        int  max     = (int)Mathf.Pow(2, bitDepth);
        int  answer  = Random.Range(0, max);

        string display = isHex
            ? "0x" + System.Convert.ToString(answer, 16).ToUpper().PadLeft(2, '0')
            : System.Convert.ToString(answer, 2).PadLeft(bitDepth, '0');

        // add spaces between binary digits for readability
        if (!isHex)
        {
            string spaced = "";
            for (int i = 0; i < display.Length; i++)
                spaced += display[i] + (i < display.Length - 1 ? " " : "");
            display = spaced;
        }

        return new Puzzle { correctAnswer = answer, displayString = display, isHex = isHex, bitDepth = bitDepth };
    }
}
