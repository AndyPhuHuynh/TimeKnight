using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.Player
{
	[RequireComponent(typeof(Animator))]
	public class PlayerAnimator : MonoBehaviour
	{
		private Animator _animator = null!;
		public readonly int AttackTriggerHash = Animator.StringToHash("Attack");

		[SerializeField] private Sword.Sword sword = null!;

		private void OnValidate()
		{
			Validation.NotNull(this, sword, nameof(sword));
		}

		private void Awake()
		{
			_animator = GetComponent<Animator>();
		}

		public void SetTrigger(int hash)
		{
			_animator.SetTrigger(hash);
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