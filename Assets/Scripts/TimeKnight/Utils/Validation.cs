using UnityEngine;

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
	}
}