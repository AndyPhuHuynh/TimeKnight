using UnityEngine.Tilemaps;

namespace TimeKnight.Core.LevelGeneration
{
	public static class RoomSpawn
	{
		public static void FromNode(RoomNode node, Tilemap tilemap)
		{
			var positions = node.GetTileWorldPositions();
			foreach (var (pos, tile) in positions)
			{
				tilemap.SetTile(pos, tile);
			}
			tilemap.RefreshAllTiles();

			var behaviors = node.GetAllBehaviors();
			foreach (var behavior in behaviors)
			{
				behavior.OnSpawn();
			}
		}
	}
}