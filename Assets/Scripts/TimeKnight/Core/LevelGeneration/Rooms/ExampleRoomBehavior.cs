using UnityEngine;

namespace TimeKnight.Core.LevelGeneration.Rooms
{
	public class ExampleRoomBehavior : MonoBehaviour, IRoomBehavior
	{
		public void OnSpawn(RoomSpawn room)
		{
			Debug.Log($"We just spawned in a room! {room.InstantiatedDefinition.gameObject.name}");
		}
	}
}