using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.Audio;

namespace TimeKnight.Core.Audio
{
	public class AudioMixerManager : Singleton<AudioMixerManager>
	{
		private const string MasterVolume  = "MasterVolume";
		private const string SoundFXVolume = "SoundFXVolume";
		private const string MusicVolume   = "MusicVolume";

		[SerializeField] private AudioMixer audioMixer = null!;

		private void OnValidate()
		{
			Validation.NotNull(this, audioMixer, nameof(audioMixer));
		}

		public float GetMasterVolume()
		{
			audioMixer.GetFloat(MasterVolume, out var value);
			return Mathf.Pow(10, value / 20);
		}
		
		public float GetSoundFXVolume()
		{
			audioMixer.GetFloat(SoundFXVolume, out var value);
			return Mathf.Pow(10, value / 20);
		}
		
		public float GetMusicVolume()
		{
			audioMixer.GetFloat(MusicVolume, out var value);
			return Mathf.Pow(10, value / 20);
		}

		public void SetMasterVolume(float level)
		{
			audioMixer.SetFloat(MasterVolume, Mathf.Log10(level) * 20);
		}

		public void SetSoundFXVolume(float level)
		{
			audioMixer.SetFloat(SoundFXVolume, Mathf.Log10(level) * 20);
		}

		public void SetMusicVolume(float level)
		{
			audioMixer.SetFloat(MusicVolume, Mathf.Log10(level) * 20);
		}
	}
}