using UnityEngine;
using UnityEngine.Tilemaps;

namespace TimeKnight.Core.LevelGeneration
{
	public struct TileEntry
	{
		public readonly Vector3Int Position;
		public readonly TileBase Tile;
		
		public TileEntry(Vector3Int position, TileBase tile) { Position = position; Tile = tile; }
	}
}