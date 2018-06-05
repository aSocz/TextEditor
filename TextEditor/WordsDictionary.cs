using System;
using System.Collections.Generic;
using System.Linq;
using TextEditor.Properties;

namespace TextEditor
{
    public class WordsDictionary
    {
        public WordsDictionary()
        {
            var numberOfCores = Environment.ProcessorCount;
            Dictionaries = GetDictionaries(numberOfCores);
        }

        public HashSet<string>[] Dictionaries { get; }

        private static HashSet<string>[] GetDictionaries(int numberOfCores)
        {
            var allWords = GetWordsList().ToList();
            var index = allWords.FindIndex(w => w == "sie");
            var x = allWords[index];
            var dictionaries = new HashSet<string>[numberOfCores];

            for (var i = 0; i < numberOfCores; i++)
            {
                dictionaries[i] = new HashSet<string>(allWords.Where((w, idx) => idx % numberOfCores == i));
            }

            return dictionaries;
        }

        private static IEnumerable<string> GetWordsList()
        {
            return GetResource()
                  .Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                  .Select(s => s.Trim())
                  .Where(s => !s.Contains(' '));
        }

        private static string GetResource() => Resources.PolishWords;
    }
}