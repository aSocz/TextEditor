using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TextEditor
{
    public static class Extensions
    {
        public static IEnumerable<int> GetAllIndices(this string source, string word)
        {
            word = Regex.Escape(" " + word + " ");
            foreach (Match match in Regex.Matches(source, word, RegexOptions.IgnoreCase))
            {
                yield return match.Index + 1;
            }
        }
    }
}