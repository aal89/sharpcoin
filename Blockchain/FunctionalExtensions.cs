using System;
using System.Collections.Generic;
using System.Linq;

namespace Blockchain
{
    public static class FunctionalExtensions
    {
        public static IEnumerable<R> Map<T, R>(this IEnumerable<T> self, Func<T, R> selector)
        {
            return self.Select(selector);
        }

        public static T Reduce<T>(this IEnumerable<T> self, Func<T, T, T> func)
        {
            return self.Aggregate(func);
        }

        public static IEnumerable<T> Filter<T>(this IEnumerable<T> self, Func<T, bool> predicate)
        {
            return self.Where(predicate);
        }
    }
}
