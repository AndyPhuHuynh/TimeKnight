using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TimeKnight.Extensions
{
    public static class ExtensionMethods
    {
        public static bool IsEmpty<T>(this IReadOnlyCollection<T> list) => list.Count == 0;
        
        public static void SetVisible(this CanvasGroup group, bool visible)
        {
            if (group == null) return;
            group.alpha = visible ? 1 : 0;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        public static Vector3Int FloorToInt(this Vector3 vec)
        {
            return new Vector3Int(Mathf.FloorToInt(vec.x),  Mathf.FloorToInt(vec.y), Mathf.FloorToInt(vec.z));
        }
        
        public static Dictionary<Vector3Int, TileBase> GetLocalPositions(this Tilemap tilemap)
        {
            var result = new Dictionary<Vector3Int, TileBase>();
            foreach (var pos in tilemap.cellBounds.allPositionsWithin)
            {
                var tile = tilemap.GetTile(pos);
                if (tile is null) continue;
                result.Add(pos, tile);
            }
            return result;
        }
    }
}
