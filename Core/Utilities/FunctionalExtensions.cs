using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public static class FunctionalExtensions
    {
        public static IEnumerable<R> Map<T, R>(this IEnumerable<T> self, Func<T, R> selector)
        {
            return self.Select(selector);
        }

        public static IEnumerable<R> Map<T, R>(this IEnumerable<T> self, Func<T, int, R> selector)
        {
            return self.Select(selector);
        }

        public static IEnumerable<(T, int)> Track<T>(this IEnumerable<T> self)
        {
            return self.Map((value, i) => (value, i));
        }

        public static T Reduce<T>(this IEnumerable<T> self, Func<T, T, T> func, T DefaultValue)
        {
            try
            {
                return self.Aggregate(func);
            }
            catch
            {
                return DefaultValue;
            }
        }

        public static IEnumerable<T> Filter<T>(this IEnumerable<T> self, Func<T, bool> predicate)
        {
            return self.Where(predicate);
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T[]> self)
        {
            return self.SelectMany(x => x);
        }

        public static IEnumerable<R> FlatMap<T, R>(this IEnumerable<T> self, Func<T, R[]> selector)
        {
            return self.Map(selector).Flatten();
        }
    }
}
