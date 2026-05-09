using System;
using System.Collections.Generic;
using TimeKnight.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace TimeKnight.Utils
{
	public static class Validation
	{
		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void NotNull(Object owner, Object field, string fieldName)
		{
			if (field == null)
			{
				Debug.LogError($"{fieldName} on {owner.name} is not assigned!", owner);
			}
		}
		
		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void NotNullOrEmpty(Object owner, string? field, string fieldName)
		{
			if (string.IsNullOrEmpty(field))
			{
				Debug.LogError($"{fieldName} on {owner.name} is not assigned!", owner);
			}
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void NotEmpty<T>(Object owner, IReadOnlyCollection<T> field, string fieldName)
		{
			if (field.IsEmpty())
			{
				Debug.LogError($"{fieldName} on {owner.name} is empty!", owner);
			}
		}
		
		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void NotFound(Object owner, Object field, string fieldName)
		{
			if (field == null)
			{
				Debug.LogError($"{fieldName} on {owner.name} was not found!", owner);
			}
		}
		
		public static bool IsExactPrefabAtPath(GameObject gameObject, string pathToCheck)
		{
#if UNITY_EDITOR
			// Check if we're currently editing this object in Prefab Mode
			try
			{
				var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
				if (prefabStage != null && prefabStage.IsPartOfPrefabContents(gameObject))
				{
					var stagePath = prefabStage.assetPath;
					if (string.IsNullOrEmpty(stagePath)) return true;
					return stagePath.Contains(pathToCheck);
				}
			}
			catch (InvalidOperationException)
			{
				// Not safe to query prefab stage yet (called during Awake/OnEnable).
				// Fall through to asset path check below.
			}
            
			// Check if this is a prefab asset in the projects tab
			var partOfPrefab = PrefabUtility.IsPartOfPrefabAsset(gameObject);
			if (!partOfPrefab) return false;
			
			// Could be in a prefab stage that isn't ready yet
			// Check the asset path via the prefab object itself as a fallback
			var sourcePrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
			if (sourcePrefab == null) return false;
			
			var sourcePath = AssetDatabase.GetAssetPath(sourcePrefab);
			if (string.IsNullOrEmpty(sourcePath)) return true;
			return sourcePath.Contains(pathToCheck);
#else
            return false;
#endif
		}
	}
}