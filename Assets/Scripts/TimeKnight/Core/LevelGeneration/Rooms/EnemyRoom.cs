using System.Collections.Generic;
using TimeKnight.Core.Enemy;
using UnityEngine;

namespace TimeKnight.Core.LevelGeneration.Rooms
{
	public class EnemyRoom : MonoBehaviour, IRoomBehavior
	{
		[SerializeField] private List<BasicEnemy> enemies = new();
		
		public void OnSpawn(RoomNode roomNode)
		{
			foreach (var enemy in enemies)
			{
				var offset = enemy.transform.position - roomNode.Definition.GetCenter();
				var newPos = roomNode.WorldPos + offset;
				Instantiate(enemy, newPos, Quaternion.identity);
			}
		}
	}
}