using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TextEditor
{
    public static class Extensions
    {
        public static IEnumerable<int> GetAllIndices(this string source, string word)
        {
            word = "\\b" + Regex.Escape(word) + "\\b";
            foreach (Match match in Regex.Matches(source, word, RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                yield return match.Index;
            }
        }

        public static string Capitalize(this string word)
        {
            return word.First().ToString().ToUpper() + word.Substring(1);
        }
    }
}