using TimeKnight.Core.Audio;
using UnityEngine;

namespace TimeKnight.Extensions
{
	public static class AudioExtensions
	{
		public static void SetParams(this AudioSource source, AudioClipParams audioParams)
		{
			var pitchVariance = Mathf.Abs(audioParams.PitchVariance);
			const float basePitch = 1.0f;
			var pitchMin = basePitch - pitchVariance;
			var pitchMax = basePitch + pitchVariance;
			var pitch = Random.Range(pitchMin, pitchMax);
			source.pitch = pitch;
				
			source.volume = audioParams.Volume;
		}
	}
}