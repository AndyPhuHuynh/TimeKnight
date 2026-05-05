using TimeKnight.Core.Input;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeKnight.Core.Pause
{
	public class PauseController : MonoBehaviour
	{
		[SerializeField] private InputReader input = null!;
		[SerializeField] private PauseMenu pauseMenu = null!;
		
		private void OnValidate()
		{
			Validation.NotNull(this, input, nameof(input));
			Validation.NotNull(this, pauseMenu, nameof(pauseMenu));
		}

		private void OnEnable()
		{
			input.Actions.Global.OpenPauseMenu.started  += OnOpenPauseStarted;
			input.Actions.Global.ClosePauseMenu.started += OnClosePauseStarted;
		}

		private void OnDisable()
		{
			input.Actions.Global.OpenPauseMenu.started  -= OnOpenPauseStarted;
			input.Actions.Global.ClosePauseMenu.started -= OnClosePauseStarted;
		}

		private void OnOpenPauseStarted(InputAction.CallbackContext ctx)
		{
			pauseMenu.OpenMenu();
			input.SetActionStatus(InputStatus.Disabled, GlobalActions.OpenPause);
			input.SetActionStatus(InputStatus.Enabled, GlobalActions.ClosePause);
		}
		
		private void OnClosePauseStarted(InputAction.CallbackContext ctx)
		{
			pauseMenu.CloseMenu();
			input.SetActionStatus(InputStatus.Disabled, GlobalActions.ClosePause);
			input.SetActionStatus(InputStatus.Enabled, GlobalActions.OpenPause);
		}
	}
}