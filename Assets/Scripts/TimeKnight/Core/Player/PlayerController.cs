using TimeKnight.Core.GrapplingHook;
using TimeKnight.Core.Input;
using TimeKnight.Core.TimePower;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeKnight.Core.Player
{
	[RequireComponent(typeof(PlayerAnimator))]
	public class PlayerController : MonoBehaviour
	{
		private PlayerAnimator _animator = null!;
		private static Transform _playerTransform = null!;
		public static Vector3 PlayerPosition => _playerTransform.position;
		
		[Header("Input Reader")]
		[SerializeField] private InputReader input = null!;
		
		[Header("Movement")]
		[SerializeField] private PlayerHorizontalMovement horizontalMovement = null!;
		[SerializeField] private PlayerJumpMovement jumpMovement = null!;
		[SerializeField] private PlayerGrapplingHookMovement grapplingHookMovement = null!;

		[Header("Combat")]
		[SerializeField] private Sword.Sword sword = null!;
		[SerializeField] private TimeManager timeManager = null!;
		[SerializeField] private PlayerCombatManager playerManager = null!;
		
		private void OnValidate()
		{
			Validation.NotNull(this, input, nameof(input));
			Validation.NotNull(this, horizontalMovement, nameof(horizontalMovement));
			Validation.NotNull(this, jumpMovement, nameof(jumpMovement));
			Validation.NotNull(this, grapplingHookMovement, nameof(grapplingHookMovement));
			Validation.NotNull(this, sword, nameof(sword));
			Validation.NotNull(this, timeManager, nameof(timeManager));
			Validation.NotNull(this, playerManager, nameof(playerManager));
		}

		private void Awake()
		{
			_playerTransform = transform;
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
			input.Actions.Gameplay.SlowTime.started += OnSlowTimeStarted;
			
			input.Actions.Gameplay.GrappleFire.started  += OnGrappleFireStarted;
			input.Actions.Gameplay.GrappleStop.started  += OnGrappleStopStarted;
			input.Actions.Gameplay.GrappleStop.canceled += OnGrappleStopCancelled;
			grapplingHookMovement.OnGrappleEnterIdle  += OnGrappleEnterIdle;
			grapplingHookMovement.OnGrappleExitIdle   += OnGrappleExitIdle;
			grapplingHookMovement.OnGrappleEnterStuck += OnGrappleEnterStuck;
			grapplingHookMovement.OnGrappleUpdateStuck += OnGrappleUpdateStuck;
			sword.OnSwordSwingEnd += OnAttackEnd;

			playerManager.OnPlayerStunBegin += OnPlayerStunBegin;
			playerManager.OnPlayerStunEnd += OnPlayerStunEnd;
		}

		private void OnDisable()
		{
			input.Actions.Gameplay.MoveHorizontal.started   -= OnHorizontalMoveStarted;
			input.Actions.Gameplay.MoveHorizontal.performed -= OnHorizontalMovePerformed;
			input.Actions.Gameplay.MoveHorizontal.canceled  -= OnHorizontalMoveCanceled;
			
			input.Actions.Gameplay.MoveJump.started  -= OnJumpStarted;
			input.Actions.Gameplay.MoveJump.canceled -= OnJumpCanceled;
			
			input.Actions.Gameplay.Attack.started -= OnAttackStarted;
			input.Actions.Gameplay.SlowTime.started -= OnSlowTimeStarted;
			
			input.Actions.Gameplay.GrappleFire.started  -= OnGrappleFireStarted;
			input.Actions.Gameplay.GrappleStop.started  -= OnGrappleStopStarted;
			input.Actions.Gameplay.GrappleStop.canceled -= OnGrappleStopCancelled;
			grapplingHookMovement.OnGrappleEnterIdle  -= OnGrappleEnterIdle;
			grapplingHookMovement.OnGrappleExitIdle   -= OnGrappleExitIdle;
			grapplingHookMovement.OnGrappleEnterStuck -= OnGrappleEnterStuck;
			grapplingHookMovement.OnGrappleUpdateStuck -= OnGrappleUpdateStuck;
			sword.OnSwordSwingEnd -= OnAttackEnd;

			playerManager.OnPlayerStunBegin -= OnPlayerStunBegin;
			playerManager.OnPlayerStunEnd -= OnPlayerStunEnd;
		}

		#region Horizontal Movement
		
		private void OnHorizontalMoveStarted(InputAction.CallbackContext ctx)
		{
			_animator.SetBool(_animator.RunningBoolHash, true);
			// This lambda is needed because if player changes directions on the same frame OnHorizontalMoveCanceled doesn't get called.
			horizontalMovement.StartMove(() => ctx.ReadValue<float>());
		}

		private void OnHorizontalMovePerformed(InputAction.CallbackContext ctx)
		{
			horizontalMovement.UpdateSpriteDirection(ctx.ReadValue<float>());
		}

		private void OnHorizontalMoveCanceled(InputAction.CallbackContext _)
		{
			_animator.SetBool(_animator.RunningBoolHash, false);
			horizontalMovement.StopMove();
		}
		
		#endregion	
		
		#region Jump

		private void OnJumpStarted(InputAction.CallbackContext ctx)
		{
			jumpMovement.StartJump(checkGround: true, () => _animator.SetTrigger(_animator.JumpTriggerHash));
		}

		private void OnJumpCanceled(InputAction.CallbackContext ctx)
		{
			jumpMovement.StopJump();
		}
		
		#endregion
		
		#region Combat

		private void OnAttackStarted(InputAction.CallbackContext _)
		{
			if (!sword.CanAttack()) return;
			_animator.SetTrigger(_animator.AttackTriggerHash);
			input.SetActionStatus(InputStatus.Disabled, GameplayActions.GrappleFire);
			input.SetActionStatus(InputStatus.Disabled, GameplayActions.Move);
			input.SetActionStatus(InputStatus.Disabled, GameplayActions.Attack);
			input.SetActionStatus(InputStatus.Disabled, GameplayActions.InteractionInteract);
			input.SetActionStatus(InputStatus.Disabled, GameplayActions.InteractionNavigate);
		}

		private void OnAttackEnd()
		{
			input.SetActionStatus(InputStatus.Enabled, GameplayActions.GrappleFire);
			input.SetActionStatus(InputStatus.Enabled, GameplayActions.Move);
			input.SetActionStatus(InputStatus.Enabled, GameplayActions.Attack);
			input.SetActionStatus(InputStatus.Enabled, GameplayActions.InteractionInteract);
			input.SetActionStatus(InputStatus.Enabled, GameplayActions.InteractionNavigate);
		}

		private void OnSlowTimeStarted(InputAction.CallbackContext _)
		{
			timeManager.ActivateSlowTime();
		}

		private void OnPlayerStunBegin()
		{
			input.SetMapStatus(InputStatus.Disabled, ActionMaps.Gameplay);
			// Interrupt grappling hook if it's being preformed.
			if (grapplingHookMovement.HookState.IsStuck() || grapplingHookMovement.HookState.IsExtending())
			{
				grapplingHookMovement.InterruptGrapple();
			}
		}

		private void OnPlayerStunEnd()
		{	
			input.SetMapStatus(InputStatus.Enabled, ActionMaps.Gameplay);
			input.SetActionStatus(InputStatus.Disabled, GameplayActions.GrappleStop);	// Disable grapple stop because we know at this point the grappling hook is either idle or retracting.
			// Only enable GrappleFire if the hook is done retracting.
			if (grapplingHookMovement.HookState.IsIdle())
			{
				input.SetActionStatus(InputStatus.Enabled, GameplayActions.GrappleFire);
			}
			else
			{
				input.SetActionStatus(InputStatus.Disabled, GameplayActions.GrappleFire);
			}
		}
		
		#endregion
		
		#region GrapplingHook
		
		private void OnGrappleFireStarted(InputAction.CallbackContext _)
		{
			// Prevent player from interacting with NPC or attacking while grappling.
			input.SetActionStatus(InputStatus.Disabled, GameplayActions.InteractionInteract);
			input.SetActionStatus(InputStatus.Disabled, GameplayActions.InteractionNavigate);
			input.SetActionStatus(InputStatus.Disabled, GameplayActions.Attack);
			_animator.ResetTrigger(_animator.EndGrappleTriggerHash);
			_animator.ResetTrigger(_animator.CloseToGrappleWallTriggerHash);
            _animator.ResetTrigger(_animator.CloseToGrappleFloorTriggerHash);
			grapplingHookMovement.StartGrappling();
		}

		private void OnGrappleStopStarted(InputAction.CallbackContext _)
		{
			grapplingHookMovement.StopGrappling();
			input.SetActionStatus(InputStatus.Enabled, GameplayActions.Move);
			jumpMovement.StartJump(checkGround: false, () => _animator.SetTrigger(_animator.EndGrappleTriggerHash));
		}

		private void OnGrappleStopCancelled(InputAction.CallbackContext _)
		{
			jumpMovement.StopJump();
			
			if (grapplingHookMovement.HookState.IsStuck()) return;
			input.SetActionStatus(InputStatus.Disabled, GameplayActions.GrappleStop);
		}

		private void OnGrappleEnterIdle()
		{
			// Resume NPC interaction when grappling hook is finished.
			input.SetActionStatus(InputStatus.Enabled, GameplayActions.InteractionInteract);
			input.SetActionStatus(InputStatus.Enabled, GameplayActions.InteractionNavigate);
			input.SetActionStatus(InputStatus.Enabled, GameplayActions.GrappleFire);
			input.SetActionStatus(InputStatus.Enabled, GameplayActions.Attack);
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

		private void OnGrappleUpdateStuck()
		{
			input.SetActionStatus(InputStatus.Enabled, GameplayActions.GrappleStop);
		}
		
		#endregion
	}
}