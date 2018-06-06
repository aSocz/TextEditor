using C5;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TextEditor
{
    public class Word
    {
        private const int HeapCapacity = 5;
        private ConcurrentBag<WordDistance> results;

        public Word(string value, WordsDictionary wordsDictionary)
        {
            Value = value.ToLower();
            IsCorrect = wordsDictionary.Dictionaries.Any(d => d.Contains(Value));
        }


        public string Value { get; }

        public bool IsCorrect { get; }

        public bool IsProcessed => IsCorrect || (SuggestedWords?.Any() ?? false);

        public IEnumerable<string> SuggestedWords { get; private set; }


        public async Task Process(WordsDictionary wordsDictionary)
        {
            if (IsCorrect)
            {
                return;
            }

            results = new ConcurrentBag<WordDistance>();
            await Task.Run(() => Parallel.ForEach(wordsDictionary.Dictionaries, d => GetTopDistances(d)));
            SuggestedWords = results.OrderBy(e => e.Distance).Select(e => e.Word).Distinct().Take(5).ToList();
        }

        private void GetTopDistances(IEnumerable<string> threadDictionary)
        {
            var comparer = new WordDistanceComparer();
            var heap = new IntervalHeap<WordDistance>(HeapCapacity, comparer);

            foreach (var entry in threadDictionary)
            {
                var threshold = heap.Any() ? heap.FindMax().Distance : int.MaxValue;
                var distance = Value.GetDistance(entry, threshold);

                if (heap.Count < HeapCapacity)
                {
                    heap.Add(new WordDistance(entry, distance));
                    continue;
                }

                if (distance >= threshold)
                {
                    continue;
                }

                heap.DeleteMax();
                heap.Add(new WordDistance(entry, distance));
            }

            foreach (var wordDistance in heap)
            {
                results.Add(wordDistance);
            }
        }
    }
}