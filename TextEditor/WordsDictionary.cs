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
            var coreCount = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }

            Dictionaries = GetDictionaries(coreCount);
        }

        public HashSet<string>[] Dictionaries { get; }

        private static HashSet<string>[] GetDictionaries(int numberOfCores)
        {
            var allWords = GetWordsList().ToList();
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
                  .Select(s => s.ToLower().Trim())
                  .Where(s => !s.Contains(' '));
        }

        private static string GetResource() => Resources.PolishWords;
    }
}