using System;
using System.Collections.Generic;
using System.Linq;

namespace TurnTracker.Common
{
    public static class LinqExtensions
    {
        public static IEnumerable<TSource> EmptyIfNull<TSource>(this IEnumerable<TSource> source)
        {
            return source ?? Enumerable.Empty<TSource>();
        }

        public static IEnumerable<TSource> Tap<TSource>(this IEnumerable<TSource> source, Action<TSource> tap)
        {
            if(source is null) throw new ArgumentNullException(nameof(source));
            if(tap is null) throw new ArgumentNullException(nameof(tap));
            foreach (var item in source)
            {
                tap(item);
                yield return item;
            }
        }
        public static IEnumerable<TSource> Tap<TSource>(this IEnumerable<TSource> source, Action<int, TSource> tap)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (tap is null) throw new ArgumentNullException(nameof(tap));
            var i = 0;
            foreach (var item in source)
            {
                tap(i++, item);
                yield return item;
            }
        }

        public static bool IsSubsetOf<TSource>(this IEnumerable<TSource> subset, IEnumerable<TSource> superset)
        {
            return !subset.Except(superset).Any();
        }

        public static bool ContainsSubset<TSource>(this IEnumerable<TSource> superset, IEnumerable<TSource> subset)
        {
            return !subset.Except(superset).Any();
        }

        public static IEnumerable<TSource> Yield<TSource>(this TSource source)
        {
            yield return source;
        }
    }
}
