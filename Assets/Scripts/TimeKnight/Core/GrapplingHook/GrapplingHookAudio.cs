using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.GrapplingHook
{
	[RequireComponent(typeof(GrapplingHook))]
	public class GrapplingHookAudio : MonoBehaviour
	{
		private GrapplingHook _grapplingHook = null!;

		[Header("Audio Source")]
		[SerializeField] private AudioSource audioSource = null!;
		
		[Header("Audio Clips")]
		[SerializeField] private AudioClip grappleChainClip = null!;
		[SerializeField] private AudioClip grappleHitClip = null!;

		private void OnValidate()
		{
			Validation.NotNull(this, audioSource, nameof(audioSource));
			Validation.NotNull(this, grappleChainClip, nameof(grappleChainClip));
			Validation.NotNull(this, grappleHitClip, nameof(grappleHitClip));
		}

		private void Awake()
		{
			_grapplingHook = GetComponent<GrapplingHook>();
			audioSource.clip = grappleChainClip;
			audioSource.loop = true;
			audioSource.volume = 0.5f;
		}

		private void OnEnable()
		{
			_grapplingHook.OnEnterIdle  += OnEnterIdle;
			_grapplingHook.OnExitIdle   += OnExitIdle;
			_grapplingHook.OnEnterStuck += OnEnterStuck;
		}
		
		private void OnDisable()
		{
			_grapplingHook.OnEnterIdle  -= OnEnterIdle;
			_grapplingHook.OnExitIdle   -= OnExitIdle;
			_grapplingHook.OnEnterStuck -= OnEnterStuck;
		}

		private void OnEnterIdle()
		{
			audioSource.Stop();
		}

		private void OnExitIdle()
		{
			audioSource.Play();
		}

		private void OnEnterStuck(Vector3 _)
		{
			audioSource.Stop();
			audioSource.PlayOneShot(grappleHitClip);
		}
	}
}