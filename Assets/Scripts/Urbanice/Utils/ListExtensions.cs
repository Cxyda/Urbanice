using System.Collections.Generic;

namespace Urbanice.Utils
{
    /// <summary>
    /// This class provides <see cref="List"/> extension functionality for easier use
    /// </summary>
    public static class ListExtensions
    {
        public static List<T> AddMultiple<T>(this List<T> list, params T[] elements)
        {
            list.AddRange(elements);
            return list;
        }
    }
}