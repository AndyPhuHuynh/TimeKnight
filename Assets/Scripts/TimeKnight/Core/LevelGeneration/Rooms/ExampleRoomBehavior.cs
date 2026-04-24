using UnityEngine;

namespace TimeKnight.Core.LevelGeneration.Rooms
{
	public class ExampleRoomBehavior : MonoBehaviour, IRoomBehavior
	{
		public void OnSpawn()
		{
			Debug.Log("We just spawned in a room!");
		}
	}
}