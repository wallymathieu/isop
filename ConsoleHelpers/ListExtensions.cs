using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleHelpers
{
    public static class ListExtensions
    {
        public static IEnumerable<KeyValuePair<int, TValue>> FindIndexAndValues<TValue>(this IList<TValue> self, Predicate<TValue> selection)
        {
            for (int i = 0; i < self.Count; i++)
            {
                var v = self[i];
                if (selection(v))
                    yield return new KeyValuePair<int, TValue>(i, v);
            }
        }
        public static TValue GetForIndexOrDefault<TValue>(this IList<TValue> self, int index)
        {
            if (index >= self.Count || index < 0)
                return default(TValue);
            return self[index];
        }
    }
}