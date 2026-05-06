using System.Collections;
using UnityEngine;

namespace TimeKnight.Utils
{
	public class CanvasGroupController
	{
		private readonly MonoBehaviour _owner;
		private readonly CanvasGroup _canvasGroup;

		public CanvasGroupController(MonoBehaviour owner, CanvasGroup canvasGroup)
		{
			_owner = owner;
			_canvasGroup = canvasGroup;
		}

		private IEnumerator FadeInCoroutine(float durationSeconds)
		{
			_canvasGroup.interactable = true;
			_canvasGroup.alpha = 0;
			while (_canvasGroup.alpha < 1)
			{
				_canvasGroup.alpha += Time.deltaTime / durationSeconds;
				yield return null;
			}
			_canvasGroup.blocksRaycasts = true;
		}
		
		private IEnumerator FadeOutCoroutine(float durationSeconds)
		{
			_canvasGroup.blocksRaycasts = false;
			_canvasGroup.alpha = 1;
			while (_canvasGroup.alpha > 0)
			{
				_canvasGroup.alpha -= Time.deltaTime / durationSeconds;
				yield return null;
			}
			_canvasGroup.interactable = false;
		}

		public void FadeIn(float durationSeconds)
		{
			_owner.StartCoroutine(FadeInCoroutine(durationSeconds));
		}

		public void FadeOut(float durationSeconds)
		{
			_owner.StartCoroutine(FadeOutCoroutine(durationSeconds));
		}
	}
}