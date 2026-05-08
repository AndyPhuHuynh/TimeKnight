using TimeKnight.Core.Audio;
using TimeKnight.Core.Interaction;
using TimeKnight.Core.Scene;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TimeKnight.Core.EndingRoom
{
	public class OpenDoorInteractable : MonoBehaviour, IInteractable
	{
		[SerializeField] private SpriteRenderer topSpriteRenderer = null!;
		[SerializeField] private SpriteRenderer bottomSpriteRenderer = null!;

		[SerializeField] private Sprite topSpriteOpen = null!;
		[SerializeField] private Sprite bottomSpriteOpen = null!;

		[SerializeField] private SceneReference exitScene = null!;
		[SerializeField] private AudioClip endSceneMusic = null!;

		private bool _doorOpened;
		
		private void OnValidate()
		{
			Validation.NotNull(this, topSpriteRenderer, nameof(topSpriteRenderer));
			Validation.NotNull(this, bottomSpriteRenderer, nameof(bottomSpriteRenderer));
			Validation.NotNull(this, topSpriteOpen, nameof(topSpriteOpen));
			Validation.NotNull(this, bottomSpriteOpen, nameof(bottomSpriteOpen));
			Validation.NotNull(this, exitScene, nameof(exitScene));
			Validation.NotNull(this, endSceneMusic, nameof(endSceneMusic));
		}

		public string InteractionName => !_doorOpened ? "Open Door" : "End Demo";
		public void Interact()
		{
			if (!_doorOpened)
			{
				OpenDoor();
			}
			else
			{
				ExitScene();
			}
		}

		private void OpenDoor()
		{
			_doorOpened = true;
			topSpriteRenderer.sprite = topSpriteOpen;
			bottomSpriteRenderer.sprite = bottomSpriteOpen;
		}

		private void ExitScene()
		{
			SceneManager.LoadScene(exitScene.SceneName);
			AudioManager.Instance.FadeInMusic(endSceneMusic);
		}
	}
}