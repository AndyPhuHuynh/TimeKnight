using System.Collections.Generic;
using TimeKnight.Core.LevelGeneration;
using UnityEngine.Tilemaps;

namespace TimeKnight.Extensions
{
	public static class TileMapExtensions
	{
		public static IEnumerable<TileEntry> GetLocalTiles(this Tilemap tilemap)
		{
			foreach (var pos in tilemap.cellBounds.allPositionsWithin)
			{
				var tile = tilemap.GetTile(pos);
				if (tile is null) continue;
				yield return new TileEntry(pos, tile);
			}
		}
		
		public static void SetTiles(this Tilemap tilemap, IEnumerable<TileEntry> tiles)
		{
			foreach (var tile in tiles)
			{
				tilemap.SetTile(tile.Position, tile.Tile);
			}
		}
	}
}