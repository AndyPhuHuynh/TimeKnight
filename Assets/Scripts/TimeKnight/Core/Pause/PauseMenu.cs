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
	}
}