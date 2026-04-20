using System.Collections.Generic;
using Random = System.Random;

namespace TimeKnight.Extensions
{
    public static class RandomUtils
    {
        public static T? GetRandomElement<T>(this IReadOnlyList<T> list, Random random)
        {
            if (list.IsEmpty()) return default;
            return list[random.Next(0, list.Count)];
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