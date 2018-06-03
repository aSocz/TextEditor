using System.Linq;

namespace TextEditor
{
    public class Levenshtein
    {
        public int CalculateLevenshteinDistance(string text, string pattern)
        {
            if (text == null && pattern == null)
                return 0;
            if (string.IsNullOrEmpty(text))
                return pattern.Length;
            if (string.IsNullOrEmpty(pattern))
                return text.Length;

            var tempResults = new int[text.Length + 1, pattern.Length + 1];
            for (short i = 0; i < text.Length + 1; i++)
            {
                for (short j = 0; j < pattern.Length + 1; j++)
                {
                    if (i == 0)
                        tempResults[i, j] = j;
                    if (j == 0)
                        tempResults[i, j] = i;
                    if (i == 0 || j == 0)
                        continue;

                    var isSubstitution = char.ToLower(text.ElementAt(i - 1)) != char.ToLower(pattern.ElementAt(j - 1));
                    var deletionCase = tempResults[i - 1, j] + 1;
                    var insertionCase = tempResults[i, j - 1] + 1;
                    var otherCase = tempResults[i - 1, j - 1] + (isSubstitution ? 1 : 0);
                    tempResults[i, j] = new[] { deletionCase, insertionCase, otherCase }.Min();
                }
            }

            return tempResults[text.Length, pattern.Length];
        }
    }
}
