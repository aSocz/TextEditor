using System;

namespace TextEditor
{
    public static class LevenshteinDamerau
    {
        public static int GetDistance(this string pattern, string value, int threshold)
        {
            var patternLength = pattern.Length;
            var valueLength = value.Length;

            // Return trivial case - difference in string lengths exceeds threshold
            if (Math.Abs(patternLength - valueLength) > threshold)
            {
                return int.MaxValue;
            }

            // Ensure arrays [i] / length1 use shorter length 
            if (patternLength > valueLength)
            {
                Swap(ref value, ref pattern);
                Swap(ref patternLength, ref valueLength);
            }

            var dCurrent = new int[patternLength + 1];
            var dMinus1 = new int[patternLength + 1];
            var dMinus2 = new int[patternLength + 1];

            for (var i = 0; i <= patternLength; i++)
            {
                dCurrent[i] = i;
            }

            var jm1 = 0;

            for (var j = 1; j <= valueLength; j++)
            {
                // Rotate
                var dSwap = dMinus2;
                dMinus2 = dMinus1;
                dMinus1 = dCurrent;
                dCurrent = dSwap;

                // Initialize
                var minDistance = int.MaxValue;
                dCurrent[0] = j;
                var im1 = 0;
                var im2 = -1;

                for (var i = 1; i <= patternLength; i++)
                {
                    var cost = pattern[im1] == value[jm1] ? 0 : 1;

                    var deletionCase = dCurrent[im1] + 1;
                    var insertionCase = dMinus1[i] + 1;
                    var substitutionCase = dMinus1[im1] + cost;

                    //Fastest execution for min value of 3 integers
                    var min = deletionCase > insertionCase ?
                        (insertionCase > substitutionCase ? substitutionCase : insertionCase) :
                        (deletionCase > substitutionCase ? substitutionCase : deletionCase);

                    if (i > 1 && j > 1 && pattern[im2] == value[jm1] && pattern[im1] == value[j - 2])
                    {
                        min = Math.Min(min, dMinus2[im2] + cost);
                    }

                    dCurrent[i] = min;
                    if (min < minDistance)
                    {
                        minDistance = min;
                    }

                    im1++;
                    im2++;
                }

                jm1++;
                if (minDistance > threshold)
                {
                    return int.MaxValue;
                }
            }

            var result = dCurrent[patternLength];
            return result > threshold ? int.MaxValue : result;
        }

        private static void Swap<T>(ref T arg1, ref T arg2)
        {
            var temp = arg1;
            arg1 = arg2;
            arg2 = temp;
        }
    }
}