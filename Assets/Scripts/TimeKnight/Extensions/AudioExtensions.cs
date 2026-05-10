using TimeKnight.Core.Audio;
using Unity.VisualScripting;
using UnityEngine;

namespace TimeKnight.Extensions
{
	public static class AudioExtensions
	{
		public static void SetParams(this AudioSource source, AudioClipParams? audioParams)
		{
			if (audioParams == null) return;
			
			var pitchVariance = Mathf.Abs(audioParams.PitchVariance);
			const float basePitch = 1.0f;
			var pitchMin = basePitch - pitchVariance;
			var pitchMax = basePitch + pitchVariance;
			var pitch = Random.Range(pitchMin, pitchMax);
			source.pitch = pitch;
				
			source.volume = audioParams.Volume;
		}

		public static void PlayWithParams(this AudioSource source, AudioClip clip, AudioClipParams? audioParams)
		{
			source.clip = clip;
			source.SetParams(audioParams);
			source.Play();
		}
	}
}