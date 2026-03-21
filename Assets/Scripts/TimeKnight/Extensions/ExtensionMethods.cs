using System.Collections.Generic;
using UnityEngine;

namespace TimeKnight.Extensions
{
    public static class ExtensionMethods
    {
        public static bool IsEmpty<T>(this LinkedList<T> list) => list == null || list.Count == 0;
        public static bool IsEmpty<TK, TV>(this Dictionary<TK, TV> dictionary) => dictionary == null || dictionary.Count == 0;
        
        public static void SetVisible(this CanvasGroup group, bool visible)
        {
            if (group == null) return;
            group.alpha = visible ? 1 : 0;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
