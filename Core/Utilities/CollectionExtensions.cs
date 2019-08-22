using System;
using System.Collections.Generic;

namespace Core.Utilities
{
    public static class CollectionExtensions
    {
        public static string Stringified<T>(this T[] self, string delimiter = null)
        {
            return self.Map(obj => obj.ToString()).Reduce(R.Concat(delimiter), "");
        }

        public static string Stringified<T>(this List<T> self, string delimiter = null)
        {
            return self.ToArray().Stringified(delimiter);
        }
    }
}
