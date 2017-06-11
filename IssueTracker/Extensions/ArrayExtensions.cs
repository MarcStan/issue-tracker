using System.Collections.Generic;
using System.Linq;

namespace IssueTracker.Extensions
{
    public static class ArrayExtensions
    {
        public static bool ContainsAll<T>(this T[] arr, IEnumerable<T> values)
        {
            return values.All(arr.Contains);
        }
    }
}