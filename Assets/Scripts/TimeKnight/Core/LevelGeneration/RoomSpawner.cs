using UnityEngine.Tilemaps;

namespace TimeKnight.Core.LevelGeneration
{
	public static class RoomSpawner
	{
		public static void FromNode(RoomNode node, Tilemap tilemap)
		{
			var positions = node.GetTileWorldPositions();
			foreach (var (pos, tile) in positions)
			{
				tilemap.SetTile(pos, tile);
			}
			tilemap.RefreshAllTiles();

			var behaviors = node.GetBehaviors();
			foreach (var behavior in behaviors)
			{
				behavior.OnSpawn(node);
			}
		}
	}
}