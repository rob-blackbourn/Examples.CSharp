using System;
using System.Collections.Generic;

namespace JetBlack.Examples.NetworkClient4
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