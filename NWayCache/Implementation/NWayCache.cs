using System;
using System.Collections.Generic;
using System.Linq;

namespace TradeDesk.NWayCache
{
    #region Additional classes
    /// <summary>
    /// Stores usage information for each line in a cache bucket\set
    /// </summary>
    public class LineUsage
    {
        public LineUsage()
        {
        }

        public LineUsage(bool isValid, DateTime inserted, DateTime? lastAccessed, uint hits)
        {
            this.IsValid = isValid;
            this.Inserted = inserted;
            this.LastAccessed = lastAccessed;
            this.Hits = hits;
        }

        /// <summary>
        /// Creates a "valid" usage of a cache line
        /// </summary>
        /// <returns>Usage instance</returns>
        public static LineUsage CreateValid()
        {
            var usage = new LineUsage()
            {
                Hits = 0,
                Inserted = DateTime.Now,
                IsValid = true,
                LastAccessed = null,
            };

            return usage;
        }

        /// <summary>
        /// Creates an "invalid" usage of a cache line (i.e. pending update)
        /// </summary>
        /// <returns>Bucket instance</returns>
        public static LineUsage CreateInvalid()
        {
            var usage = CreateValid();
            usage.IsValid = false;
            return usage;
        }

        /// <summary>
        /// Flag which indicates that this line can be updated
        /// </summary>
        public bool IsValid { get; internal set; }

        /// <summary>
        /// DateTime when line was updated
        /// </summary>
        public DateTime Inserted { get; internal set; }

        /// <summary>
        /// DateTime when line was accessed
        /// </summary>
        public DateTime? LastAccessed { get; internal set; }

        /// <summary>
        /// Count of line hits
        /// </summary>
        public uint Hits { get; internal set; }
    }

    /// <summary>
    /// Line in each cache bucket
    /// </summary>
    /// <typeparam name="Key">Type of keys</typeparam>
    /// <typeparam name="Value">Type of values</typeparam>
    public class Line<K, V>
    {
        public Line() :
            this(default(K), default(V))
        {
        }

        public Line(K key, V value) :
            this(key, value, new LineUsage())
        {
        }
        public Line(K key, V value, LineUsage usage)
        {
            this.Key = key;
            this.Value = value;
            this.Usage = usage;
        }

        public Line<K, V> Invalidate()
        {
            this.Usage.IsValid = false;
            return this;
        }

        public Line<K, V> Validate()
        {
            this.Usage.IsValid = true;
            return this;
        }

        public K Key { get; internal set; }
        public V Value { get; internal set; }
        public LineUsage Usage { get; internal set; }
    }
    #endregion

    /// <summary>
    /// N-way set-associative in-memory cache
    /// </summary>
    /// <typeparam name="K">Type of keys in cache</typeparam>
    /// <typeparam name="V">Type of values in cache</typeparam>
    public class NWayCache<K, V>
       where K : IComparable<K>
       where V : IComparable<V>
    {
        #region Private fields
        /// <summary>
        /// The actual cache representation.
        /// </summary>
        private Line<K, V>[] cache;
        #endregion

        #region Public fields
        /// <summary>
        /// Delegate on cache replacement\update algorithm (e.g. MRU, LRU and etc.)
        /// </summary>
        public Action<IEnumerable<Line<K, V>>> ReplacementAlgorithm { get; private set; }

        /// <summary>
        /// Delegate which is invoked when a cache miss occurs
        /// </summary>
        public Func<K, V> MissHandler { get; private set; }

        /// <summary>
        /// Count of buckets (or sets) in cache
        /// </summary>
        public ushort Buckets { get; private set; }

        /// <summary>
        /// N-way value (i.e. count of lines in each cache)
        /// </summary>
        public ushort N { get; private set; }

        /// <summary>
        /// Total count of cache hits
        /// </summary>
        public uint Hits { get; private set; }

        /// <summary>
        /// Total count of cache misses
        /// </summary>
        public uint Misses { get; private set; }
        #endregion

        #region Public ctors, methods, operators
        public NWayCache(ushort n, ushort buckets, Action<IEnumerable<Line<K, V>>> replacementAlgorithm, Func<K, V> missHandler)
        {
            if (n == 0 || buckets == 0 || replacementAlgorithm == null || missHandler == null)
                throw new ArgumentException("Incorrect parameters");

            this.Buckets = buckets;
            this.N = n;

            // Cache consist of (buckets*n-way) of cache lines
            this.cache = new Line<K, V>[buckets * n];

            for (var i = 0; i < this.cache.Length; ++i)
                this.cache[i] = new Line<K, V>();

            this.MissHandler = missHandler;
            this.ReplacementAlgorithm = replacementAlgorithm;
        }

        public V this[K i]
        {
            get { return Get(i); }
            set { Set(i, value); }
        }

        public bool Contains(K key)
        {
            try
            {
                Get(key);
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Private methods

        private V InvokeMissHandler(K key)
        {
            try
            {
                return this.MissHandler(key);
            }
            catch(Exception ex)
            {
                throw new KeyNotFoundException(
                    String.Format("Cannot retrieve the value for provided key ({0)", key.ToString()),
                    ex);
            }
        }

        private V Get(K key)
        {
            var result = new Line<K, V>();

            try
            {
                // Look-up for key in every line which
                // 1) doesn't contain null key
                // 2) line is valid
                // 3) line.key equals to key
                result = GetBucket(key).First(line => line.Key != null && line.Usage.IsValid && line.Key.CompareTo(key) == 0);
                result.Usage.Hits++;
                result.Usage.LastAccessed = DateTime.Now;
                this.Hits++;
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException || ex is NullReferenceException)
                {
                    // No key in the bucket, i.e. cache miss occurred
                    this.Misses++;
                    result.Value = InvokeMissHandler(key);
                    Set(key, result.Value);
                }
            }

            return result.Value;
        }

        private void Set(K key, V value)
        {
            bool isBucketFull = true;
            int cacheIndex = -1;

            foreach (var index in GetBucketIndexesBy(GetBucketIdFrom(key)))
            {
                cacheIndex = index;
                if (!this.cache[index].Usage.IsValid)
                {
                    isBucketFull = false;
                    break;
                }
            }

            if (!isBucketFull)
            {
                this.cache[cacheIndex].Key = key;
                this.cache[cacheIndex].Value = value;
                this.cache[cacheIndex].Usage = LineUsage.CreateValid();
            }
            else
            {
                ReplacementAlgorithm(GetBucket(key));
                Set(key, value);
            }
        }

        private IEnumerable<Line<K, V>> GetBucket(K key)
        {
            foreach (var index in GetBucketIndexesBy(GetBucketIdFrom(key)))
                yield return this.cache[index];
        }

        private IEnumerable<int> GetBucketIndexesBy(int id)
        {
            for (var i = 0; i < this.N; ++i)
                // Calculate the offset in the cache lines array
                yield return id * this.N + i;
        }

        private int GetBucketIdFrom(K key)
        {
            return Math.Abs(key.GetHashCode() % this.Buckets);
        }
        #endregion
    }
}
