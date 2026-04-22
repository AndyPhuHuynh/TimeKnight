using UnityEngine;
using Object = UnityEngine.Object;

namespace TimeKnight.Core.LevelGeneration
{
	// Represents a Room that has been spawned and instantiated into the scene
	public class RoomSpawn
	{
		public RoomDefinition InstantiatedDefinition { get; private set; }= null!;
		private IRoomBehavior[] _behaviors = null!;

		private RoomSpawn() {}

		public static RoomSpawn FromNode(RoomNode node, string objectName)
		{
			var obj = Object.Instantiate(node.Definition, node.WorldPos, Quaternion.identity);
			obj.name = objectName;
			
			var behaviors = obj.GetComponentsInChildren<IRoomBehavior>();
			var roomSpawn = new RoomSpawn
			{
				InstantiatedDefinition = obj,
				_behaviors =  behaviors,
			};
			
			foreach (var behavior in behaviors)
			{
				behavior.OnSpawn(roomSpawn);
			}
			
			return roomSpawn;
		}
	}
}