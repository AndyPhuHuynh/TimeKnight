using System.Collections.Generic;
using UnityEngine;

namespace TimeKnight.Extensions
{
    public static class RandomUtils
    {
        public static T GetRandomElement<T>(this IReadOnlyList<T> list)
        {
            if (list.IsEmpty()) return default;
            return list[Random.Range(0, list.Count)];
        }
        
        public static void ShuffleInPlace<T>(this IList<T> array)
        {
            for (var i = array.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
    }
}