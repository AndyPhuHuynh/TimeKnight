using TimeKnight.Core.Input;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeKnight.Core.Pause
{
	[RequireComponent(typeof(PauseMenu))]
	public class PauseController : MonoBehaviour
	{
		private PauseMenu _pauseMenu = null!;
		[SerializeField] private InputReader input = null!;
		
		private void OnValidate()
		{
			Validation.NotNull(this, input, nameof(input));
		}

		private void Awake()
		{
			_pauseMenu = GetComponent<PauseMenu>();
			Validation.NotFound(this, _pauseMenu, nameof(_pauseMenu));
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
			_pauseMenu.OpenMenu();
			input.SetActionStatus(InputStatus.Disabled, GlobalActions.OpenPause);
			input.SetActionStatus(InputStatus.Enabled, GlobalActions.ClosePause);
		}
		
		private void OnClosePauseStarted(InputAction.CallbackContext ctx)
		{
			_pauseMenu.CloseMenu();
			input.SetActionStatus(InputStatus.Disabled, GlobalActions.ClosePause);
			input.SetActionStatus(InputStatus.Enabled, GlobalActions.OpenPause);
		}
	}
}