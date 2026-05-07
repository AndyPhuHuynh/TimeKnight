using System.Collections.Generic;
using System.Linq;
using TimeKnight.Extensions;
using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.LevelGeneration
{
	[CreateAssetMenu(
		fileName = ScriptableObjectStrings.RoomRegistryFileName,
		menuName = ScriptableObjectStrings.RoomRegistryMenuName)]
	public class RoomRegistry : ScriptableObject
	{
		[field: SerializeField] public List<RoomDefinition> AllRooms { get; private set; } = new();
		public readonly Dictionary<RoomType, List<RoomDefinition>> RoomsOfType = new();

		private readonly RoomType[] _allValidTypes = 
			EnumUtils.GetEnumValues<RoomType>()
				.Where(t => t != RoomType.None)
				.ToArray();
		
		private void OnEnable()
		{
			Initialize();
		}
		
		public void Initialize()
		{
			foreach (var type in _allValidTypes)
			{
				if (RoomsOfType.TryGetValue(type, out var rooms))
				{
					rooms.Clear();	
				}
				else
				{
					RoomsOfType[type] = new List<RoomDefinition>();
				}
			}
			
			for (var i = 0; i < AllRooms.Count; i++)
			{
				var room = AllRooms[i];
				
#if UNITY_EDITOR
				// Always reload from disk to get the latest saved data, bypassing Unity's stale in-memory cache
				var path = UnityEditor.AssetDatabase.GetAssetPath(room);
				if (!string.IsNullOrEmpty(path))
				{
					UnityEditor.AssetDatabase.ImportAsset(path);
					room = UnityEditor.AssetDatabase.LoadAssetAtPath<RoomDefinition>(path);
				}
#endif
				
				if (room == null)
				{
					Debug.LogError($"RoomRegistry: Room at index {i} is null", this);
					continue;
				}
				if (room.RoomType == RoomType.None)
				{
					Debug.LogError($"RoomRegistry: RoomType on {room.name} is None", room.gameObject);
					continue;
				}
				RoomsOfType[room.RoomType].Add(room);
			}

			foreach (var type in _allValidTypes)
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
		
		public static void ReinitializeAllRegistries()
		{
#if UNITY_EDITOR
			var guids = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(RoomRegistry)}");
			foreach (var guid in guids)
			{
				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				var registry = UnityEditor.AssetDatabase.LoadAssetAtPath<RoomRegistry>(path);
				registry?.Initialize();
			}
#endif
		}
	}
}