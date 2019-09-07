using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Utilities
{
    public static class CollectionExtensions
    {
        public static string Stringified<T>(this T[] self, string delimiter = null)
        {
            return self.Map(obj => obj.ToString()).Reduce(R.Concat(delimiter), "");
        }

        public static bool ContainsDuplicates<T>(this T[] self) where T: IEquatable<T>
        {
            return self.Distinct().Count() != self.Count();
        }

        public static string Stringified<T>(this List<T> self, string delimiter = null)
        {
            return self.ToArray().Stringified(delimiter);
        }

        public static bool ContainsDuplicates<T>(this IEnumerable<T> self) where T : IEquatable<T>
        {
            return self.ToArray().ContainsDuplicates();
        }

        public static string Stringified<T>(this HashSet<T> self, string delimiter = null)
        {
            return self.ToArray().Stringified(delimiter);
        }
    }
}
