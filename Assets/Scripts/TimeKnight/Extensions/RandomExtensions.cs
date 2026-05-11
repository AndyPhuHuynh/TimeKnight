using System;
using System.Collections.Generic;

namespace TimeKnight.Extensions
{
    public static class RandomExtensions
    {
        public static void ShuffleInPlace<T>(this IList<T> array, Random random)
        {
            for (var i = array.Count - 1; i > 0; i--)
            {
                var j = random.Next(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
    }
}