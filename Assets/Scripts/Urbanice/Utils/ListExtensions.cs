using System.Collections.Generic;

namespace Urbanice.Utils
{
    /// <summary>
    /// TODO:
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