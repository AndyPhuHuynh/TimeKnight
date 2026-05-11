using TimeKnight.Core.Player;
using UnityEngine;

namespace TimeKnight.Core
{
    public class GroundCheck : MonoBehaviour
    {
        public bool IsGrounded { get; private set; }
        [SerializeField] PlayerAnimator? playerAnimator;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if ("Ground" == LayerMask.LayerToName(collision.gameObject.layer))
            {
                IsGrounded = true;
                UpdatePlayerAnimator();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if ("Ground" == LayerMask.LayerToName(collision.gameObject.layer))
            {
                IsGrounded = false;
                UpdatePlayerAnimator();
            }
        }

        private void UpdatePlayerAnimator()
        {
            if (playerAnimator != null)
            {
                playerAnimator.SetBool(playerAnimator.GroundedBoolHash, IsGrounded);
            }
        }
    }
}
