using System;
using System.Collections.Generic;
using System.Linq;

namespace TradeDesk.NWayCache
{
    /// <summary>
    /// Implementation of frequently used cache updating algorithms
    /// </summary>
    /// <typeparam name="K">Type of keys in cache</typeparam>
    /// <typeparam name="V">Type of values in cache</typeparam>
    public static class ReplacementAlgorithm<K, V>
        where K : IComparable<K>
        where V : IComparable<V>
    {
        /// <summary>
        /// Each bucket consists of cache lines.
        /// If there is no line to place a new value in cache, select the least recently used line.
        /// </summary>
        /// <param name="bucket">Collection of cache lines</param>
        public static void LRU(IEnumerable<Line<K, V>> bucket)
        {
            // Select the very first least recently accessed cache line
            //
            //  null -> will use MinValue to compare                 [First element]
            //  null -> will use MinValue to compare
            //  2011-09-04
            //  2012-01-01
            //  2012-04-01
            //
            //
            // OrderBy orders items ascending
            bucket.OrderBy(line => line.Usage.LastAccessed ?? DateTime.MinValue)
                  .First()
                  .Invalidate();
        }

        /// <summary>
        /// Each bucket consists of cache lines.
        /// If there is no line to place a new value in cache, select the most recently used line.
        /// </summary>
        /// <param name="bucket">Collection of cache lines</param>
        public static void MRU(IEnumerable<Line<K, V>> bucket)
        {
            // Select the very first least recently accessed cache line
            //
            //  2012-04-01                                           [First element]
            //  2012-01-01
            //  2011-09-04
            //  null -> will be at the bottom
            //  null -> will be at the bottom
            //
            bucket.OrderByDescending(line => line.Usage.LastAccessed)
                  .First()
                  .Invalidate();
        }
    }
}
