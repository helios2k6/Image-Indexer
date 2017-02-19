using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.DSA;
using System.Linq;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class BKTreeTests
    {
        public class ExampleMetric : IMetric<ExampleMetric>
        {
            public ushort Id { get; private set; }
            public int[] Data { get; private set; } // String of symbols

            public ExampleMetric(ushort id, int[] data)
            {
                Id = id;
                Data = data;
            }

            public ExampleMetric(int[] data) : this(0, data)
            {
            }

            public int CalculateDistance(ExampleMetric other)
            {
                return DistanceMetric.CalculateLeeDistance(
                    Data,
                    other.Data
                );
            }
        }

        #region Unit tests
        [TestMethod]
        public void BKTreeShouldCalculateVarietyOfDistances()
        {
            Assert.AreEqual(10,
                DistanceMetric.CalculateHammingDistance(
                    new byte[] { 0xEF, 0x35, 0x20 },
                    new byte[] { 0xAD, 0x13, 0x87 }));

            Assert.AreEqual(101,
                DistanceMetric.CalculateLeeDistance(
                    new int[] { 196, 105, 48 },
                    new int[] { 201, 12, 51 }));

            Assert.AreEqual(3,
                DistanceMetric.CalculateLevenshteinDistance(
                    "kitten",
                    "sitting"
                ));

        }

        [TestMethod]
        public void BKTreeShouldFindBestNodeWithDistance()
        {
            BKTree<ExampleMetric> tree = new BKTree<ExampleMetric>();

            ExampleMetric search = new ExampleMetric(new int[] { 365, 422, 399 });
            ExampleMetric best = new ExampleMetric(4, new int[] { 400, 400, 400 });

            tree.Add(new ExampleMetric(1, new int[] { 100, 100, 100 }));
            tree.Add(new ExampleMetric(2, new int[] { 200, 200, 200 }));
            tree.Add(new ExampleMetric(3, new int[] { 300, 300, 300 }));
            tree.Add(best);
            tree.Add(new ExampleMetric(5, new int[] { 500, 500, 500 }));

            Tuple<ExampleMetric, int> result = tree.FindClosestElement(search);

            Assert.AreEqual(58, DistanceMetric.CalculateLeeDistance(search.Data, best.Data));
            Assert.AreEqual(58, result.Item2);
            Assert.AreEqual(4, result.Item1.Id);
            Assert.AreEqual(best.Data, result.Item1.Data);
        }

        [TestMethod]
        public void BKTreeShouldQueryBestMatchesBelowGivenThreshold()
        {
            BKTree<ExampleMetric> tree = new BKTree<ExampleMetric>();

            ExampleMetric search = new ExampleMetric(new int[] { 399, 400, 400 });

            ExampleMetric best1 = new ExampleMetric(41, new int[] { 400, 400, 400 });
            ExampleMetric best2 = new ExampleMetric(42, new int[] { 403, 403, 403 });
            ExampleMetric best3 = new ExampleMetric(43, new int[] { 406, 406, 406 });

            tree.Add(new ExampleMetric(1, new int[] { 100, 100, 100 }));
            tree.Add(new ExampleMetric(2, new int[] { 200, 200, 200 }));
            tree.Add(new ExampleMetric(3, new int[] { 300, 300, 300 }));
            tree.Add(best1);
            tree.Add(best2);
            tree.Add(new ExampleMetric(5, new int[] { 500, 500, 500 }));

            // Query for match within distance of 1 (best1 is only expected result)
            IDictionary<ExampleMetric, int> results = tree.Query(search, 1);

            Assert.AreEqual(1, DistanceMetric.CalculateLeeDistance(search.Data, best1.Data));
            Assert.AreEqual(1, results.Values.ElementAt(0));
            Assert.AreEqual(41, results.Keys.ElementAt(0).Id);
            Assert.AreEqual(best1.Data, results.Keys.ElementAt(0).Data);

            // Query for match within distance of 10 (best1 & best2 are expected results)
            tree.Add(best3); // exercise adding another node after already queried
            results = tree.Query(search, 10);

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(1, DistanceMetric.CalculateLeeDistance(search.Data, best1.Data));
            Assert.AreEqual(10, DistanceMetric.CalculateLeeDistance(search.Data, best2.Data));
            Assert.IsTrue(results.Contains(new KeyValuePair<ExampleMetric, int>(best1, 1)));
            Assert.IsTrue(results.Contains(new KeyValuePair<ExampleMetric, int>(best2, 10)));

            // Query for matches within distance of 20 (best1, best2 & best3 are expected results)
            results = tree.Query(search, 20);

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(1, DistanceMetric.CalculateLeeDistance(search.Data, best1.Data));
            Assert.AreEqual(10, DistanceMetric.CalculateLeeDistance(search.Data, best2.Data));
            Assert.AreEqual(19, DistanceMetric.CalculateLeeDistance(search.Data, best3.Data));
            Assert.IsTrue(results.Contains(new KeyValuePair<ExampleMetric, int>(best1, 1)));
            Assert.IsTrue(results.Contains(new KeyValuePair<ExampleMetric, int>(best2, 10)));
            Assert.IsTrue(results.Contains(new KeyValuePair<ExampleMetric, int>(best3, 19)));
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void BKTreeShouldThrowUponAddingNullNode()
        {
            BKTree<ExampleMetric> tree = new BKTree<ExampleMetric>();

            tree.Add(new ExampleMetric(1, new int[] { 100, 200, 300 }));
            tree.Add(new ExampleMetric(2, new int[] { 110, 210, 310 }));
            tree.Add(new ExampleMetric(3, new int[] { 130, 230, 330 }));
            tree.Add(new ExampleMetric(4, new int[] { 140, 240, 340 }));

            tree.Add(null);
        }
        #endregion
    }

    public static class DistanceMetric
    {
        public static int CalculateLeeDistance(int[] source, int[] target)
        {
            if (source.Length != target.Length)
            {
                throw new Exception("Lee distance string comparisons must be of equal length.");
            }

            // Iterate both arrays simultaneously, summing absolute value of difference at each position
            return source
                .Zip(target, (v1, v2) => new { v1, v2 })
                .Sum(m => Math.Abs(m.v1 - m.v2));
        }

        public static int CalculateHammingDistance(byte[] source, byte[] target)
        {
            if (source.Length != target.Length)
            {
                throw new Exception("Hamming distance string comparisons must be of equal length.");
            }

            // Iterate both arrays simultaneously, summing count of bit differences of each byte
            return source
                .Zip(target, (v1, v2) => new { v1, v2 })
                .Sum(m =>
                // Wegner algorithm
                {
                    int d = 0;
                    int v = m.v1 ^ m.v2; // XOR values to find all dissimilar bits

                    // Count number of set bits
                    while (v > 0)
                    {
                        ++d;
                        v &= (v - 1);
                    }

                    return d;
                });
        }

        public static int CalculateLevenshteinDistance(string source, string target)
        {
            int[,] distance; // distance matrix
            int n; // length of first string
            int m; // length of second string
            int i; // iterates through first string
            int j; // iterates through second string
            char s_i; // ith character of first string
            char t_j; // jth character of second string
            int cost; // cost

            // Step 1
            n = source.Length;
            m = target.Length;
            if (n == 0)
                return m;
            if (m == 0)
                return n;
            distance = new int[n + 1, m + 1];

            // Step 2
            for (i = 0; i <= n; i++)
                distance[i, 0] = i;
            for (j = 0; j <= m; j++)
                distance[0, j] = j;

            // Step 3
            for (i = 1; i <= n; i++)
            {
                s_i = source[i - 1];

                // Step 4
                for (j = 1; j <= m; j++)
                {
                    t_j = target[j - 1];

                    // Step 5
                    if (s_i == t_j)
                        cost = 0;
                    else
                        cost = 1;

                    // Step 6
                    distance[i, j] =
                        Math.Min(
                            Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                            distance[i - 1, j - 1] + cost);
                }
            }

            // Step 7
            return distance[n, m];
        }
    }
}
