using System.Collections.Generic;

namespace TextEditor
{
    public struct WordDistance
    {
        public WordDistance(string word, int distance)
        {
            Word = word;
            Distance = distance;
        }

        public string Word { get; }
        public int Distance { get; }
    }

    public class WordDistanceComparer : IComparer<WordDistance>
    {
        public int Compare(WordDistance x, WordDistance y)
        {
            if (x.Distance > y.Distance)
                return 1;
            if (x.Distance == y.Distance)
                return 0;
            return -1;
        }
    }
}