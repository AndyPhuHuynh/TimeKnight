using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.Audio
{
	public class AudioTrigger : MonoBehaviour
	{
		[SerializeField] private AudioClip clip = null!;

		private void OnValidate()
		{
			Validation.NotNull(this, clip, nameof(clip));
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (!collision.CompareTag("Player")) return;
			AudioManager.Instance.FadeInMusic(clip);	
		}
	}
}