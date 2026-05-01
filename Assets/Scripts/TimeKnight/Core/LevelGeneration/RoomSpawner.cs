using System.Linq;
using TimeKnight.Extensions;
using UnityEngine.Tilemaps;

namespace TimeKnight.Core.LevelGeneration
{
	public static class RoomSpawner
	{
		public static void FromNode(RoomNode node, Tilemap terrain, Tilemap background)
		{
			var terrainTiles = node.GetTileWorldPositions();
			terrain.SetTiles(terrainTiles);
			foreach (var connection in node.Connections.Where(c => !c.IsConnected))
			{
				if (connection.Definition.fillTilemap == null) continue;
				
				var worldPos = node.WorldPos.FloorToInt();
				var worldPositionTiles = connection.Definition.fillTilemap.GetLocalTiles()
					.Select(t => new TileEntry(t.Position + worldPos, t.Tile)).ToList();
				terrain.SetTiles(worldPositionTiles);
			}
			terrain.RefreshAllTiles();
			
			var backgroundTiles = node.GetBackgroundWorldPositions();
			background.SetTiles(backgroundTiles);
			background.RefreshAllTiles();
			
			var behaviors = node.GetBehaviors();
			foreach (var behavior in behaviors)
			{
				behavior.OnSpawn(node);
			}
		}
	}
}