using TimeKnight.Core.Audio;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace TimeKnight.Core.Pause
{
	public class PauseMenu : MonoBehaviour
	{
		private CanvasGroup _cg = null!;
		
		[Header("Volume Controls")]
		[SerializeField] private Slider masterSlider = null!;
		[SerializeField] private Slider soundFXSlider = null!;
		[SerializeField] private Slider musicSlider = null!;

		[Header("Back Button")]
		[SerializeField] private Button backButton = null!;

		private CanvasGroupController _cgController = null!;
		
		private void OnValidate()
		{
			Validation.NotNull(this, masterSlider, nameof(masterSlider));
			Validation.NotNull(this, soundFXSlider, nameof(soundFXSlider));
			Validation.NotNull(this, musicSlider, nameof(musicSlider));
			Validation.NotNull(this, backButton, nameof(backButton));
		}

		private void Awake()
		{
			_cg = GetComponent<CanvasGroup>();
			_cgController = new CanvasGroupController(this, _cg);
			Validation.NotFound(this, _cg, nameof(_cg));
			
			backButton.onClick.AddListener(CloseMenu);
		}

		private void Start()
		{
			masterSlider.onValueChanged.AddListener(AudioMixerManager.Instance.SetMasterVolume);
			soundFXSlider.onValueChanged.AddListener(AudioMixerManager.Instance.SetSoundFXVolume);
			musicSlider.onValueChanged.AddListener(AudioMixerManager.Instance.SetMusicVolume);
		}

		public void OpenMenu()
		{
			masterSlider.value = AudioMixerManager.Instance.GetMasterVolume();
			soundFXSlider.value = AudioMixerManager.Instance.GetSoundFXVolume();
			musicSlider.value = AudioMixerManager.Instance.GetMusicVolume();
			_cgController.FadeIn(0.2f);
		}

		public void CloseMenu()
		{
			_cgController.FadeOut(0.2f);
		}
	}
}