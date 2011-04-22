using System;
using System.Collections.Generic;

namespace Helpers
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
    }
}