using System.Collections;
using UnityEngine;

namespace TimeKnight.Utils
{
	public class CanvasGroupController
	{
		private readonly MonoBehaviour _owner;
		private readonly CanvasGroup _canvasGroup;
		
		private readonly CoWrapper _fadeInWrapper;
		private readonly CoWrapper _fadeOutWrapper;

		public CanvasGroupController(MonoBehaviour owner, CanvasGroup canvasGroup)
		{
			_owner = owner;
			_canvasGroup = canvasGroup;
			_fadeInWrapper = new CoWrapper(owner);
			_fadeOutWrapper = new CoWrapper(owner);
		}

		private IEnumerator FadeInCoroutine(float durationSeconds)
		{
			_canvasGroup.interactable = true;
			_canvasGroup.alpha = 0;
			if (durationSeconds <= 0)
			{
				_canvasGroup.alpha = 1;
			}
			else
			{
				while (_canvasGroup.alpha < 1)
				{
					_canvasGroup.alpha += Time.deltaTime / durationSeconds;
					yield return null;
				}
			}
			_canvasGroup.blocksRaycasts = true;
		}
		
		private IEnumerator FadeOutCoroutine(float durationSeconds)
		{
			_canvasGroup.blocksRaycasts = false;
			_canvasGroup.alpha = 1;
			if (durationSeconds <= 0)
			{
				_canvasGroup.alpha = 0;
			}
			else
			{
				while (_canvasGroup.alpha > 0)
				{
					_canvasGroup.alpha -= Time.deltaTime / durationSeconds;
					yield return null;
				}
			}
			_canvasGroup.interactable = false;
		}

		public void FadeIn(float durationSeconds)
		{
			if (_fadeOutWrapper.IsRunning) _fadeOutWrapper.Stop();
			_fadeInWrapper.Start(FadeInCoroutine(durationSeconds));
		}

		public void FadeOut(float durationSeconds)
		{
			if (_fadeInWrapper.IsRunning) _fadeInWrapper.Stop();
			_owner.StartCoroutine(FadeOutCoroutine(durationSeconds));
		}
	}
}