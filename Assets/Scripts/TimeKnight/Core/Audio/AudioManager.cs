using System.Collections;
using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.Audio
{
	public class AudioManager : Singleton<AudioManager>
	{
		[Header("SFX")]
		[SerializeField] private AudioSource soundFXSource = null!;
		
		[Header("Music")]
		[SerializeField] private AudioSource musicSource1 = null!;
		[SerializeField] private AudioSource musicSource2 = null!;

		private AudioSource _current = null!;
		private AudioSource _other = null!;

		private CoWrapper _fadeIn = null!;
		private CoWrapper _fadeOut = null!;

		private const float FadeInSeconds = 5.0f;
		private const float FadeOutSeconds = 2.0f;
		
		private void OnValidate()
		{
			Validation.NotNull(this, soundFXSource, nameof(soundFXSource));
			Validation.NotNull(this, musicSource1, nameof(musicSource1));
			Validation.NotNull(this, musicSource2, nameof(musicSource2));
		}

		protected override void Awake()
		{
			base.Awake();
			if (Instance != this) return;
			
			_current = musicSource1;
			_other = musicSource2;

			_current.volume = 0.0f;
			_other.volume = 0.0f;

			_fadeIn = new CoWrapper(this);
			_fadeOut = new CoWrapper(this);
		}

		private bool IsPlayingMusic()
		{
			return _current.isPlaying && _current.clip != null;
		}

		private static IEnumerator FadeInMusicCoroutine(AudioSource source, AudioClip clip)
		{
			source.clip = clip;
			source.Play();
			
			var time = 0.0f;
			while (time < FadeInSeconds)
			{
				time += Time.deltaTime;
				source.volume = Mathf.Lerp(0, 1, time / FadeInSeconds);
				yield return null;
			}
		}

		private static IEnumerator FadeOutMusicCoroutine(AudioSource source)
		{
			var time = 0.0f;
			while (time < FadeOutSeconds)
			{
				time += Time.deltaTime;
				source.volume = Mathf.Lerp(1, 0, time / FadeOutSeconds);
				yield return null;
			}

			source.Stop();
			source.clip = null;
		}

		public void FadeInMusic(AudioClip clip)
		{
			if (!IsPlayingMusic())
			{
				_fadeIn.Start(FadeInMusicCoroutine(_current, clip));
				return;
			}
			(_current, _other) = (_other, _current);
			_fadeIn.Start(FadeInMusicCoroutine(_current, clip));
			_fadeOut.Start(FadeOutMusicCoroutine(_other));
		}
		
		public void PlaySoundEffect(AudioClip clip)
		{
			soundFXSource.clip = clip;
			soundFXSource.Play();
		}
	}
}