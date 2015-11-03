using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase.Implementation
{
    public static class EnumerableExtensions
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
