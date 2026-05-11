using System.Collections;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TimeKnight.Core.Scene
{
	[RequireComponent(typeof(CanvasGroup))]
	public class TransitionCanvas : Singleton<TransitionCanvas>
	{
		private CanvasGroup _canvasGroup = null!;
		private CanvasGroupController _canvasGroupController = null!;

		protected override void Awake()
		{
			base.Awake();
			if (Instance != this) return;
			
			_canvasGroup = GetComponent<CanvasGroup>();
			Validation.NotFound(this, _canvasGroup, nameof(_canvasGroup));
			
			_canvasGroupController = new CanvasGroupController(this, _canvasGroup);
		}

		private void FadeToBlack()
		{
			_canvasGroupController.FadeIn(1.0f);
		}

		private void FadeToClear()
		{
			_canvasGroupController.FadeOut(1.0f);
		}
		
		public IEnumerator LoadGameScene(string sceneName)
		{
			FadeToBlack();
			var asyncOp = SceneManager.LoadSceneAsync(sceneName);
			while (asyncOp is { isDone: false })
			{
				yield return null;
			}
			FadeToClear();
		}
	}
}