using System.Collections.Generic;
using System.Linq;
using TimeKnight.Extensions;
using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.LevelGeneration
{
	[CreateAssetMenu(fileName = "RoomRegistry", menuName = "Scriptable Objects/RoomRegistry")]
	public class RoomRegistry : ScriptableObject
	{
		[field: SerializeField] public List<RoomDefinition> AllRooms { get; private set; } = new();
		public readonly Dictionary<RoomType, List<RoomDefinition>> RoomsOfType = new();

		private readonly RoomType[] allValidTypes = 
			EnumUtils.GetEnumValues<RoomType>()
				.Where(t => t != RoomType.None)
				.ToArray();
		
		private void OnEnable()
		{
			foreach (var type in allValidTypes)
			{
				RoomsOfType[type] = new List<RoomDefinition>();
			}
			Initialize();
		}
		
		public void Initialize()
		{
			foreach (var type in allValidTypes)
			{
				RoomsOfType[type].Clear();
			}
			
			for (var i = 0; i < AllRooms.Count; i++)
			{
				var room = AllRooms[i];
				if (room == null)
				{
					Debug.LogError($"RoomRegistry: Room at index {i} is null", this);
					continue;
				}
				if (room.RoomType == RoomType.None) Debug.LogError($"RoomType on {room.name} is None", this);
				RoomsOfType[room.RoomType].Add(room);
			}

			foreach (var type in allValidTypes)
			{
				if (RoomsOfType[type].IsEmpty())
				{
					Debug.LogError($"RoomRegistry: No rooms with type {type} found", this);
				}
			}
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}
	}
}