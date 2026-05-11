using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.Player
{
	[RequireComponent(typeof(Animator))]
	public class PlayerAnimator : MonoBehaviour
	{
		private Animator _animator = null!;
		public readonly int AttackTriggerHash = Animator.StringToHash("Attack");
		public readonly int JumpTriggerHash = Animator.StringToHash("Jump");
		public readonly int RunningBoolHash = Animator.StringToHash("Running");
		public readonly int GroundedBoolHash = Animator.StringToHash("Grounded");
		public readonly int CloseToGrappleWallTriggerHash = Animator.StringToHash("CloseToGrappleWall");
		public readonly int CloseToGrappleFloorTriggerHash = Animator.StringToHash("CloseToGrappleFloor");
		public readonly int GrappleTriggerHash = Animator.StringToHash("Grapple");
		public readonly int EndGrappleTriggerHash = Animator.StringToHash("EndGrapple");
		public readonly int HurtTriggerHash = Animator.StringToHash("Hurt");

		[SerializeField] private Sword.Sword sword = null!;

		private void OnValidate()
		{
			Validation.NotNull(this, sword, nameof(sword));
		}

		private void Awake()
		{
			_animator = GetComponent<Animator>();
		}

		public void ResetTrigger(int hash)
		{
			_animator.ResetTrigger(hash);
		}

		public void SetTrigger(int hash)
		{
			_animator.SetTrigger(hash);
		}

		public void SetBool(int hash, bool value)
		{
			_animator.SetBool(hash, value);
		}

		// Called in the AnimationClip
		private void OnAttackAnimationBegin()
		{
			sword.BeginSwing();
		}

		// Called in the AnimationClip
		private void OnAttackAnimationEnd()
		{
			sword.EndSwing();
		}
	}
}