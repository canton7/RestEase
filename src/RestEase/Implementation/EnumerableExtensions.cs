using System.Collections.Generic;

namespace RestEase.Implementation
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> Concat<T>(T item1, IEnumerable<T> rest)
        {
            yield return item1;
            foreach (var other in rest)
            {
                yield return other;
            }
        }
    }
}
