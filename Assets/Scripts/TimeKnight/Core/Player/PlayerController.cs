using TimeKnight.Core.Input;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeKnight.Core.Player
{
	[RequireComponent(typeof(PlayerAnimator))]
	public class PlayerController : MonoBehaviour
	{
		private PlayerAnimator _animator = null!;
		
		[Header("Input Reader")]
		[SerializeField] private InputReader input = null!;
		
		[Header("Movement")]
		[SerializeField] private PlayerHorizontalMovement horizontalMovement = null!;
		[SerializeField] private PlayerJumpMovement jumpMovement = null!;
		[SerializeField] private PlayerGrapplingHookMovement grapplingHookMovement = null!;

		[Header("Attack")]
		[SerializeField] private Sword.Sword sword = null!;
		
		private void OnValidate()
		{
			Validation.NotNull(this, input, nameof(input));
			Validation.NotNull(this, horizontalMovement, nameof(horizontalMovement));
			Validation.NotNull(this, jumpMovement, nameof(jumpMovement));
			Validation.NotNull(this, grapplingHookMovement, nameof(grapplingHookMovement));
			Validation.NotNull(this, sword, nameof(sword));
		}

		private void Awake()
		{
			_animator = GetComponent<PlayerAnimator>();
		}

		private void OnEnable()
		{
			input.Actions.Gameplay.MoveHorizontal.started   += OnHorizontalMoveStarted;
			input.Actions.Gameplay.MoveHorizontal.performed += OnHorizontalMovePerformed;
			input.Actions.Gameplay.MoveHorizontal.canceled  += OnHorizontalMoveCanceled;
			
			input.Actions.Gameplay.MoveJump.started  += OnJumpStarted;
			input.Actions.Gameplay.MoveJump.canceled += OnJumpCanceled;

			input.Actions.Gameplay.Attack.started += OnAttackStarted;
			
			input.Actions.Gameplay.GrappleFire.started  += OnGrappleFireStarted;
			input.Actions.Gameplay.GrappleStop.started  += OnGrappleStopStarted;
			input.Actions.Gameplay.GrappleStop.canceled += OnGrappleStopCancelled;
			grapplingHookMovement.OnGrappleEnterIdle  += OnGrappleEnterIdle;
			grapplingHookMovement.OnGrappleExitIdle   += OnGrappleExitIdle;
			grapplingHookMovement.OnGrappleEnterStuck += OnGrappleEnterStuck;
		}

		private void OnDisable()
		{
			input.Actions.Gameplay.MoveHorizontal.started   -= OnHorizontalMoveStarted;
			input.Actions.Gameplay.MoveHorizontal.performed -= OnHorizontalMovePerformed;
			input.Actions.Gameplay.MoveHorizontal.canceled  -= OnHorizontalMoveCanceled;
			
			input.Actions.Gameplay.MoveJump.started  -= OnJumpStarted;
			input.Actions.Gameplay.MoveJump.canceled -= OnJumpCanceled;
			
			input.Actions.Gameplay.Attack.started -= OnAttackStarted;
			
			input.Actions.Gameplay.GrappleFire.started  -= OnGrappleFireStarted;
			input.Actions.Gameplay.GrappleStop.started  -= OnGrappleStopStarted;
			input.Actions.Gameplay.GrappleStop.canceled -= OnGrappleStopCancelled;
			grapplingHookMovement.OnGrappleEnterIdle  -= OnGrappleEnterIdle;
			grapplingHookMovement.OnGrappleExitIdle   -= OnGrappleExitIdle;
			grapplingHookMovement.OnGrappleEnterStuck -= OnGrappleEnterStuck;
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
			jumpMovement.StartJump(checkGround: true);
		}

		private void OnJumpCanceled(InputAction.CallbackContext ctx)
		{
			jumpMovement.StopJump();
		}
		
		#endregion
		
		#region Attack

		private void OnAttackStarted(InputAction.CallbackContext _)
		{
			if (!sword.CanAttack()) return;
			_animator.SetTrigger(_animator.AttackTriggerHash);
		}
		
		#endregion
		
		#region GrapplingHook

		private void OnGrappleFireStarted(InputAction.CallbackContext _)
		{
			grapplingHookMovement.StartGrappling();
		}

		private void OnGrappleStopStarted(InputAction.CallbackContext _)
		{
			grapplingHookMovement.StopGrappling();
			input.SetActionStatus(InputStatus.Enabled, GameplayActions.Move);
			jumpMovement.StartJump(checkGround: false);
		}

		private void OnGrappleStopCancelled(InputAction.CallbackContext _)
		{
			input.SetActionStatus(InputStatus.Disabled, GameplayActions.GrappleStop);
			jumpMovement.StopJump();
		}

		private void OnGrappleEnterIdle()
		{
			input.SetActionStatus(InputStatus.Enabled, GameplayActions.GrappleFire);
		}

		private void OnGrappleExitIdle()
		{
			input.SetActionStatus(InputStatus.Disabled, GameplayActions.GrappleFire);
		}

		private void OnGrappleEnterStuck(Vector3 _)
		{
			input.SetActionStatus(InputStatus.Disabled, GameplayActions.Move);
			input.SetActionStatus(InputStatus.Enabled, GameplayActions.GrappleStop); 
		}
		
		#endregion
	}
}