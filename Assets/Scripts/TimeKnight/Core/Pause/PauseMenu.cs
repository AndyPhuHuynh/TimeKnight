using TimeKnight.Core.Audio;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace TimeKnight.Core.Pause
{
	public class PauseMenu : MonoBehaviour
	{
		[Header("Volume Controls")]
		[SerializeField] private Slider masterSlider = null!;
		[SerializeField] private Slider soundFXSlider = null!;
		[SerializeField] private Slider musicSlider = null!;

		[Header("Back Button")]
		[SerializeField] private Button backButton = null!;
		
		private void OnValidate()
		{
			Validation.NotNull(this, masterSlider, nameof(masterSlider));
			Validation.NotNull(this, soundFXSlider, nameof(soundFXSlider));
			Validation.NotNull(this, musicSlider, nameof(musicSlider));
			Validation.NotNull(this, backButton, nameof(backButton));
		}

		private void Awake()
		{
			masterSlider.onValueChanged.AddListener(AudioMixerManager.Instance.SetMasterVolume);
			soundFXSlider.onValueChanged.AddListener(AudioMixerManager.Instance.SetSoundFXVolume);
			musicSlider.onValueChanged.AddListener(AudioMixerManager.Instance.SetMusicVolume);
			backButton.onClick.AddListener(CloseMenu);
		}

		public void OpenMenu()
		{
			Debug.Log("Opening menu");
			gameObject.SetActive(true);
			masterSlider.value = AudioMixerManager.Instance.GetMasterVolume();
			soundFXSlider.value = AudioMixerManager.Instance.GetSoundFXVolume();
			musicSlider.value = AudioMixerManager.Instance.GetMusicVolume();
		}

		public void CloseMenu()
		{
			gameObject.SetActive(false);
		}
	}
}