using UnityEngine.Tilemaps;

namespace TimeKnight.Core.LevelGeneration
{
	public static class RoomSpawner
	{
		public static void FromNode(RoomNode node, Tilemap terrain, Tilemap background)
		{
			var terrainTiles = node.GetTileWorldPositions();
			foreach (var (pos, tile) in terrainTiles)
			{
				terrain.SetTile(pos, tile);
			}
			terrain.RefreshAllTiles();
			
			var backgroundTiles = node.GetBackgroundWorldPositions();
			foreach (var (pos, tile) in backgroundTiles)
			{
				background.SetTile(pos, tile);
			}
			background.RefreshAllTiles();

			var behaviors = node.GetBehaviors();
			foreach (var behavior in behaviors)
			{
				behavior.OnSpawn(node);
			}
		}
	}
}