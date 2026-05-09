using System;
using System.Collections.Generic;

namespace TimeKnight.Extensions
{
    public static class RandomExtensions
    {
        public static T GetRandomElement<T>(this IReadOnlyList<T> list)
        {
            if (list.IsEmpty())
            {
                throw new InvalidOperationException("List is empty!");
            }
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
        
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