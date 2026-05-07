using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.Audio
{
	public class AudioManager : Singleton<AudioManager>
	{
		[SerializeField] private AudioSource soundFXSource = null!;
		[SerializeField] private AudioSource musicSource = null!;
		
		private void OnValidate()
		{
			Validation.NotNull(this, soundFXSource, nameof(soundFXSource));
			Validation.NotNull(this, musicSource, nameof(musicSource));
		}

		public void PlayClip(AudioClip clip)
		{
			musicSource.clip = clip;
			musicSource.Play();
		}
	}
}