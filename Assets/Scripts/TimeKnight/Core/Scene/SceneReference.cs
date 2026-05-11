using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.Scene
{
	[CreateAssetMenu(
		fileName = ScriptableObjectStrings.SceneReferenceFileName,
		menuName = ScriptableObjectStrings.SceneReferenceMenuName)]
	public class SceneReference : ScriptableObject
	{
		[field: SerializeField] public string SceneName { get; private set; } = null!;
		
		private void OnValidate()
		{
			Validation.NotNullOrEmpty(this, SceneName, nameof(SceneName));
		}
	}
}