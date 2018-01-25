using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Questions.Model
{
    public class Fuzzy
    {
        /// <summary>
        /// Computes the Damerau-Levenshtein Distance between two strings, represented as arrays of
        /// integers, where each integer represents the code point of a character in the source string.
        /// Includes an optional threshhold which can be used to indicate the maximum allowable distance.
        /// </summary>
        /// <param name="source">An array of the code points of the first string</param>
        /// <param name="target">An array of the code points of the second string</param>
        /// <param name="threshold">Maximum allowable distance</param>
        /// <returns>Int.MaxValue if threshhold exceeded; otherwise the Damerau-Leveshteim distance between the strings</returns>
        public static int DamerauLevenshteinDistance(string source, string target, int threshold)
        {

            int length1 = source.Length;
            int length2 = target.Length;

            // Return trivial case - difference in string lengths exceeds threshhold
            if (Math.Abs(length1 - length2) > threshold) { return int.MaxValue; }

            // Ensure arrays [i] / length1 use shorter length 
            if (length1 > length2)
            {
                Swap(ref target, ref source);
                Swap(ref length1, ref length2);
            }

            int maxi = length1;
            int maxj = length2;

            int[] dCurrent = new int[maxi + 1];
            int[] dMinus1 = new int[maxi + 1];
            int[] dMinus2 = new int[maxi + 1];
            int[] dSwap;

            for (int i = 0; i <= maxi; i++) { dCurrent[i] = i; }

            int jm1 = 0, im1 = 0, im2 = -1;

            for (int j = 1; j <= maxj; j++)
            {

                // Rotate
                dSwap = dMinus2;
                dMinus2 = dMinus1;
                dMinus1 = dCurrent;
                dCurrent = dSwap;

                // Initialize
                int minDistance = int.MaxValue;
                dCurrent[0] = j;
                im1 = 0;
                im2 = -1;

                for (int i = 1; i <= maxi; i++)
                {

                    int cost = source[im1] == target[jm1] ? 0 : 1;

                    int del = dCurrent[im1] + 1;
                    int ins = dMinus1[i] + 1;
                    int sub = dMinus1[im1] + cost;

                    //Fastest execution for min value of 3 integers
                    int min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

                    if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
                        min = Math.Min(min, dMinus2[im2] + cost);

                    dCurrent[i] = min;
                    if (min < minDistance) { minDistance = min; }
                    im1++;
                    im2++;
                }
                jm1++;
                if (minDistance > threshold) { return int.MaxValue; }
            }

            int result = dCurrent[maxi];
            return (result > threshold) ? int.MaxValue : result;
        }

        internal static bool AreSimilar(string userResponse, string parentResponseForInvokingThisChildQuestion)
        {
            int threshold = 2;
            if (userResponse == null || parentResponseForInvokingThisChildQuestion == null) return false;
            return DamerauLevenshteinDistance(userResponse.ToLower(), parentResponseForInvokingThisChildQuestion.ToLower(), threshold) <= threshold;
        }

        public static int DamerauLevenshteinDistanceOnePrefix(string source, string target, int threshold)
        {
            if (source[0] == target[0])
            {
                return DamerauLevenshteinDistance(source, target, threshold);
            }
            else
                return Int32.MaxValue;
        }

        static void Swap<T>(ref T arg1, ref T arg2)
        {
            T temp = arg1;
            arg1 = arg2;
            arg2 = temp;
        }

        public static int LevenshteinDistance(string a, string b)
        {
            if (String.IsNullOrEmpty(a) || String.IsNullOrEmpty(b)) return 0;

            int lengthA = a.Length;
            int lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];
            for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
            for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

            for (int i = 1; i <= lengthA; i++)
                for (int j = 1; j <= lengthB; j++)
                {
                    int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                    distances[i, j] = Math.Min
                        (
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost
                        );
                }
            return distances[lengthA, lengthB];
        }

        public static string GetBestMatch(IEnumerable<string> source, string target)
        {
            return GetBestMatch(source, target, true);
        }

        public static string GetExactMatch(IEnumerable<string> source, string target, bool ignoreCase)
        {
            IEnumerable<Tuple<string, string>> sourceObject = null;
            if (ignoreCase)
            {
                sourceObject = source.Select(s => new Tuple<string, string>(s, s.ToLower()));
                target = target.ToLower();
            }
            else
            {
                sourceObject = source.Select(s => new Tuple<string, string>(s, s));
            }
            var match = sourceObject.Where(s => s.Item2 == target).FirstOrDefault();
            if (match != null)
                return match.Item1;
            else
                return null;
        }

        public static string GetBestMatch(IEnumerable<string> source, string target, bool ignoreCase)
        {
            IEnumerable<Tuple<string, string>> sourceObject = null;
            if (ignoreCase)
            {
                sourceObject = source.Select(s => new Tuple<string, string>( s, s.ToLower() ));
                target = target.ToLower();
            }
            else
            {
                sourceObject = source.Select(s => new Tuple<string, string>(s, s));
            }
            var matches = sourceObject.Where(s => s.Item2.Trim().Length > 0).Select(s =>
              {
                  int targetFragmentCount = target.Split(' ').Count();
                  int sourceFragmentCount = s.Item2.Split(' ').Count();

                  string sourceForMatching = s.Item2;
                  string targetFOrMatching = target;
                  if (sourceFragmentCount < targetFragmentCount)
                  {
                      sourceForMatching = target;
                      targetFOrMatching = s.Item2;
                  }

                  var nGrams = GetNGrams(sourceForMatching, targetFOrMatching.Split(' ').Count());
                  int v = SuggestEditDistanceTolerance(targetFOrMatching);
                  int minDistance = nGrams.Min(t => Fuzzy.DamerauLevenshteinDistance(t, targetFOrMatching, v));
                  return new { Source = s, MinDIsctance = minDistance, LengthDifference = Math.Abs(s.Item2.Length - target.Length)  };
              })
            .OrderBy(s => s.MinDIsctance).ThenBy(s=>s.LengthDifference);
            var match = matches.FirstOrDefault();
            if (match != null && match.MinDIsctance < Int32.MaxValue)
            {
                return match.Source.Item1;
            }
            return null;
        }

        internal static string GetBestMatch(List<Tuple<string, string>> list, string target)
        {
            var commnonList = list.SelectMany(s => s.Item2.Split(',').Select(t => t.Trim().ToLower()));
            var match = GetBestMatch(commnonList, target.ToLower());
            var candidate = list.FirstOrDefault(s => s.Item2.ToLower().Contains(match));
            if (candidate != null) return candidate.Item1; else return null;
        }

        public static bool DamerauLevenshteinDistanceNGram(string source, string searchString, int v)
        {
            int count = searchString.Split(' ').Count();
            var nGrams = GetNGrams(source, count);
            return nGrams.Any(s => Fuzzy.DamerauLevenshteinDistance(s, searchString, v) < v + 1);
        }

        public static bool LevenshteinDistanceNGram(string source, string searchString, int v)
        {
            int count = searchString.Split(' ').Count();
            var nGrams = GetNGrams(source, count);
            return nGrams.Any(s => Fuzzy.LevenshteinDistance(s, searchString) < v + 1);
        }

        public static int SuggestEditDistanceTolerance(string searchString)
        {
            return (int)Math.Round(Math.Log(searchString.Length), 0) + 1;
        }

        public static List<string> GetNGrams(string source, int count)
        {
            var splits = source.Split(' ');
            var result = GetCombinations(count, splits);
            result.AddRange(GetCombinations(count, splits.Reverse()));
            return result;
        }

        private static List<string> GetCombinations(int count, IEnumerable<string> splits)
        {
            List<string> result = new List<string>();
            int skip = 0;
            while (skip < splits.Count() - count)
            {
                var combo = splits.Skip(skip).Take(count).Aggregate((a, b) => $"{a} {b}");
                result.Add(combo);
                skip++;
            }
            var leftOver = splits.Skip(skip).Aggregate((a, b) => $"{a} {b}");
            if (!string.IsNullOrWhiteSpace(leftOver)) result.Add(leftOver);
            return result;
        }

        /*

        // Read this excellend discssion on StackOverflow: 
        // https://stackoverflow.com/questions/9453731/how-to-calculate-distance-similarity-measure-of-given-2-strings
        public static bool HasSmithWatermanGotohMatch(SmithWatermanGotoh swg, string name, string searchName, double threshold)
        {
            //if (string.IsNullOrWhiteSpace(one) || string.IsNullOrWhiteSpace(two)) return 0.0;
            return swg.GetSimilarity(searchName, name) > threshold;
        }

        public static double GetSimilarityLevenstein(string one, string two)
        {
            if (string.IsNullOrWhiteSpace(one) || string.IsNullOrWhiteSpace(two)) return 0.0;

            return new Levenstein().GetSimilarity(one, two);
        }

        public static double GetSimilaritySmithWatermanGotoh(string one, string two)
        {
            if (string.IsNullOrWhiteSpace(one) || string.IsNullOrWhiteSpace(two)) return 0.0;
            return new SmithWatermanGotoh().GetSimilarity(one, two);
        }
        */
    }
}


