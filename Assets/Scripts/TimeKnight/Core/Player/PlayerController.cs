using TimeKnight.Core.Input;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeKnight.Core.Player
{
	public class PlayerController : MonoBehaviour
	{
		[SerializeField] private InputReader input = null!;
		[SerializeField] private PlayerHorizontalMovement horizontalMovement = null!;
		[SerializeField] private PlayerJumpController jump = null!;

		private void OnValidate()
		{
			Validation.NotNull(this, input, nameof(input));
			Validation.NotNull(this, horizontalMovement, nameof(horizontalMovement));
			Validation.NotNull(this, jump, nameof(jump));
		}

		private void OnEnable()
		{
			input.Actions.Player.HorizontalMove.started   += OnHorizontalMoveStarted;
			input.Actions.Player.HorizontalMove.performed += OnHorizontalMovePerformed;
			input.Actions.Player.HorizontalMove.canceled  += OnHorizontalMoveCanceled;
			
			input.Actions.Player.Jump.started  += OnJumpStarted;
			input.Actions.Player.Jump.canceled += OnJumpCanceled;
		}

		private void OnDisable()
		{
			input.Actions.Player.HorizontalMove.started   -= OnHorizontalMoveStarted;
			input.Actions.Player.HorizontalMove.performed -= OnHorizontalMovePerformed;
			input.Actions.Player.HorizontalMove.canceled  -= OnHorizontalMoveCanceled;
			
			input.Actions.Player.Jump.started  -= OnJumpStarted;
			input.Actions.Player.Jump.canceled -= OnJumpCanceled;
		}

		#region Horizontal Movement
		
		private void OnHorizontalMoveStarted(InputAction.CallbackContext ctx)
		{
			horizontalMovement.StartMove(ctx.ReadValue<float>());
		}

		private void OnHorizontalMovePerformed(InputAction.CallbackContext ctx)
		{
			horizontalMovement.UpdateSpriteDirection(ctx.ReadValue<float>());
		}

		private void OnHorizontalMoveCanceled(InputAction.CallbackContext _)
		{
			horizontalMovement.StopMove();
		}
		
		#endregion
		
		#region Jump

		private void OnJumpStarted(InputAction.CallbackContext ctx)
		{
			jump.StartJump();
		}

		private void OnJumpCanceled(InputAction.CallbackContext ctx)
		{
			jump.StopJump();
		}
		
		#endregion
	}
}