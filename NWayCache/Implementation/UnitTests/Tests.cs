using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TradeDesk.NWayCache;

namespace TradeDesk.UnitTests
{
    [TestClass]
    public class StringStringCacheTests
    {
        private class DataSource
        {
            StringDictionary data = new StringDictionary();

            public DataSource()
            {
                data.Add("Gleb", "Karapish");
                data.Add("Albert", "Fong");
                data.Add("Sunjeeb", "Sun");
                data.Add("Sushant", "Di");
                data.Add("Stephen", "Chisa");
                data.Add("Robert", "McKinsey");
                data.Add("Bob", "Lomme");
            }

            public string Get(string key)
            {
                return this.data[key];
            }
        }

        /// <summary>
        /// Backing data source to store in cache
        /// </summary>
        DataSource datasource = new DataSource();

        /// <summary>
        /// Instance of n-way set associative cache
        /// </summary>
        NWayCache<string, string> cache;

        public void Init(ushort nWay, ushort nBuckets, Action<IEnumerable<Line<string, string>>> replacementAlgorithm)
        {
            this.cache = new NWayCache<string, string>(
                n: nWay, buckets: nBuckets,
                replacementAlgorithm: replacementAlgorithm,
                missHandler: this.datasource.Get);
        }

        [TestMethod]
        public void StringStringCache_Tests()
        {
            Init(nWay: 4, nBuckets: 2, replacementAlgorithm: ReplacementAlgorithm<string, string>.LRU);

            Assert.AreEqual("Karapish", this.cache["Gleb"]);
            Assert.IsNull(this.cache["NoGleb"]);
            Assert.AreEqual("Di", this.cache["Sushant"]);
            Assert.AreEqual("Di", this.cache["Sushant"]);
            Assert.AreEqual("McKinsey", this.cache["Robert"]);

            Assert.AreEqual(2, this.cache.Buckets);
            Assert.AreEqual(4, this.cache.N);
            Assert.AreEqual((uint)4, this.cache.Misses);
            Assert.AreEqual((uint)1, this.cache.Hits);
        }
    }

    [TestClass]
    public class IntStringCacheTests
    {
        /// <summary>
        /// Backing data source to store in cache
        /// </summary>
        HashSet<int> datasource = new HashSet<int>(new int[] { 3, 4, 5, 5, 7, 3, -9, 54395 });

        /// <summary>
        /// Instance of n-way set associative cache
        /// </summary>
        NWayCache<int, string> cache;

        static string[] unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
        static string[] tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

        private string Get(int key)
        {
            if (!this.datasource.Contains(key))
                throw new KeyNotFoundException();

            return NumberToWords(key);
        }

        private static string NumberToWords(int n)
        {
            if (n == 0)
                return "zero";

            if (n < 0)
                return "minus " + NumberToWords(Math.Abs(n));

            string words = "";

            if ((n / 1000000) > 0)
            {
                words += NumberToWords(n / 1000000) + " million ";
                n %= 1000000;
            }

            if ((n / 1000) > 0)
            {
                words += NumberToWords(n / 1000) + " thousand ";
                n %= 1000;
            }

            if ((n / 100) > 0)
            {
                words += NumberToWords(n / 100) + " hundred ";
                n %= 100;
            }

            if (n > 0)
            {
                if (n < 20)
                    words += unitsMap[n];
                else
                {
                    words += tensMap[n / 10];
                    if ((n % 10) > 0)
                        words += " " + unitsMap[n % 10];
                }
            }

            return words;
        }
        public void Init(ushort nWay, ushort nBuckets, Action<IEnumerable<Line<int, string>>> replacementAlgorithm)
        {
            this.cache = new NWayCache<int, string>(
                n: nWay, buckets: nBuckets,
                replacementAlgorithm: replacementAlgorithm,
                missHandler: this.Get);
        }

        [TestMethod]
        public void IntStringCache_Tests()
        {
            Init(nWay: 2, nBuckets: 2, replacementAlgorithm: ReplacementAlgorithm<int, string>.MRU);

            Assert.AreEqual("five", this.cache[5]);
            Assert.AreEqual("five", this.cache[5]);

            bool throws = false;
            try { Assert.AreEqual("zero", this.cache[0]); } catch (Exception) { throws = true; }
            Assert.IsTrue(throws);

            Assert.AreEqual("three", this.cache[3]);
            Assert.AreEqual("fifty four thousand three hundred ninety five", this.cache[54395]);
            Assert.AreEqual("minus nine", this.cache[-9]);

            Assert.AreEqual((uint)5, this.cache.Misses);
            Assert.AreEqual((uint)1, this.cache.Hits);
        }
    }

    [TestClass]
    public class ReplacementAlgorithmsTests
    {
        [TestMethod]
        public void MRU_AllLinesAccessed()
        {
            var bucket = new Line<int, string>[]
            {
                new Line<int, string>(0, "a", new LineUsage(true, DateTime.Now, DateTime.Now.AddMinutes(3), 0)),
                new Line<int, string>(-1, "b", new LineUsage(true, DateTime.Now, DateTime.Now.AddMinutes(1), 0)),
                new Line<int, string>(3, "c", new LineUsage(true, DateTime.Now, DateTime.Now.AddMinutes(2), 0)),
            };

            ReplacementAlgorithm<int, string>.MRU(bucket);

            Assert.IsFalse(bucket[0].Usage.IsValid);
            Assert.IsTrue(bucket[1].Usage.IsValid);
            Assert.IsTrue(bucket[2].Usage.IsValid);
        }

        [TestMethod]
        public void MRU_OneLineNotAccessed()
        {
            var bucket = new Line<int, string>[]
            {
                new Line<int, string>(0, "a", new LineUsage(true, DateTime.Now, null, 0)),
                new Line<int, string>(-1, "b", new LineUsage(true, DateTime.Now, DateTime.Now.AddMinutes(1), 0)),
                new Line<int, string>(3, "c", new LineUsage(true, DateTime.Now, DateTime.Now.AddMinutes(2), 0)),
            };

            ReplacementAlgorithm<int, string>.MRU(bucket);

            Assert.IsFalse(bucket[2].Usage.IsValid);
            Assert.IsTrue(bucket[0].Usage.IsValid);
            Assert.IsTrue(bucket[1].Usage.IsValid);
        }

        [TestMethod]
        public void MRU_AllLinesNotAccessed()
        {
            var bucket = new Line<int, string>[]
            {
                new Line<int, string>(0, "a", new LineUsage(true, DateTime.Now, null, 0)),
                new Line<int, string>(0, "b", new LineUsage(true, DateTime.Now, null, 0)),
                new Line<int, string>(0, "c", new LineUsage(true, DateTime.Now, null, 0)),
            };

            ReplacementAlgorithm<int, string>.MRU(bucket);

            Assert.IsFalse(bucket[0].Usage.IsValid);
            Assert.IsTrue(bucket[1].Usage.IsValid);
            Assert.IsTrue(bucket[2].Usage.IsValid);
        }

        [TestMethod]
        public void LRU_AllLinesAccessed()
        {
            var bucket = new Line<int, string>[]
            {
                new Line<int, string>(0, "a", new LineUsage(true, DateTime.Now, DateTime.Now.AddMinutes(1), 0)),
                new Line<int, string>(-1, "b", new LineUsage(true, DateTime.Now, DateTime.Now.AddMinutes(3), 0)),
                new Line<int, string>(3, "c", new LineUsage(true, DateTime.Now, DateTime.Now.AddMinutes(2), 0)),
            };

            ReplacementAlgorithm<int, string>.LRU(bucket);

            Assert.IsFalse(bucket[0].Usage.IsValid);
            Assert.IsTrue(bucket[1].Usage.IsValid);
            Assert.IsTrue(bucket[2].Usage.IsValid);
        }


        [TestMethod]
        public void LRU_OneLineNotAccessed()
        {
            var bucket = new Line<int, string>[]
            {
                new Line<int, string>(0, "a", new LineUsage(true, DateTime.Now, null, 0)),
                new Line<int, string>(-1, "b", new LineUsage(true, DateTime.Now, DateTime.Now.AddMinutes(1), 0)),
                new Line<int, string>(3, "c", new LineUsage(true, DateTime.Now, DateTime.Now.AddMinutes(2), 0)),
            };

            ReplacementAlgorithm<int, string>.LRU(bucket);

            Assert.IsFalse(bucket[0].Usage.IsValid);
            Assert.IsTrue(bucket[1].Usage.IsValid);
            Assert.IsTrue(bucket[2].Usage.IsValid);
        }

        [TestMethod]
        public void LRU_AllLinesNotAccessed()
        {
            var bucket = new Line<int, string>[]
            {
                new Line<int, string>(0, "a", new LineUsage(true, DateTime.Now, null, 0)),
                new Line<int, string>(0, "b", new LineUsage(true, DateTime.Now, null, 0)),
                new Line<int, string>(0, "c", new LineUsage(true, DateTime.Now, null, 0)),
            };

            ReplacementAlgorithm<int, string>.MRU(bucket);

            Assert.IsFalse(bucket[0].Usage.IsValid);
            Assert.IsTrue(bucket[1].Usage.IsValid);
            Assert.IsTrue(bucket[2].Usage.IsValid);
        }
    }
}
