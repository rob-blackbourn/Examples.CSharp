using System;
using System.Collections.Generic;

namespace JetBlack.Examples.NetworkServer2
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
        {
            foreach (var value in values)
                action(value);
        }
    }
}