using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Steam.Net.Utilities
{
    internal static class EnumerableUtils
    {
        internal static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
            => new ReadOnlyCollection<T>(enumerable?.ToList() ?? new List<T>());
    }
}
