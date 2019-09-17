using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TurnTracker.Common
{
    public static class LinqExtensions
    {
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
    }
}
